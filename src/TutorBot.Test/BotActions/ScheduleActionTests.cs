using Moq;
using Moq.Protected;
using Shouldly;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;
using TutorBot.TelegramService;
using TutorBot.TelegramService.BotActions;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.Test.BotActions;

[Trait("Category", "Unit")]
public class ScheduleActionTests : IDisposable
{
    private readonly Mock<ITelegramBot> _botMock = new();
    private readonly Mock<IApplication> _appMock = new();
    private readonly Mock<IChatService> _chatServiceMock = new();
    private readonly Mock<IHistoryService> _historyServiceMock = new();
    private readonly Mock<IALServiceService> _alServiceMock = new();

    private readonly TgBotServiceOptions _options = new()
    {
        Enable = true,
        Token = "test-token",
        DialogModelPath = "test.json",
        EvaluateKey = "eval-key"
    };

    private readonly HttpClient _originalClient;
    private readonly string _originalBaseUrl;

    public ScheduleActionTests()
    {
        _originalClient = ScheduleAction.Client;
        _originalBaseUrl = ScheduleAction.BaseUrl;

        _appMock.Setup(x => x.ChatService).Returns(_chatServiceMock.Object);
        _appMock.Setup(x => x.HistoryService).Returns(_historyServiceMock.Object);
        _appMock.Setup(x => x.ALService).Returns(_alServiceMock.Object);
        _historyServiceMock.Setup(x => x.AddHistory(It.IsAny<MessageHistory>())).Returns(Task.CompletedTask);
        _chatServiceMock.Setup(x => x.Update(It.IsAny<ChatEntry>())).Returns(Task.CompletedTask);
        _chatServiceMock.Setup(x => x.Find(-1)).ReturnsAsync((ChatEntry?)null);
        _chatServiceMock.Setup(x => x.Create(-1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), -1))
            .ReturnsAsync(new ChatEntry { ID = -1, ChatID = -1, UserID = -1, FullName = "Error Service" });
        _botMock.Setup(x => x.SendMessage(It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<ParseMode>(),
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message());
    }

    public void Dispose()
    {
        ScheduleAction.Client = _originalClient;
        ScheduleAction.BaseUrl = _originalBaseUrl;
    }

    private TutorBotContext CreateContext(Action<ChatEntry>? configure = null)
    {
        TutorBotContext context = new TutorBotContext(_botMock.Object, _options, _appMock.Object, 12345, CancellationToken.None);
        ChatEntry chatEntry = new ChatEntry
        {
            ID = 1,
            ChatID = 100,
            UserID = 42,
            FullName = "иванов иван иванович",
            FirstName = "Иван",
            LastName = "Иванов",
            UserName = "ivanov",
            GroupNumber = "РИ-151001",
            SessionID = Guid.NewGuid()
        };
        configure?.Invoke(chatEntry);
        context.ChatEntry = chatEntry;
        return context;
    }

    private static Message CreateMessage(string? text)
    {
        return new Message
        {
            Text = text,
            From = new User { Id = 42, FirstName = "Test" },
            Chat = new Chat { Id = 100, Type = ChatType.Private },
            Date = DateTime.Now
        };
    }

    private static DialogModel CreateModel(string[]? scheduleText = null, string[]? menuButtons = null)
    {
        return new DialogModel
        {
            Start = new StartNodeModel { Handler = "s", NextStep = "n" },
            Handlers = new HandlersModel
            {
                Welcome = new WelcomeHandler { Key = "/start", WelcomeText = "W", ErrorText = "E", GroupNumbers = ["1"] },
                Schedule = new ScheduleItem { Key = "Schedule", Text = scheduleText ?? ["Timetable for {UserName}: #URL#"] },
                SimpleText = [],
                YandexSearchText = []
            },
            Menus = [
                new MenuItem { Key = "Main", Text = "Menu", Buttons = menuButtons ?? ["Schedule"] }
            ]
        };
    }

    [Fact]
    public async Task ExecuteAsync_NoMenu_WritesError()
    {
        DialogModel model = CreateModel(menuButtons: ["OtherButton"]);
        ScheduleAction action = new ScheduleAction(model);
        TutorBotContext context = CreateContext();
        Message message = CreateMessage("Schedule");

        await action.ExecuteAsync(message, context);

        _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
            h.MessageText.Contains("not found menu"))), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_NoUrlInText_SendsSimpleText()
    {
        DialogModel model = CreateModel(scheduleText: ["Timetable for {UserName}"]);
        ScheduleAction action = new ScheduleAction(model);
        TutorBotContext context = CreateContext();
        Message message = CreateMessage("Schedule");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "Timetable for иван", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithUrlInText_CachedGroup_SendsWithUrl()
    {
        DialogModel model = CreateModel();
        ScheduleAction action = new ScheduleAction(model);
        TutorBotContext context = CreateContext();
        Message message = CreateMessage("Schedule");

        FieldInfo? field = typeof(ScheduleAction).GetField("_groupID", BindingFlags.NonPublic | BindingFlags.Instance);
        ConcurrentDictionary<string, ulong> cache = (ConcurrentDictionary<string, ulong>)field!.GetValue(action)!;
        cache["РИ-151001"] = 12345;

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "Timetable for иван: https://urfu.ru/ru/students/study/schedule/#/groups/12345", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithUrlInText_UncachedGroup_FetchesAndSends()
    {
        string jsonResponse = JsonSerializer.Serialize(new[]
        {
            new { id = 12345UL, divisionId = 0UL, course = 0UL, title = "РИ-151001" }
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });
        ScheduleAction.Client = new HttpClient(handlerMock.Object);

        ScheduleAction action = new ScheduleAction(CreateModel());
        TutorBotContext context = CreateContext();
        Message message = CreateMessage("Schedule");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "Timetable for иван: https://urfu.ru/ru/students/study/schedule/#/groups/12345", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithUrlInText_ApiError_HandlesError()
    {
        _chatServiceMock.Setup(x => x.GetChats(It.IsAny<GetChatsFilter?>()))
            .ReturnsAsync([]);

        Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("API error"));
        ScheduleAction.Client = new HttpClient(handlerMock.Object);

        ScheduleAction action = new ScheduleAction(CreateModel());
        TutorBotContext context = CreateContext();
        Message message = CreateMessage("Schedule");

        await action.ExecuteAsync(message, context);

        _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
            h.Type == MessageHistoryRole.Error)), Times.AtLeastOnce);
        _botMock.Verify(x => x.SendMessage(100, "Timetable for иван: https://urfu.ru/ru/students/study/schedule/", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithUrlInText_GroupIdZero_SendsFallbackUrl()
    {
        _chatServiceMock.Setup(x => x.GetChats(It.IsAny<GetChatsFilter?>()))
            .ReturnsAsync([]);

        string jsonResponse = JsonSerializer.Serialize(new[]
        {
            new { id = 99999UL, divisionId = 0UL, course = 0UL, title = "OTHER-GROUP" }
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });
        ScheduleAction.Client = new HttpClient(handlerMock.Object);

        ScheduleAction action = new ScheduleAction(CreateModel());
        TutorBotContext context = CreateContext();
        Message message = CreateMessage("Schedule");

        await action.ExecuteAsync(message, context);

        _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
            h.Type == MessageHistoryRole.Error)), Times.AtLeastOnce);
        _botMock.Verify(x => x.SendMessage(100, "Timetable for иван: https://urfu.ru/ru/students/study/schedule/", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGroupsAsync_Success_ReturnsGroups()
    {
        string jsonResponse = JsonSerializer.Serialize(new[]
        {
            new { id = 1UL, divisionId = 1UL, course = 1UL, title = "РИ-151001" },
            new { id = 2UL, divisionId = 2UL, course = 2UL, title = "РИ-151002" }
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });
        ScheduleAction.Client = new HttpClient(handlerMock.Object);

        ScheduleAction.GroupInfo[] result = await ScheduleAction.GetGroupsAsync("РИ-151001", TestContext.Current.CancellationToken);

        result.Length.ShouldBe(2);
        result[0].Id.ShouldBe(1UL);
        result[0].Title.ShouldBe("РИ-151001");
        result[1].Id.ShouldBe(2UL);
        result[1].Title.ShouldBe("РИ-151002");
    }

    [Fact]
    public async Task GetGroupsAsync_HttpError_Throws()
    {
        Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });
        ScheduleAction.Client = new HttpClient(handlerMock.Object);

        await Should.ThrowAsync<HttpRequestException>(() => ScheduleAction.GetGroupsAsync("РИ-151001", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetGroupsAsync_EmptyResponse_ReturnsEmpty()
    {
        string jsonResponse = "[]";

        Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });
        ScheduleAction.Client = new HttpClient(handlerMock.Object);

        ScheduleAction.GroupInfo[] result = await ScheduleAction.GetGroupsAsync("РИ-151001", TestContext.Current.CancellationToken);

        result.ShouldBeEmpty();
    }
}
