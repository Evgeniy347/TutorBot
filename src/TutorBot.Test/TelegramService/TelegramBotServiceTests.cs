using Moq;
using Shouldly;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TutorBot.Abstractions;
using TutorBot.TelegramService;
using TutorBot.TelegramService.BotActions;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.Test.TelegramService;

[Trait("Category", "Unit")]
public class TelegramBotServiceTests : IDisposable
{
    private sealed class StubBotFactory : IBotFactory
    {
        public Task<ITelegramBot> CreateBot(CancellationToken cancellationToken) =>
            Task.FromResult(Mock.Of<ITelegramBot>());
    }

    private readonly string _tempDir;
    private readonly string _modelPath;
    private readonly Mock<ITelegramBot> _botMock = new();
    private readonly Mock<IApplication> _appMock = new();
    private readonly Mock<IChatService> _chatServiceMock = new();
    private readonly Mock<IHistoryService> _historyServiceMock = new();
    private readonly TgBotServiceOptions _options;
    private readonly TelegramBotService _service;

    public TelegramBotServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _modelPath = Path.Combine(_tempDir, "dialog.json");
        File.WriteAllText(_modelPath, /*lang=json*/ """
        {
            "Start": { "Handler": "s", "NextStep": "Help" },
            "Handlers": {
                "Welcome": { "Key": "/start", "WelcomeText": "W", "ErrorText": "E", "GroupNumbers": ["1"] },
                "Schedule": { "Key": "Sched", "Text": ["t"] },
                "SimpleText": [{ "Key": "Help", "Text": ["help text"] }],
                "YandexSearchText": []
            },
            "Menus": [{ "Key": "Main", "Text": "Menu", "Buttons": ["Help", "Sched"] }]
        }
        """);

        _options = new TgBotServiceOptions
        {
            Enable = true,
            Token = "test-token",
            DialogModelPath = _modelPath,
            EvaluateKey = "\"test123\""
        };

        _appMock.Setup(x => x.ChatService).Returns(_chatServiceMock.Object);
        _appMock.Setup(x => x.HistoryService).Returns(_historyServiceMock.Object);
        _historyServiceMock.Setup(x => x.AddHistory(It.IsAny<MessageHistory>())).Returns(Task.CompletedTask);
        _historyServiceMock.Setup(x => x.AddStatusService(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        _service = new TelegramBotService(_appMock.Object, new Microsoft.Extensions.Options.OptionsWrapper<TgBotServiceOptions>(_options), new StubBotFactory());
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private static TutorBotContext CreateContext(Action<ChatEntry>? configure = null)
    {
        ChatEntry entry = new ChatEntry
        {
            ID = 1,
            ChatID = 100,
            UserID = 42,
            FullName = "Test User",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            GroupNumber = "RI-100500",
            SessionID = Guid.NewGuid(),
            IsAdmin = false
        };
        configure?.Invoke(entry);

        TutorBotContext context = new TutorBotContext(
            Mock.Of<ITelegramBot>(),
            new TgBotServiceOptions { Enable = true, Token = "t", DialogModelPath = "x", EvaluateKey = "k" },
            Mock.Of<IApplication>(),
            12345,
            CancellationToken.None
        );
        context.ChatEntry = entry;
        return context;
    }

    // ==================== IsWelcome ====================

    [Fact]
    public void IsWelcome_EmptyGroupNumber_ReturnsTrue()
    {
        TutorBotContext context = CreateContext(ce => ce.GroupNumber = string.Empty);
        DialogModel model = new DialogModel
        {
            Start = new StartNodeModel { Handler = "s", NextStep = "n" },
            Handlers = new HandlersModel
            {
                Welcome = new WelcomeHandler { Key = "/start", WelcomeText = "W", ErrorText = "E", GroupNumbers = ["1"], FullNameQuestion = null },
                Schedule = new ScheduleItem { Key = "Sched", Text = ["t"] },
                SimpleText = [],
                YandexSearchText = []
            },
            Menus = []
        };

        bool result = TelegramBotService.IsWelcome(context, model);

        result.ShouldBeTrue();
    }

    [Fact]
    public void IsWelcome_HasGroupAndFullName_ReturnsFalse()
    {
        TutorBotContext context = CreateContext();
        DialogModel model = new DialogModel
        {
            Start = new StartNodeModel { Handler = "s", NextStep = "n" },
            Handlers = new HandlersModel
            {
                Welcome = new WelcomeHandler { Key = "/start", WelcomeText = "W", ErrorText = "E", GroupNumbers = ["1"], FullNameQuestion = null },
                Schedule = new ScheduleItem { Key = "Sched", Text = ["t"] },
                SimpleText = [],
                YandexSearchText = []
            },
            Menus = []
        };

        bool result = TelegramBotService.IsWelcome(context, model);

        result.ShouldBeFalse();
    }

    [Fact]
    public void IsWelcome_HasGroupEmptyFullNameWithQuestion_ReturnsTrue()
    {
        TutorBotContext context = CreateContext(ce => ce.FullName = string.Empty);
        DialogModel model = new DialogModel
        {
            Start = new StartNodeModel { Handler = "s", NextStep = "n" },
            Handlers = new HandlersModel
            {
                Welcome = new WelcomeHandler { Key = "/start", WelcomeText = "W", ErrorText = "E", GroupNumbers = ["1"], FullNameQuestion = "What is your name?" },
                Schedule = new ScheduleItem { Key = "Sched", Text = ["t"] },
                SimpleText = [],
                YandexSearchText = []
            },
            Menus = []
        };

        bool result = TelegramBotService.IsWelcome(context, model);

        result.ShouldBeTrue();
    }

    [Fact]
    public void IsWelcome_HasGroupEmptyFullNameWithoutQuestion_ReturnsFalse()
    {
        TutorBotContext context = CreateContext(ce => ce.FullName = string.Empty);
        DialogModel model = new DialogModel
        {
            Start = new StartNodeModel { Handler = "s", NextStep = "n" },
            Handlers = new HandlersModel
            {
                Welcome = new WelcomeHandler { Key = "/start", WelcomeText = "W", ErrorText = "E", GroupNumbers = ["1"], FullNameQuestion = null },
                Schedule = new ScheduleItem { Key = "Sched", Text = ["t"] },
                SimpleText = [],
                YandexSearchText = []
            },
            Menus = []
        };

        bool result = TelegramBotService.IsWelcome(context, model);

        result.ShouldBeFalse();
    }

    // ==================== FindAction ====================

    [Fact]
    public void FindAction_TextFound_ReturnsAction()
    {
        TutorBotContext context = CreateContext();
        IBotAction? result = _service.FindAction("Help", context);
        result.ShouldNotBeNull();
        result.Key.ShouldBe("Help");
    }

    [Fact]
    public void FindAction_TextNotFoundNotAdmin_ReturnsNull()
    {
        TutorBotContext context = CreateContext();
        IBotAction? result = _service.FindAction("NonExistent", context);
        result.ShouldBeNull();
    }

    [Fact]
    public void FindAction_TextNotFoundIsAdmin_FindsInAdminHandles()
    {
        TutorBotContext context = CreateContext(ce => ce.IsAdmin = true);
        IBotAction? result = _service.FindAction("Получить статистику", context);
        result.ShouldNotBeNull();
        result.Key.ShouldBe("Получить статистику");
    }

    // ==================== SelectAction ====================

    [Fact]
    public void SelectAction_TextFound_ReturnsAction()
    {
        TutorBotContext context = CreateContext();
        Message message = new Message { Text = "Help" };
        IBotAction? result = _service.SelectAction(message, context);
        result.ShouldNotBeNull();
        result.Key.ShouldBe("Help");
    }

    [Fact]
    public void SelectAction_TextNotFoundProlongated_ReturnsLastAction()
    {
        TutorBotContext context = CreateContext(ce =>
        {
            ce.LastActionKey = "Спросить нейросеть";
            ce.IsAdmin = true;
        });
        Message message = new Message { Text = "Some question" };
        IBotAction? result = _service.SelectAction(message, context);
        result.ShouldNotBeNull();
        result.Key.ShouldBe("Спросить нейросеть");
    }

    [Fact]
    public void SelectAction_TextNotFoundNoProlongated_ReturnsNull()
    {
        TutorBotContext context = CreateContext(ce =>
        {
            ce.LastActionKey = "Welcome";
            ce.IsAdmin = true;
        });
        Message message = new Message { Text = "Some text" };
        IBotAction? result = _service.SelectAction(message, context);
        result.ShouldBeNull();
    }

    [Fact]
    public void SelectAction_TextNotFoundNoLastKey_ReturnsNull()
    {
        TutorBotContext context = CreateContext(ce => ce.LastActionKey = null);
        Message message = new Message { Text = "Some text" };
        IBotAction? result = _service.SelectAction(message, context);
        result.ShouldBeNull();
    }

    // ==================== EnsureChat ====================

    [Fact]
    public async Task EnsureChat_ExistingChat_ReturnsEntry()
    {
        ChatEntry existingEntry = new ChatEntry { ID = 1, ChatID = 12345, UserID = 42 };
        _chatServiceMock.Setup(x => x.Find(12345)).ReturnsAsync(existingEntry);
        Message message = new Message { Chat = new Chat { Id = 12345 }, From = new User { Id = 42 } };

        ChatEntry result = await _service.EnsureChat(message);

        result.ShouldBe(existingEntry);
        _chatServiceMock.Verify(x => x.Create(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task EnsureChat_NewChat_CreatesEntry()
    {
        _chatServiceMock.Setup(x => x.Find(12345)).ReturnsAsync((ChatEntry?)null);
        _chatServiceMock.Setup(x => x.Create(99, "Test", "User", "testuser", 12345))
            .ReturnsAsync(new ChatEntry { ID = 2, ChatID = 12345, UserID = 99 });
        Message message = new Message
        {
            Chat = new Chat { Id = 12345 },
            From = new User { Id = 99, FirstName = "Test", LastName = "User", Username = "testuser" }
        };

        ChatEntry result = await _service.EnsureChat(message);

        result.ShouldNotBeNull();
        result.ChatID.ShouldBe(12345);
    }

    [Fact]
    public void EnsureChat_NullChat_Throws()
    {
        Message message = new Message { Chat = null!, From = new User { Id = 42 } };

        Should.Throw<ArgumentNullException>(() => _service.EnsureChat(message));
    }

    // ==================== ErrorHandle ====================

    [Fact]
    public async Task ErrorHandle_CreatesContextAndDelegates()
    {
        _chatServiceMock.Setup(x => x.Find(-1)).ReturnsAsync((ChatEntry?)null);
        _chatServiceMock.Setup(x => x.Create(-1, "Error Service", string.Empty, string.Empty, -1))
            .ReturnsAsync(new ChatEntry { ID = -1, ChatID = -1, UserID = -1 });
        _chatServiceMock.Setup(x => x.GetChats(It.IsAny<GetChatsFilter>())).ReturnsAsync([]);

        await _service.ErrorHandle(new InvalidOperationException("test"), HandleErrorSource.HandleUpdateError, 12345, Mock.Of<ITelegramBot>(), CancellationToken.None);

        _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
            h.Type == MessageHistoryRole.Error)), Times.AtLeastOnce);
    }
}
