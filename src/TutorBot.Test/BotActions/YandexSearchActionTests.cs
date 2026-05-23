using Moq;
using Shouldly;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;
using TutorBot.TelegramService;
using TutorBot.TelegramService.BotActions;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.Test.BotActions;

[Trait("Category", "Unit")]
public class YandexSearchActionTests
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

    private TutorBotContext CreateContext(ChatEntry? chatEntry = null)
    {
        var context = new TutorBotContext(_botMock.Object, _options, _appMock.Object, 12345, CancellationToken.None);
        context.ChatEntry = chatEntry ?? new ChatEntry
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

    private static DialogModel CreateModelWithMenu(string key)
    {
        return new DialogModel
        {
            Start = new StartNodeModel { Handler = "start", NextStep = "menu" },
            Handlers = new HandlersModel
            {
                Welcome = new WelcomeHandler
                {
                    Key = "welcome", WelcomeText = "hello", ErrorText = "err",
                    GroupNumbers = ["101"]
                },
                Schedule = new ScheduleItem { Key = "sched", Text = ["s"] },
                SimpleText = [],
                YandexSearchText = []
            },
            Menus = [
                new MenuItem { Key = "Главное меню", Text = "меню", Buttons = [key] }
            ]
        };
    }

    public YandexSearchActionTests()
    {
        _appMock.Setup(x => x.ChatService).Returns(_chatServiceMock.Object);
        _appMock.Setup(x => x.HistoryService).Returns(_historyServiceMock.Object);
        _appMock.Setup(x => x.ALService).Returns(_alServiceMock.Object);
        _historyServiceMock.Setup(x => x.AddHistory(It.IsAny<MessageHistory>())).Returns(Task.CompletedTask);
        _chatServiceMock.Setup(x => x.Update(It.IsAny<ChatEntry>())).Returns(Task.CompletedTask);
        _botMock.Setup(x => x.SendMessage(It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<ParseMode>(),
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message());
    }

    [Fact]
    public async Task ExecuteAsync_KeyEquals_SendsDescriptionsWithMenu()
    {
        var model = CreateModelWithMenu("search");
        var item = new YandexSearchTextItem
        {
            Key = "search",
            Descriptions = "Введите запрос:",
            Text = ["результат: {Text}"]
        };
        var action = new YandexSearchAction(model, item);
        var context = CreateContext();
        var message = CreateMessage("search");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(It.IsAny<ChatId>(),
            "Введите запрос:",
            ParseMode.Html,
            It.IsAny<ReplyParameters>(),
            It.IsAny<ReplyMarkup>(),
            It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(),
            It.IsAny<IEnumerable<MessageEntity>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NullText_SendsDescriptions()
    {
        var model = CreateModelWithMenu("search");
        var item = new YandexSearchTextItem
        {
            Key = "search",
            Descriptions = "Введите запрос:",
            Text = ["результат: {Text}"]
        };
        var action = new YandexSearchAction(model, item);
        var context = CreateContext();
        var message = CreateMessage(null);

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(It.IsAny<ChatId>(),
            "Введите запрос:",
            ParseMode.Html,
            It.IsAny<ReplyParameters>(),
            It.IsAny<ReplyMarkup>(),
            It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(),
            It.IsAny<IEnumerable<MessageEntity>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyText_SendsDescriptions()
    {
        var model = CreateModelWithMenu("search");
        var item = new YandexSearchTextItem
        {
            Key = "search",
            Descriptions = "Введите запрос:",
            Text = ["результат: {Text}"]
        };
        var action = new YandexSearchAction(model, item);
        var context = CreateContext();
        var message = CreateMessage("");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(It.IsAny<ChatId>(),
            "Введите запрос:",
            ParseMode.Html,
            It.IsAny<ReplyParameters>(),
            It.IsAny<ReplyMarkup>(),
            It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(),
            It.IsAny<IEnumerable<MessageEntity>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidText_SendsReplacedText()
    {
        var model = CreateModelWithMenu("search");
        var item = new YandexSearchTextItem
        {
            Key = "search",
            Descriptions = "Введите запрос:",
            Text = ["Результат: {Text}"]
        };
        var action = new YandexSearchAction(model, item);
        var context = CreateContext();
        var message = CreateMessage("some query");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(It.IsAny<ChatId>(),
            "Результат: some query",
            ParseMode.Html,
            It.IsAny<ReplyParameters>(),
            It.IsAny<ReplyMarkup>(),
            It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(),
            It.IsAny<IEnumerable<MessageEntity>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithPattern_ValidInput_SendsText()
    {
        var model = CreateModelWithMenu("search");
        var item = new YandexSearchTextItem
        {
            Key = "search",
            Descriptions = "Введите запрос:",
            Pattern = @"^\d+$",
            InvalidPatternMessage = "Только цифры!",
            Text = ["Результат: {Text}"]
        };
        var action = new YandexSearchAction(model, item);
        var context = CreateContext();
        var message = CreateMessage("123");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(It.IsAny<ChatId>(),
            "Результат: 123",
            ParseMode.Html,
            It.IsAny<ReplyParameters>(),
            It.IsAny<ReplyMarkup>(),
            It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(),
            It.IsAny<IEnumerable<MessageEntity>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithPattern_InvalidInput_SendsError()
    {
        var model = CreateModelWithMenu("search");
        var item = new YandexSearchTextItem
        {
            Key = "search",
            Descriptions = "Введите запрос:",
            Pattern = @"^\d+$",
            InvalidPatternMessage = "Только цифры!",
            Text = ["Результат: {Text}"]
        };
        var action = new YandexSearchAction(model, item);
        var context = CreateContext();
        var message = CreateMessage("abc");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(It.IsAny<ChatId>(),
            "Только цифры!",
            ParseMode.Html,
            It.IsAny<ReplyParameters>(),
            It.IsAny<ReplyMarkup>(),
            It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(),
            It.IsAny<IEnumerable<MessageEntity>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoMenu_WritesError()
    {
        var model = new DialogModel
        {
            Start = new StartNodeModel { Handler = "start", NextStep = "menu" },
            Handlers = new HandlersModel
            {
                Welcome = new WelcomeHandler
                {
                    Key = "welcome", WelcomeText = "hello", ErrorText = "err",
                    GroupNumbers = ["101"]
                },
                Schedule = new ScheduleItem { Key = "sched", Text = ["s"] },
                SimpleText = [],
                YandexSearchText = []
            },
            Menus = []
        };
        var item = new YandexSearchTextItem
        {
            Key = "no-menu-key",
            Descriptions = "desc",
            Text = ["text"]
        };
        var action = new YandexSearchAction(model, item);
        var context = CreateContext();
        var message = CreateMessage("any");

        _chatServiceMock.Setup(x => x.Find(-1)).ReturnsAsync((ChatEntry?)null);
        _chatServiceMock.Setup(x => x.Create(-1, It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), -1)).ReturnsAsync(new ChatEntry
            {
                ID = -1, ChatID = -1, UserID = -1,
                FullName = "Error Service"
            });

        await action.ExecuteAsync(message, context);

        _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
            h.MessageText.Contains("not found menu"))), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ReplacesTextAndTextUri()
    {
        var model = CreateModelWithMenu("search");
        var item = new YandexSearchTextItem
        {
            Key = "search",
            Descriptions = "desc",
            Text = ["query={Text}", "uri={Text:URI}"]
        };
        var action = new YandexSearchAction(model, item);
        var context = CreateContext();
        var message = CreateMessage("hello world");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(It.IsAny<ChatId>(),
            $"query=hello world{Environment.NewLine}uri=hello+world",
            ParseMode.Html,
            It.IsAny<ReplyParameters>(),
            It.IsAny<ReplyMarkup>(),
            It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(),
            It.IsAny<IEnumerable<MessageEntity>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
