using Moq;
using Shouldly;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;
using TutorBot.TelegramService;
using TutorBot.TelegramService.BotActions.Admins;

namespace TutorBot.Test.BotActions.Admins;

[Trait("Category", "Unit")]
public class AdminBotActionTests
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

    private readonly AdminBotAction _action = new();

    public AdminBotActionTests()
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

    private static Message CreateMessage(string? text, long userId = 42)
    {
        return new Message
        {
            Text = text,
            From = new User { Id = userId, FirstName = "Test" },
            Chat = new Chat { Id = 100, Type = ChatType.Private },
            Date = DateTime.Now
        };
    }

    [Fact]
    public void Key_ReturnsAdmin()
    {
        _action.Key.ShouldBe("/admin");
    }

    [Fact]
    public void EnableProlongated_ReturnsTrue()
    {
        _action.EnableProlongated.ShouldBeTrue();
    }

    [Fact]
    public void MaxCount_Returns10()
    {
        AdminBotAction.MaxCount.ShouldBe(10UL);
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyAdmin_SendsAdminMenu()
    {
        var context = CreateContext(ce => ce.IsAdmin = true);
        var message = CreateMessage("/admin");

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

        await _action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "Выберите опцию:", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
        capturedMarkup.ShouldBeOfType<ReplyKeyboardMarkup>();
    }

    [Fact]
    public async Task ExecuteAsync_NotAdmin_KeyMatches_PromptsForCode()
    {
        var context = CreateContext(ce => ce.IsAdmin = false);
        var message = CreateMessage("/admin", userId: 99);

        await _action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "Введите код доступа", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NotAdmin_CorrectCode_GrantsAccess()
    {
        long userId = 999001;
        var options = new TgBotServiceOptions
        {
            Enable = true,
            Token = "test-token",
            DialogModelPath = "test.json",
            EvaluateKey = "\"test123\""
        };
        var context = new TutorBotContext(_botMock.Object, options, _appMock.Object, 12345, CancellationToken.None);
        context.ChatEntry = new ChatEntry
        {
            ID = 1, ChatID = 100, UserID = userId, FullName = "Test", FirstName = "Test",
            LastName = "User", UserName = "tester", GroupNumber = "GRP-001",
            SessionID = Guid.NewGuid(), IsAdmin = false
        };
        var message = new Message
        {
            Text = "test123",
            From = new User { Id = userId, FirstName = "Test" },
            Chat = new Chat { Id = 100, Type = ChatType.Private },
            Date = DateTime.Now
        };

        await _action.ExecuteAsync(message, context);

        context.ChatEntry.IsAdmin.ShouldBeTrue();
        _botMock.Verify(x => x.SendMessage(100, "Теперь вы администратор", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NotAdmin_WrongCode_Rejects()
    {
        long userId = 999002;
        var options = new TgBotServiceOptions
        {
            Enable = true,
            Token = "test-token",
            DialogModelPath = "test.json",
            EvaluateKey = "\"test123\""
        };
        var context = new TutorBotContext(_botMock.Object, options, _appMock.Object, 12345, CancellationToken.None);
        context.ChatEntry = new ChatEntry
        {
            ID = 1, ChatID = 100, UserID = userId, FullName = "Test", FirstName = "Test",
            LastName = "User", UserName = "tester", GroupNumber = "GRP-001",
            SessionID = Guid.NewGuid(), IsAdmin = false
        };
        var message = new Message
        {
            Text = "wrong",
            From = new User { Id = userId, FirstName = "Test" },
            Chat = new Chat { Id = 100, Type = ChatType.Private },
            Date = DateTime.Now
        };

        await _action.ExecuteAsync(message, context);

        context.ChatEntry.IsAdmin.ShouldBeFalse();
        _botMock.Verify(x => x.SendMessage(100, "Код доступа введен с ошибкой", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NotAdmin_ExceedsMaxAttempts_Denies()
    {
        long userId = 999003;
        var context = CreateContext(ce =>
        {
            ce.IsAdmin = false;
            ce.UserID = userId;
        });
        var message = CreateMessage("/admin", userId: userId);

        for (int i = 0; i < 12; i++)
        {
            await _action.ExecuteAsync(message, context);
        }

        _botMock.Verify(x => x.SendMessage(100, "Вам запрещено вводить код доступа", ParseMode.Html,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
