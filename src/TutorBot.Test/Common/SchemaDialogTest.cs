using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using System.Runtime.CompilerServices;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;
using TutorBot.TelegramService;
using TutorBot.TelegramService.BotActions;
using TutorBot.TelegramService.Helpers;
using TutorBot.Test.Helpers;
using TutorBot.Test.TestFramework;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.Test.Common;

[DatabaseSnapshotGroup]
public class SchemaDialogTest(CustomAppFactory factory) : IntegrationTestsBase
{
    private readonly UniqueRandomGenerator _random = new UniqueRandomGenerator();

    [Fact]
    public async Task App_Welcome_StressTest()
    {
        using (HttpClient client = await factory.CreateApplication())
        {
            await Parallel.ForAsync(0, 500, async (x, y) => await TestFullScenario($"user-{x}"));
        }
    }

    [Theory]
    [InlineData("first user")]
    [InlineData("second user")]
    public async Task App_Welcome(string firstName)
    {
        using (HttpClient client = await factory.CreateApplication())
        {
            await TestFullScenario(firstName);
        }
    }

    #region Individual Tests

    [Fact]
    public async Task Should_Show_Welcome_Text_On_Start_Command()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = GetModel();
        UserChatHelper chatHelper = CreateRandomUser("test user");

        // Act & Assert
        await chatHelper.SentTextWithCheck("/start", model.Handlers.Welcome.WelcomeText, []);
    }

    [Fact]
    public async Task Should_Show_Welcome_Text_On_DoubleStart_Command()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = GetModel();
        UserChatHelper chatHelper = CreateRandomUser("test user");

        // Act & Assert
        await chatHelper.SentTextWithCheck("/start", model.Handlers.Welcome.WelcomeText, []);
        await chatHelper.SentTextWithCheck("/start", model.Handlers.Welcome.WelcomeText, []);
    }

    [Fact]
    public async Task Should_Show_Error_Text_On_Invalid_Input_After_Start()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = GetModel();
        UserChatHelper chatHelper = CreateRandomUser("test user");

        // Act & Assert
        await chatHelper.SentTextWithCheck("/start", model.Handlers.Welcome.WelcomeText, []);
        await chatHelper.SentTextWithCheck("xxx", model.Handlers.Welcome.ErrorText, []);
    }

    [Fact]
    public async Task Should_Ask_FullName_After_Valid_Group_Input()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = GetModel();
        UserChatHelper chatHelper = CreateRandomUser("test user");

        // Act & Assert
        await chatHelper.SentTextWithCheck("/start", model.Handlers.Welcome.WelcomeText, []);
        await chatHelper.SentTextWithCheck("РИ-421056", model.Handlers.Welcome.FullNameQuestion!, []);
    }

    [Fact]
    public async Task Should_Show_FullName_Error_On_Invalid_Name_Input()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = GetModel();
        UserChatHelper chatHelper = CreateRandomUser("test user");

        // Act & Assert
        await chatHelper.SentTextWithCheck("/start", model.Handlers.Welcome.WelcomeText, []);
        await chatHelper.SentTextWithCheck("РИ-421056", model.Handlers.Welcome.FullNameQuestion!, []);
        await chatHelper.SentTextWithCheck("!1$%", model.Handlers.Welcome.FullNameError!);
    }

    [Fact]
    public async Task Should_Show_Main_Menu_After_Valid_FullName_Input()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = GetModel();
        MenuItem menu = model.Menus.Single(x => x.Key == "↩️ В главное меню");
        UserChatHelper chatHelper = CreateRandomUser("test user");
        string fullName = "иванов иван иванович";
        string menuText = StringHelpers.ReplaceUserName(menu.Text, fullName);

        // Act & Assert
        await chatHelper.SentTextWithCheck("/start", model.Handlers.Welcome.WelcomeText, []);
        await chatHelper.SentTextWithCheck("РИ-421056", model.Handlers.Welcome.FullNameQuestion!, []);
        await chatHelper.SentTextWithCheck(fullName, menuText, menu.Buttons);
    }

    [Fact]
    public async Task Should_Show_YandexSearchText()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = GetModel();
        MenuItem menu = model.Menus.Single(x => x.Key == "↩️ В главное меню");
        YandexSearchTextItem yandexSearchText = model.Handlers.YandexSearchText.Single(x => x.Key == "👨‍🏫 Найти преподавателя");
        UserChatHelper chatHelper = CreateRandomUser("test user");
        string name = "иванов иван иванович";

        string text = yandexSearchText.GetText().Replace("{Text}", name).Replace("{Text:URI}", name.Replace(" ", "+"));

        // Act & Assert - Complete initial flow
        await CompleteWelcomeFlow(chatHelper, model);

        // Act & Assert
        await chatHelper.SentTextWithCheck("👨‍🏫 Найти преподавателя", yandexSearchText.Descriptions, menu.Buttons);
        await chatHelper.SentTextWithCheck("!1$%", yandexSearchText.InvalidPatternMessage!, menu.Buttons);
        await chatHelper.SentTextWithCheck(name, text, menu.Buttons);
    }

    [Fact]
    public async Task Should_Navigate_To_SubMenu_And_Back()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = GetModel();
        MenuItem menu = model.Menus.Single(x => x.Key == "↩️ В главное меню");
        MenuItem subMenu = model.Menus.Single(x => x.Key == "📚 Ликвидации академических задолженностей");
        SimpleTextItem simpleTextSubMenu = model.Handlers.SimpleText.Single(x => x.Key == "❓ Сколько у меня долгов?");
        UserChatHelper chatHelper = CreateRandomUser("test user");
        string fullName = "иванов иван иванович";
        string menuText = StringHelpers.ReplaceUserName(menu.Text, fullName);

        // Act & Assert - Navigate to submenu
        await CompleteWelcomeFlow(chatHelper, model);
        await chatHelper.SentTextWithCheck("📚 Ликвидации академических задолженностей", subMenu.Text, subMenu.Buttons);

        // Act & Assert - Use submenu item
        await chatHelper.SentTextWithCheck("❓ Сколько у меня долгов?", simpleTextSubMenu.GetText(), subMenu.Buttons);

        // Act & Assert - Return to main menu
        await chatHelper.SentTextWithCheck("↩️ В главное меню", menuText, menu.Buttons);
    }

    [Fact]
    public async Task Should_Restart_Application_On_Restart_Command()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = GetModel();
        UserChatHelper chatHelper = CreateRandomUser("test user");
        MenuItem menu = model.Menus.Single(x => x.Key == "↩️ В главное меню");
        string fullName = "иванов иван иванович";
        string menuText = StringHelpers.ReplaceUserName(menu.Text, fullName);

        // Act & Assert - Complete initial flow
        await CompleteWelcomeFlow(chatHelper, model);

        // Act & Assert - Restart and verify welcome text
        await chatHelper.SentTextWithCheck("Перезапустить", model.Handlers.Welcome.WelcomeText, []);
        await chatHelper.SentTextWithCheck("РИ-421056", model.Handlers.Welcome.FullNameQuestion!, []); 
        await chatHelper.SentTextWithCheck(fullName, menuText, menu.Buttons);
    }

    #endregion

    #region Helper Methods

    private async Task CompleteWelcomeFlow(UserChatHelper chatHelper, DialogModel model)
    {
        MenuItem menu = model.Menus.Single(x => x.Key == "↩️ В главное меню");
        string fullName = "иванов иван иванович";
        string menuText = StringHelpers.ReplaceUserName(menu.Text, fullName);

        await chatHelper.SentTextWithCheck("/start", model.Handlers.Welcome.WelcomeText, []);
        await chatHelper.SentTextWithCheck("РИ-421056", model.Handlers.Welcome.FullNameQuestion!, []);

        await chatHelper.SentTextWithCheck(fullName, menuText, menu.Buttons);
    }

    private async Task TestFullScenario(string firstName)
    { 
        DialogModel model = GetModel();

        MenuItem menu = model.Menus.Single(x => x.Key == "↩️ В главное меню");
        MenuItem subMenu = model.Menus.Single(x => x.Key == "📚 Ликвидации академических задолженностей");
        SimpleTextItem simpleTextMenu = model.Handlers.SimpleText.Single(x => x.Key == "📅 Расписание группы");
        SimpleTextItem simpleTextSubMenu = model.Handlers.SimpleText.Single(x => x.Key == "❓ Сколько у меня долгов?");
        string fullName = "иванов иван иванович";
        string menuText = StringHelpers.ReplaceUserName(menu.Text, fullName);

        UserChatHelper chatHelper = CreateRandomUser(firstName);

        await CompleteWelcomeFlow(chatHelper, model);
        await chatHelper.SentTextWithCheck("📚 Ликвидации академических задолженностей", subMenu.Text, subMenu.Buttons);
        await chatHelper.SentTextWithCheck("❓ Сколько у меня долгов?", simpleTextSubMenu.GetText(), subMenu.Buttons);
        await chatHelper.SentTextWithCheck("↩️ В главное меню", menuText, menu.Buttons);
        await chatHelper.SentTextWithCheck("📅 Расписание группы", simpleTextMenu.GetText(), menu.Buttons);
        await chatHelper.SentTextWithCheck("Перезапустить", model.Handlers.Welcome.WelcomeText, []);
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

    #endregion
}