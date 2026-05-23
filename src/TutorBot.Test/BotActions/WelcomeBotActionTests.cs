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
public class WelcomeBotActionTests
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
    private readonly DialogModel _model;

    public WelcomeBotActionTests()
    {
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

        _model = new DialogModel
        {
            Start = new StartNodeModel { Handler = "Welcome", NextStep = "SomeKey" },
            Handlers = new HandlersModel
            {
                Welcome = new WelcomeHandler
                {
                    Key = "Welcome",
                    WelcomeText = "Hello!",
                    ErrorText = "Invalid!",
                    FullNameQuestion = "What is your name?",
                    FullNameError = "Invalid name!",
                    GroupNumbers = ["1", "2", "3"]
                },
                Schedule = new ScheduleItem { Key = "schedule", Text = ["t"] },
                SimpleText = [
                    new SimpleTextItem { Key = "SomeKey", Text = ["forwarded"] }
                ],
                YandexSearchText = []
            },
            Menus = []
        };
    }

    private TutorBotContext CreateContext(Action<ChatEntry>? configure = null)
    {
        var context = new TutorBotContext(_botMock.Object, _options, _appMock.Object, 12345, CancellationToken.None);
        var chatEntry = new ChatEntry
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

    [Fact]
    public async Task ExecuteAsync_FirstMessage_SendsWelcomeRemovesKeyboard()
    {
        var context = CreateContext(ce => ce.IsFirstMessage = true);
        var action = new WelcomeBotAction(_model);
        var message = CreateMessage("anything");

        ReplyMarkup? capturedMarkup = null;
        _botMock.Setup(x => x.SendMessage(It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<ParseMode>(),
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Callback<ChatId, string, ParseMode, ReplyParameters?, ReplyMarkup?, LinkPreviewOptions?,
                int?, IEnumerable<MessageEntity>?, bool, bool, string?, string?, bool, CancellationToken>(
                (_, _, _, _, markup, _, _, _, _, _, _, _, _, _) => capturedMarkup = markup)
            .ReturnsAsync(new Message());

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "Hello!", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
        capturedMarkup.ShouldBeOfType<ReplyKeyboardRemove>();
    }

    [Fact]
    public async Task ExecuteAsync_SlashStart_SendsWelcome()
    {
        var context = CreateContext();
        var action = new WelcomeBotAction(_model);
        var message = CreateMessage("/start");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "Hello!", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_FirstMessage_SetsIsFirstMessageFalseAndUpdates()
    {
        var context = CreateContext(ce => ce.IsFirstMessage = true);
        var action = new WelcomeBotAction(_model);
        var message = CreateMessage("anything");

        await action.ExecuteAsync(message, context);

        context.ChatEntry.IsFirstMessage.ShouldBeFalse();
        _chatServiceMock.Verify(x => x.Update(context.ChatEntry), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidGroupNumber_UpdatesGroupAndForwards()
    {
        var context = CreateContext(ce =>
        {
            ce.GroupNumber = string.Empty;
        });
        var action = new WelcomeBotAction(_model);
        var message = CreateMessage("1");

        await action.ExecuteAsync(message, context);

        context.ChatEntry.GroupNumber.ShouldBe("1");
        _chatServiceMock.Verify(x => x.Update(It.Is<ChatEntry>(ce => ce.GroupNumber == "1")), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidGroupNumber_SendsError()
    {
        var context = CreateContext(ce => ce.GroupNumber = string.Empty);
        var action = new WelcomeBotAction(_model);
        var message = CreateMessage("invalid");

        ReplyMarkup? capturedMarkup = null;
        _botMock.Setup(x => x.SendMessage(It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<ParseMode>(),
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Callback<ChatId, string, ParseMode, ReplyParameters?, ReplyMarkup?, LinkPreviewOptions?,
                int?, IEnumerable<MessageEntity>?, bool, bool, string?, string?, bool, CancellationToken>(
                (_, _, _, _, markup, _, _, _, _, _, _, _, _, _) => capturedMarkup = markup)
            .ReturnsAsync(new Message());

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "Invalid!", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
        capturedMarkup.ShouldBeOfType<ReplyKeyboardRemove>();
    }

    [Fact]
    public async Task ExecuteAsync_GroupSetMissingFullName_SendsFullNameQuestion()
    {
        var context = CreateContext(ce =>
        {
            ce.GroupNumber = string.Empty;
            ce.FullName = string.Empty;
        });
        var action = new WelcomeBotAction(_model);
        var message = CreateMessage("1");

        ReplyMarkup? capturedMarkup = null;
        _botMock.Setup(x => x.SendMessage(It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<ParseMode>(),
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Callback<ChatId, string, ParseMode, ReplyParameters?, ReplyMarkup?, LinkPreviewOptions?,
                int?, IEnumerable<MessageEntity>?, bool, bool, string?, string?, bool, CancellationToken>(
                (_, _, _, _, markup, _, _, _, _, _, _, _, _, _) => capturedMarkup = markup)
            .ReturnsAsync(new Message());

        await action.ExecuteAsync(message, context);

        context.ChatEntry.GroupNumber.ShouldBe("1");
        _botMock.Verify(x => x.SendMessage(100, "What is your name?", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
        capturedMarkup.ShouldBeOfType<ReplyKeyboardRemove>();
    }

    [Fact]
    public async Task ExecuteAsync_InvalidFullName_SendsFullNameError()
    {
        var context = CreateContext(ce =>
        {
            ce.FullName = string.Empty;
        });
        var action = new WelcomeBotAction(_model);
        var message = CreateMessage("12345");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "Invalid name!", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidFullName_SetsFullNameAndUpdates()
    {
        var context = CreateContext(ce =>
        {
            ce.FullName = string.Empty;
        });
        var action = new WelcomeBotAction(_model);
        var message = CreateMessage("Иванов Иван");

        await action.ExecuteAsync(message, context);

        context.ChatEntry.FullName.ShouldBe("Иванов Иван");
        _chatServiceMock.Verify(x => x.Update(It.Is<ChatEntry>(ce => ce.FullName == "Иванов Иван")), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_GroupSetWithFullName_ForwardsToNextStep()
    {
        var context = CreateContext();
        var action = new WelcomeBotAction(_model);
        var message = CreateMessage("1");

        await action.ExecuteAsync(message, context);

        _chatServiceMock.Verify(x => x.Update(It.Is<ChatEntry>(ce => ce.GroupNumber == "1")), Times.Once);
    }
}
