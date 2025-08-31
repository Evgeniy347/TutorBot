using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using System.Runtime.CompilerServices;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;
using TutorBot.TelegramService;
using TutorBot.TelegramService.BotActions;
using TutorBot.Test.Helpers;
using TutorBot.Test.TestFramework;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.Test.Common;

[DatabaseSnapshotGroup]
public class ApplicationCoreTest(CustomAppFactory factory, ITestOutputHelper output) : IntegrationTestsBase
{
    private readonly UniqueRandomGenerator _random = new UniqueRandomGenerator();

    [Fact]
    public async Task App_Health()
    {
        using (HttpClient client = await factory.CreateApplication())
        {
            using var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);
            output.WriteLine($@"StatusCode {(int)response.StatusCode} {response.StatusCode}
{await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)}");

            if (!response.IsSuccessStatusCode)
            {
                Assert.NotNull(response.Content);

                string html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

                Assert.Fail(html);
            }
        }
    }

    [Fact]
    public async Task App_Error()
    {
        using (HttpClient client = await factory.CreateApplication())
        {
            IApplication app = factory.Services.GetRequiredService<IApplication>();

            if (TelegramBotFake.Instance._onError == null)
                throw new NullReferenceException("TelegramBotFake.Instance._onError");

            long adminChatID = _random.NextUniqueInt64(), alternativeAdminChatID = _random.NextUniqueInt64(), userChatID = _random.NextUniqueInt64();

            await EnsureChat(app, userChatID, "user");
            await EnsureChat(app, adminChatID, "admin", x => { x.IsAdmin = true; x.EnableAdminError = true; });
            await EnsureChat(app, alternativeAdminChatID, "admin", x => { x.IsAdmin = true; });

            string message = $"fake exception {Guid.NewGuid()}";

            await TelegramBotFake.Instance._onError.Invoke(new Exception(message), Telegram.Bot.Polling.HandleErrorSource.FatalError);

            MessageHistory[] messages;

            messages = await app.HistoryService.GetMessages(adminChatID, int.MaxValue, 10, true);
            if (!messages.Any(x => x.MessageText.Contains(message)))
                Assert.Fail("not found fake exception");

            messages = await app.HistoryService.GetMessages(alternativeAdminChatID, int.MaxValue, 10, true);
            if (messages.Any(x => x.MessageText.Contains(message)))
                Assert.Fail("found fake exception");

            messages = await app.HistoryService.GetMessages(userChatID, int.MaxValue, 10, true);
            if (messages.Any(x => x.MessageText.Contains(message)))
                Assert.Fail("found fake exception");
        }
    }

    [Fact]
    public async Task App_Welcome_StressTest()
    {
        using (HttpClient client = await factory.CreateApplication())
        {
            await Parallel.ForAsync(0, 500, async (x, y) => await TestScenario($"user-{x}"));
        }
    }

    [Theory]
    [InlineData("first user")]
    [InlineData("second user")]
    public async Task App_Welcome(string firstName)
    {
        using (HttpClient client = await factory.CreateApplication())
        {
            await TestScenario(firstName);
        }
    }

    private async Task TestScenario(string firstName)
    {
        IApplication app = factory.Services.GetRequiredService<IApplication>();

        if (TelegramBotFake.Instance._onMessage == null)
            throw new NullReferenceException("TelegramBotFake.Instance._onMessage");

        DialogModel model = GetModel();

        MenuItem menu = model.Menus.Single(x => x.Key == "↩️ В главное меню");
        MenuItem subMenu = model.Menus.Single(x => x.Key == "📚 Ликвидации академических задолженностей");
        SimpleTextItem simpleTextMenu = model.Handlers.SimpleText.Single(x => x.Key == "📅 Расписание группы");
        SimpleTextItem simpleTextSubMenu = model.Handlers.SimpleText.Single(x => x.Key == "❓ Сколько у меня долгов?");

        UserChatHelper chatHelper = CreateRandomUser(firstName);

        await chatHelper.SentTextWithCheck("/start", model.Handlers.Welcome.WelcomeText, []);

        await chatHelper.SentTextWithCheck("xxx", model.Handlers.Welcome.ErrorText, []);

        await chatHelper.SentTextWithCheck("РИ-421056", model.Handlers.Welcome.FullNameQuestion!, []);

        await chatHelper.SentTextWithCheck("!1$%", model.Handlers.Welcome.FullNameError!);

        await chatHelper.SentTextWithCheck("иванов иван", menu.Text, menu.Buttons);

        await chatHelper.SentTextWithCheck("📚 Ликвидации академических задолженностей", subMenu.Text, subMenu.Buttons);

        await chatHelper.SentTextWithCheck("❓ Сколько у меня долгов?", simpleTextSubMenu.GetText(), subMenu.Buttons);

        await chatHelper.SentTextWithCheck("↩️ В главное меню", menu.Text, menu.Buttons);

        await chatHelper.SentTextWithCheck("📅 Расписание группы", simpleTextMenu.GetText(), menu.Buttons);

        await chatHelper.SentTextWithCheck("Перезапустить", model.Handlers.Welcome.WelcomeText, []);

        await chatHelper.SentTextWithCheck("xxx", model.Handlers.Welcome.ErrorText, []);

        await chatHelper.SentTextWithCheck("РИ-421056", model.Handlers.Welcome.FullNameQuestion!, []);

        await chatHelper.SentTextWithCheck("!1$%", model.Handlers.Welcome.FullNameError!);

        await chatHelper.SentTextWithCheck("иван", menu.Text, menu.Buttons);

        await chatHelper.SentTextWithCheck("📚 Ликвидации академических задолженностей", subMenu.Text, subMenu.Buttons);
    }

    internal class UserChatHelper
    {
        public required Chat Chat { get; init; }
        public required User From { get; init; }

        public async Task SentText(string text)
        {
            if (TelegramBotFake.Instance._onMessage == null)
                throw new NullReferenceException("TelegramBotFake.Instance._onMessage");

            Message message = new Message()
            {
                Text = text,
                From = From,
                Chat = Chat,
            };

            await TelegramBotFake.Instance._onMessage.Invoke(message, Telegram.Bot.Types.Enums.UpdateType.Message);
        }

        public async Task SentTextWithCheck(string text, string textResult, string[]? buttons = null, [CallerArgumentExpression(nameof(textResult))] string valueTitle = "")
        {
            await SentText(text);
            SendMessageArgs sendResult = TelegramBotFake.Instance.SendingMessage.First(x => x.chatId == Chat.Id);

            string comment = $@"text:{text} 
{valueTitle}:{textResult}";

            sendResult.text.ShouldBe(textResult, comment);
            sendResult.parseMode.ShouldBe(Telegram.Bot.Types.Enums.ParseMode.Html, comment);

            if (buttons == null)
                sendResult.replyMarkup.ShouldBeNull(comment);
            else if (buttons.Length == 0)
                sendResult.replyMarkup.ShouldBeOfType<ReplyKeyboardRemove>(comment);
            else
            {
                string[] sendButtons = ((ReplyKeyboardMarkup)Check.NotNull(sendResult.replyMarkup, valueTitle)).Keyboard.SelectMany(x => x).Select(x => x.Text).ToArray();
                sendButtons.ShouldBeEquivalentTo(buttons!, comment);
            }

        }
    }


    private UserChatHelper CreateRandomUser(string firstName)
    {
        User from = new User()
        {
            Id = _random.NextUniqueInt64(),
            FirstName = firstName,
            LastName = "test LastName"
        };

        Chat chat = new Chat() { Id = _random.NextUniqueInt64() };

        return new UserChatHelper()
        {
            Chat = chat,
            From = from,
        };
    }


    private DialogModel GetModel()
    {
        IOptions<TgBotServiceOptions> opt = factory.Services.GetRequiredService<IOptions<TgBotServiceOptions>>();

        DialogModelLoader dialogLoader = new DialogModelLoader(opt.Value.DialogModelPath);

        return dialogLoader.GetModel();
    }

    private async Task<ChatEntry> EnsureChat(IApplication app, long chatID, string name, Action<ChatEntry>? update = null)
    {
        ChatEntry? chat = await app.ChatService.Find(chatID);

        if (chat == null)
            chat = await app.ChatService.Create(chatID, name, name, name, chatID);


        if (update != null)
        {
            update(chat);
            await app.ChatService.Update(chat);
        }

        return chat;
    }
}