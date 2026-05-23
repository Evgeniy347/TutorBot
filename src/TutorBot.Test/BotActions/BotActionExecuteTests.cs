using Moq;
using Shouldly;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;
using TutorBot.TelegramService;
using TutorBot.TelegramService.BotActions;
using TutorBot.TelegramService.BotActions.Admins;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.Test.BotActions;

[Trait("Category", "Unit")]
public class BotActionExecuteTests
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

    private static Message CreateMessage(string text)
    {
        return new Message
        {
            Text = text,
            From = new User { Id = 42, FirstName = "Test" },
            Chat = new Chat { Id = 100, Type = ChatType.Private },
            Date = DateTime.Now
        };
    }

    public BotActionExecuteTests()
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
    public async Task NotifyBotAction_TogglesEnableAdminError()
    {
        var context = CreateContext();
        var action = new NotifyBotAction();
        var message = CreateMessage("Оповещения об ошибках");

        await action.ExecuteAsync(message, context);

        context.ChatEntry.EnableAdminError.ShouldBeTrue();

        await action.ExecuteAsync(message, context);

        context.ChatEntry.EnableAdminError.ShouldBeFalse();
    }

    [Fact]
    public async Task NotifyBotAction_SendsStatusMessage()
    {
        var context = CreateContext();
        var action = new NotifyBotAction();
        var message = CreateMessage("Оповещения об ошибках");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "EnableAdminError:True",
            It.IsAny<ParseMode>(), It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(),
            It.IsAny<LinkPreviewOptions>(), It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(),
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SimpleSubMenuBotAction_SendsMessageWithUserName()
    {
        var context = CreateContext();
        var menu = new MenuItem
        {
            Key = "test-menu",
            Text = "Привет, {UserName}!",
            Buttons = ["Кнопка 1", "Кнопка 2"]
        };
        var action = new SimpleSubMenuBotAction(menu);
        var message = CreateMessage("test-menu");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100,
            "Привет, иван!",
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
    public async Task SimpleSubMenuBotAction_EnableProlongated_ReturnsTrue()
    {
        var menu = new MenuItem { Key = "test", Text = "test", Buttons = ["btn"] };
        var action = new SimpleSubMenuBotAction(menu);
        action.EnableProlongated.ShouldBeTrue();
    }

    [Fact]
    public async Task SimpleTextBotAction_SendsMessageWithUserName()
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
            Menus = [
                new MenuItem { Key = "Главное меню", Text = "меню", Buttons = ["Помощь"] }
            ]
        };
        var item = new SimpleTextItem { Key = "Помощь", Text = ["{UserName}, помощь"] };
        var action = new SimpleTextBotAction(model, item);
        var context = CreateContext();
        var message = CreateMessage("Помощь");

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100,
            "иван, помощь",
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
    public async Task ResetBotAction_ClearsChatEntryAndSendsWelcome()
    {
        var context = CreateContext();
        var action = new ResetBotAction("Добро пожаловать!");
        var message = CreateMessage("Перезапустить");

        await action.ExecuteAsync(message, context);

        context.ChatEntry.FullName.ShouldBe(string.Empty);
        context.ChatEntry.GroupNumber.ShouldBe(string.Empty);
        _chatServiceMock.Verify(x => x.Update(context.ChatEntry), Times.Once);

        _botMock.Verify(x => x.SendMessage(100,
            "Добро пожаловать!",
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
    public async Task ResetBotAction_ReplyMarkup_IsReplyKeyboardRemove()
    {
        var context = CreateContext();
        var action = new ResetBotAction("текст");
        var message = CreateMessage("Перезапустить");

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

        capturedMarkup.ShouldBeOfType<ReplyKeyboardRemove>();
    }

    [Fact]
    public async Task GroupChatBotAction_DoesNotThrow()
    {
        var context = CreateContext();
        var action = new GroupChatBotAction();
        var message = CreateMessage("test");

        await action.ExecuteAsync(message, context);
    }

    [Fact]
    public async Task GroupChatBotAction_FirstMessage_CallsAskAssistantWithWelcome()
    {
        var context = CreateContext();
        context.ChatEntry.IsFirstMessage = true;
        var action = new GroupChatBotAction();
        var message = CreateMessage("test");

        _alServiceMock.Setup(x => x.AskAssistant(TextPromts.WelcomeGroup)).ReturnsAsync("answer");

        await action.ExecuteAsync(message, context);

        context.ChatEntry.IsFirstMessage.ShouldBeFalse();
        _alServiceMock.Verify(x => x.AskAssistant(TextPromts.WelcomeGroup), Times.Once);
    }

    [Fact]
    public async Task GroupChatBotAction_SubsequentMessage_CallsAskAssistantWithParams()
    {
        var sessionId = Guid.NewGuid();
        var context = CreateContext();
        context.ChatEntry.IsFirstMessage = false;
        context.ChatEntry.ChatID = 100;
        context.ChatEntry.SessionID = sessionId;
        var action = new GroupChatBotAction();
        var message = CreateMessage("hello");

        await action.ExecuteAsync(message, context);

        _alServiceMock.Verify(x => x.AskAssistant(100, 42, "hello", sessionId), Times.Once);
    }

    [Fact]
    public async Task GroupChatBotAction_EmptyAnswer_DoesNotSendMessage()
    {
        var context = CreateContext();
        context.ChatEntry.IsFirstMessage = true;
        var action = new GroupChatBotAction();
        var message = CreateMessage("test");

        _alServiceMock.Setup(x => x.AskAssistant(It.IsAny<string>())).ReturnsAsync(string.Empty);

        await action.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(
            It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<ParseMode>(),
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(),
            It.IsAny<LinkPreviewOptions>(), It.IsAny<int?>(),
            It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GroupChatBotAction_NullText_ReturnsEarly()
    {
        var context = CreateContext();
        var action = new GroupChatBotAction();
        var message = CreateMessage(null!);

        await action.ExecuteAsync(message, context);

        _alServiceMock.Verify(x => x.AskAssistant(It.IsAny<string>()), Times.Never);
        _botMock.Verify(x => x.SendMessage(
            It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<ParseMode>(),
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(),
            It.IsAny<LinkPreviewOptions>(), It.IsAny<int?>(),
            It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GroupChatBotAction_NoFrom_ReturnsEarly()
    {
        var context = CreateContext();
        var action = new GroupChatBotAction();
        var message = CreateMessage("test");
        message.From = null;

        await action.ExecuteAsync(message, context);

        _alServiceMock.Verify(x => x.AskAssistant(It.IsAny<string>()), Times.Never);
        _botMock.Verify(x => x.SendMessage(
            It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<ParseMode>(),
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(),
            It.IsAny<LinkPreviewOptions>(), It.IsAny<int?>(),
            It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SimpleTextBotAction_NoMatchingMenu_WritesError()
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
        var item = new SimpleTextItem { Key = "key без меню", Text = ["текст"] };
        var action = new SimpleTextBotAction(model, item);
        var context = CreateContext();
        var message = CreateMessage("key без меню");

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
}
