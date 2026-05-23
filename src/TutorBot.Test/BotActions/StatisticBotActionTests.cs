using Moq;
using Shouldly;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;
using TutorBot.TelegramService;
using TutorBot.TelegramService.BotActions.Admins;

namespace TutorBot.Test.BotActions;

[Trait("Category", "Unit")]
public class StatisticBotActionTests
{
    private readonly Mock<ITelegramBot> _botMock = new();
    private readonly Mock<IApplication> _appMock = new();
    private readonly Mock<IChatService> _chatServiceMock = new();
    private readonly Mock<IHistoryService> _historyServiceMock = new();

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

    public StatisticBotActionTests()
    {
        _appMock.Setup(x => x.ChatService).Returns(_chatServiceMock.Object);
        _appMock.Setup(x => x.HistoryService).Returns(_historyServiceMock.Object);
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
    public void GenerateHtmlReport_EmptyReport_ReturnsHeader()
    {
        var report = new ChatSummaryReport();

        var result = StatisticBotAction.GenerateHtmlReport(report);

        result.ShouldContain("<b>📊 Статистика чатов</b>");
        result.ShouldContain("<b>Всего чатов:</b> 0");
        result.ShouldContain("<b>Всего сообщений:</b> 0");
        result.ShouldContain("<b>📈 Статистика по группам</b>");
        result.ShouldContain("<b>🏆 Топ 100 пользователей по количеству сообщений</b>");
        result.ShouldContain("<b>⏰ Среднее количество обращений по часам (последние 20 дней)</b>");
    }

    [Fact]
    public void GenerateHtmlReport_WithGroupSummaries_IncludesGroupTable()
    {
        var report = new ChatSummaryReport
        {
            GroupSummaries =
            [
                new GroupSummary { GroupNumber = "РИ-151001", UserCount = 30, MessageCount = 500 },
                new GroupSummary { GroupNumber = "РИ-151002", UserCount = 25, MessageCount = 300 }
            ]
        };

        var result = StatisticBotAction.GenerateHtmlReport(report);

        result.ShouldContain("РИ-151001");
        result.ShouldContain("РИ-151002");
        result.ShouldContain("30");
        result.ShouldContain("500");
        result.ShouldContain("25");
        result.ShouldContain("300");
    }

    [Fact]
    public void GenerateHtmlReport_WithTopUsers_IncludesUserTable()
    {
        var report = new ChatSummaryReport
        {
            TopUsers =
            [
                new UserMessageCount { FullName = "Иванов Иван Иванович", MessageCount = 150 },
                new UserMessageCount { FullName = "Петров Петр Петрович", MessageCount = 100 }
            ]
        };

        var result = StatisticBotAction.GenerateHtmlReport(report);

        result.ShouldContain("Иванов Иван Иванович");
        result.ShouldContain("Петров Петр Петрович");
        result.ShouldContain("150");
        result.ShouldContain("100");
    }

    [Fact]
    public void GenerateHtmlReport_WithHourlyAverages_IncludesHourlyTable()
    {
        var report = new ChatSummaryReport
        {
            GroupSummaries =
            [
                new GroupSummary { GroupNumber = "РИ-151001", UserCount = 30, MessageCount = 500 }
            ],
            HourlyAverages =
            [
                new HourlyAverage { GroupNumber = "РИ-151001", Hour = 10, MessageCount = 50, AverageMessages = 5.5 },
                new HourlyAverage { GroupNumber = "РИ-151001", Hour = 14, MessageCount = 30, AverageMessages = 3.2 }
            ]
        };

        var result = StatisticBotAction.GenerateHtmlReport(report);

        result.ShouldContain("<b>Группа РИ-151001:</b>");
        result.ShouldContain("10:00 | ");
        result.ShouldContain("14:00 | ");
    }

    [Fact]
    public void GenerateHtmlReport_AllSections_ReturnsCompleteReport()
    {
        var report = new ChatSummaryReport
        {
            NumberOfChats = 5,
            NumberOfMessages = 1000,
            GroupSummaries =
            [
                new GroupSummary { GroupNumber = "РИ-151001", UserCount = 30, MessageCount = 500 }
            ],
            TopUsers =
            [
                new UserMessageCount { FullName = "Иванов Иван", MessageCount = 200 }
            ],
            HourlyAverages =
            [
                new HourlyAverage { GroupNumber = "РИ-151001", Hour = 10, MessageCount = 50, AverageMessages = 5.5 }
            ]
        };

        var result = StatisticBotAction.GenerateHtmlReport(report);

        result.ShouldContain("<b>Всего чатов:</b> 5");
        result.ShouldContain("<b>Всего сообщений:</b> 1000");
        result.ShouldContain("РИ-151001");
        result.ShouldContain("Иванов Иван");
        result.ShouldContain("10:00");
    }

    [Fact]
    public void GenerateHtmlReport_TopUsersTruncation_TruncatesLongNames()
    {
        var report = new ChatSummaryReport
        {
            TopUsers =
            [
                new UserMessageCount
                {
                    FullName = "Оченьдлинноеимяфамилияотчествобольше25символов",
                    MessageCount = 99
                }
            ]
        };

        var result = StatisticBotAction.GenerateHtmlReport(report);

        result.ShouldContain("Оченьдлинноеимяфамилияотч...");
        result.ShouldNotContain("Оченьдлинноеимяфамилияотчествобольше25символов");
    }

    [Fact]
    public async Task ExecuteAsync_GetsChatsAndSendsReport()
    {
        var report = new ChatSummaryReport
        {
            NumberOfChats = 3,
            NumberOfMessages = 500
        };

        _chatServiceMock.Setup(x => x.GetChats(It.IsAny<GetChatsFilter?>()))
            .ReturnsAsync([]);
        _chatServiceMock.Setup(x => x.GetSummaryInfo())
            .ReturnsAsync(report);

        var action = new StatisticBotAction();
        var context = CreateContext();
        var message = new Message
        {
            Text = "Получить статистику",
            From = new User { Id = 42 },
            Chat = new Chat { Id = 100 },
            Date = DateTime.Now
        };

        await action.ExecuteAsync(message, context);

        _chatServiceMock.Verify(x => x.GetChats(It.IsAny<GetChatsFilter?>()), Times.Once);
        _chatServiceMock.Verify(x => x.GetSummaryInfo(), Times.Once);
        _botMock.Verify(x => x.SendMessage(It.IsAny<ChatId>(),
            It.Is<string>(s => s.Contains("<b>📊 Статистика чатов</b>")),
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
