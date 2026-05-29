using Moq;
using Shouldly;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;
using TutorBot.TelegramService;

namespace TutorBot.Test.TelegramService;

[Trait("Category", "Unit")]
public class TutorBotContextTests
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
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public TutorBotContextTests()
    {
        _appMock.Setup(x => x.ChatService).Returns(_chatServiceMock.Object);
        _appMock.Setup(x => x.HistoryService).Returns(_historyServiceMock.Object);
        _appMock.Setup(x => x.ALService).Returns(_alServiceMock.Object);
        _historyServiceMock.Setup(x => x.AddHistory(It.IsAny<MessageHistory>())).Returns(Task.CompletedTask);
        _botMock.Setup(x => x.SendMessage(
            It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<ParseMode>(),
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message());
    }

    private TutorBotContext CreateContext(ChatEntry? chatEntry = null)
    {
        TutorBotContext context = new TutorBotContext(_botMock.Object, _options, _appMock.Object, 12345, _cancellationToken);
        context.ChatEntry = chatEntry ?? new ChatEntry
        {
            ID = 1,
            ChatID = 100,
            UserID = 42,
            FullName = "Test User",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            GroupNumber = "TestGroup",
            SessionID = Guid.NewGuid()
        };
        return context;
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        TutorBotContext context = new TutorBotContext(_botMock.Object, _options, _appMock.Object, 12345, _cancellationToken);

        context.Client.ShouldBe(_botMock.Object);
        context.Opt.ShouldBe(_options);
        context.App.ShouldBe(_appMock.Object);
        context.BotID.ShouldBe(12345);
        context.stoppingToken.ShouldBe(_cancellationToken);
    }

    [Fact]
    public void ChatEntry_SetAndGet()
    {
        TutorBotContext context = CreateContext();
        ChatEntry chatEntry = new ChatEntry
        {
            ID = 99,
            ChatID = 999,
            UserID = 999,
        };

        context.ChatEntry = chatEntry;
        context.ChatEntry.ShouldBe(chatEntry);
    }

    [Fact]
    public void ChatEntry_SetNull_Throws()
    {
        TutorBotContext context = new TutorBotContext(_botMock.Object, _options, _appMock.Object, 12345, _cancellationToken);

        Should.Throw<ArgumentNullException>(() => context.ChatEntry = null!);
    }

    [Fact]
    public void ChatEntry_GetBeforeSet_Throws()
    {
        TutorBotContext context = new TutorBotContext(_botMock.Object, _options, _appMock.Object, 12345, _cancellationToken);

        Should.Throw<ArgumentNullException>(() => context.ChatEntry);
    }

    [Fact]
    public void Token_ReturnsStoppingToken()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        TutorBotContext context = new TutorBotContext(_botMock.Object, _options, _appMock.Object, 12345, cts.Token);

        context.Token.ShouldBe(cts.Token);
        context.Token.ShouldBe(context.stoppingToken);
    }

    [Fact]
    public void IsGroupChat_DefaultFalse()
    {
        TutorBotContext context = CreateContext();

        context.IsGroupChat.ShouldBeFalse();
    }

    [Fact]
    public void IsGroupChat_SetTrue()
    {
        TutorBotContext context = CreateContext();

        context.IsGroupChat = true;

        context.IsGroupChat.ShouldBeTrue();
    }

    [Fact]
    public async Task SendMessage_NoReplyMarkup_CallsClient()
    {
        TutorBotContext context = CreateContext();

        await context.SendMessage("Hello");

        _botMock.Verify(x => x.SendMessage(
            It.Is<ChatId>(id => id!.Identifier == 100),
            "Hello",
            ParseMode.Html,
            It.IsAny<ReplyParameters>(),
            null,
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
    public async Task SendMessage_WithReplyKeyboardRemove_LogsRemove()
    {
        TutorBotContext context = CreateContext();
        ReplyKeyboardRemove removeMarkup = new ReplyKeyboardRemove();

        await context.SendMessage("test", replyMarkup: removeMarkup);

        _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
            h.MessageText.Contains("ReplyKeyboardRemove"))), Times.Once);
        _botMock.Verify(x => x.SendMessage(
            It.Is<ChatId>(id => id!.Identifier == 100),
            "test",
            ParseMode.Html,
            It.IsAny<ReplyParameters>(),
            removeMarkup,
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
    public async Task SendMessage_WithReplyKeyboardMarkup_LogsMarkup()
    {
        TutorBotContext context = CreateContext();
        ReplyKeyboardMarkup keyboardMarkup = new ReplyKeyboardMarkup([
            [new KeyboardButton("Btn1"), new KeyboardButton("Btn2")],
            [new KeyboardButton("Btn3")]
        ]);

        await context.SendMessage("test", replyMarkup: keyboardMarkup);

        _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
            h.MessageText.Contains("ReplyKeyboardMarkup") &&
            h.MessageText.Contains("Btn1; Btn2; Btn3"))), Times.Once);
    }

    [Fact]
    public async Task SendMessage_AddsHistory()
    {
        TutorBotContext context = CreateContext();
        Guid sessionId = context.ChatEntry.SessionID;

        await context.SendMessage("test message");

        _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
            h.ChatID == 100 &&
            h.MessageText == "test message" &&
            h.Type == MessageHistoryRole.Bot &&
            h.UserID == 42 &&
            h.SessionID == sessionId)), Times.Once);
    }

    [Fact]
    public async Task ErrorHandle_WithNoAdminChats_WritesError()
    {
        TutorBotContext context = CreateContext();
        InvalidOperationException exception = new InvalidOperationException("test error");

        _chatServiceMock.Setup(x => x.GetChats(It.IsAny<GetChatsFilter>()))
            .ReturnsAsync([]);
        _chatServiceMock.Setup(x => x.Find(-1))
            .ReturnsAsync((ChatEntry?)null);
        _chatServiceMock.Setup(x => x.Create(-1, "Error Service", string.Empty, string.Empty, -1))
            .ReturnsAsync(new ChatEntry { ID = -1, ChatID = -1, UserID = -1 });

        await context.ErrorHandle(exception, "Title");

        _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
            h.MessageText.Contains("Title") &&
            h.MessageText.Contains("test error") &&
            h.Type == MessageHistoryRole.Error)), Times.Once);
        _chatServiceMock.Verify(x => x.GetChats(It.Is<GetChatsFilter>(f =>
            !f.IsAdmin && f.EnableAdminError)), Times.Once);
    }

    [Fact]
    public async Task ErrorHandle_WithAdminChats_NotifiesEach()
    {
        TutorBotContext context = CreateContext();
        InvalidOperationException exception = new InvalidOperationException("test error");
        ChatEntry[] adminChats = new[]
        {
            new ChatEntry { ID = 10, ChatID = 1000, UserID = 100, IsAdmin = true, EnableAdminError = true },
            new ChatEntry { ID = 11, ChatID = 1001, UserID = 101, IsAdmin = true, EnableAdminError = true },
        };

        _chatServiceMock.Setup(x => x.GetChats(It.IsAny<GetChatsFilter>()))
            .ReturnsAsync(adminChats);
        _chatServiceMock.Setup(x => x.Find(-1))
            .ReturnsAsync((ChatEntry?)null);
        _chatServiceMock.Setup(x => x.Create(-1, "Error Service", string.Empty, string.Empty, -1))
            .ReturnsAsync(new ChatEntry { ID = -1, ChatID = -1, UserID = -1 });

        await context.ErrorHandle(exception);

        _botMock.Verify(x => x.SendMessage(
            It.Is<ChatId>(id => id!.Identifier == 1000),
            It.Is<string>(s => s.Contains("test error")),
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
        _botMock.Verify(x => x.SendMessage(
            It.Is<ChatId>(id => id!.Identifier == 1001),
            It.Is<string>(s => s.Contains("test error")),
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
    public async Task ErrorHandle_ExceptionInAdminNotify_DoesNotThrow()
    {
        TutorBotContext context = CreateContext();
        InvalidOperationException exception = new InvalidOperationException("test error");
        ChatEntry[] adminChats = new[]
        {
            new ChatEntry { ID = 10, ChatID = 1000, UserID = 100, IsAdmin = true, EnableAdminError = true },
        };

        _chatServiceMock.Setup(x => x.GetChats(It.IsAny<GetChatsFilter>()))
            .ReturnsAsync(adminChats);
        _chatServiceMock.Setup(x => x.Find(-1))
            .ReturnsAsync((ChatEntry?)null);
        _chatServiceMock.Setup(x => x.Create(-1, "Error Service", string.Empty, string.Empty, -1))
            .ReturnsAsync(new ChatEntry { ID = -1, ChatID = -1, UserID = -1 });
        _botMock.Setup(x => x.SendMessage(
            It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<ParseMode>(),
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("send failed"));

        await context.ErrorHandle(exception);
    }

    [Fact]
    public async Task WriteError_CallsConsoleAndHistory()
    {
        TutorBotContext context = CreateContext();
        StringWriter stringWriter = new StringWriter();
        TextWriter originalOut = Console.Out;
        Console.SetOut(stringWriter);

        try
        {
            _chatServiceMock.Setup(x => x.Find(-1))
                .ReturnsAsync((ChatEntry?)null);
            _chatServiceMock.Setup(x => x.Create(-1, "Error Service", string.Empty, string.Empty, -1))
                .ReturnsAsync(new ChatEntry { ID = -1, ChatID = -1, UserID = -1 });

            await context.WriteError("error message");

            stringWriter.ToString().ShouldContain("error message");
            _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
                h.MessageText == "error message" &&
                h.Type == MessageHistoryRole.Error)), Times.Once);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task WriteError_GetErrorChat_CreatesIfNotFound()
    {
        TutorBotContext context = CreateContext();

        _chatServiceMock.Setup(x => x.Find(-1))
            .ReturnsAsync((ChatEntry?)null);
        _chatServiceMock.Setup(x => x.Create(-1, "Error Service", string.Empty, string.Empty, -1))
            .ReturnsAsync(new ChatEntry { ID = -1, ChatID = -1, UserID = -1 });

        await context.WriteError("test");

        _chatServiceMock.Verify(x => x.Find(-1), Times.Once);
        _chatServiceMock.Verify(x => x.Create(-1, "Error Service", string.Empty, string.Empty, -1), Times.Once);
        _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
            h.ChatID == -1 && h.UserID == 12345)), Times.Once);
    }

    [Fact]
    public async Task WriteError_GetErrorChat_FindsExisting()
    {
        TutorBotContext context = CreateContext();

        _chatServiceMock.Setup(x => x.Find(-1))
            .ReturnsAsync(new ChatEntry { ID = -1, ChatID = -1, UserID = -1 });

        await context.WriteError("test");

        _chatServiceMock.Verify(x => x.Find(-1), Times.Once);
        _chatServiceMock.Verify(x => x.Create(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
        _historyServiceMock.Verify(x => x.AddHistory(It.Is<MessageHistory>(h =>
            h.ChatID == -1 && h.UserID == 12345)), Times.Once);
    }
}
