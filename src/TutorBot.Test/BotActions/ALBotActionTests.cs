using Moq;
using Shouldly;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;
using TutorBot.TelegramService;
using TutorBot.TelegramService.BotActions;

namespace TutorBot.Test.BotActions;

[Trait("Category", "Unit")]
public class ALBotActionTests
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

    public ALBotActionTests()
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
        _alServiceMock.Setup(x => x.TransferQuestionAL(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync("test answer");
    }

    private TutorBotContext CreateContext()
    {
        var context = new TutorBotContext(_botMock.Object, _options, _appMock.Object, 12345, CancellationToken.None);
        context.ChatEntry = new ChatEntry
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

    [Fact]
    public void Key_ReturnsCorrectValue()
    {
        ALBotAction.Instance.Key.ShouldBe("Спросить нейросеть");
    }

    [Fact]
    public void EnableProlongated_ReturnsTrue()
    {
        ALBotAction.Instance.EnableProlongated.ShouldBeTrue();
    }

    [Fact]
    public void Instance_IsSingleton()
    {
        ALBotAction.Instance.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_TextEqualsKey_SendsPrompt()
    {
        var context = CreateContext();
        var message = CreateMessage("Спросить нейросеть");

        await ALBotAction.Instance.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "Сформулируйте вопрос",
            It.IsAny<ParseMode>(),
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_OtherText_CallsTransferQuestionAL()
    {
        _alServiceMock.Setup(x => x.TransferQuestionAL(100, "some question", It.IsAny<Guid>()))
            .ReturnsAsync("answer");

        var context = CreateContext();
        var message = CreateMessage("some question");

        await ALBotAction.Instance.ExecuteAsync(message, context);

        _alServiceMock.Verify(x => x.TransferQuestionAL(100, "some question", It.IsAny<Guid>()), Times.Once);
        _botMock.Verify(x => x.SendMessage(100, "answer",
            ParseMode.MarkdownV2,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_OtherText_EscapesMarkdown()
    {
        _alServiceMock.Setup(x => x.TransferQuestionAL(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync("hello_world");

        var context = CreateContext();
        var message = CreateMessage("question");

        await ALBotAction.Instance.ExecuteAsync(message, context);

        _botMock.Verify(x => x.SendMessage(100, "hello\\_world",
            ParseMode.MarkdownV2,
            It.IsAny<ReplyParameters>(), It.IsAny<ReplyMarkup>(), It.IsAny<LinkPreviewOptions>(),
            It.IsAny<int?>(), It.IsAny<IEnumerable<MessageEntity>>(), It.IsAny<bool>(),
            It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NullText_TransfersEmptyString()
    {
        string? capturedText = null;
        _alServiceMock.Setup(x => x.TransferQuestionAL(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Callback<long, string, Guid>((_, text, _) => capturedText = text)
            .ReturnsAsync("answer");

        var context = CreateContext();
        var message = CreateMessage(null);

        await ALBotAction.Instance.ExecuteAsync(message, context);

        capturedText.ShouldBe(string.Empty);
    }
}
