using TutorBot.TelegramService.BotActions;
using TutorBot.TelegramService.Helpers;
using TutorBot.Test.Helpers;
using TutorBot.Test.TestFramework;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.Test.Common;

[DatabaseSnapshotGroup]
public class AdminBotActionTest(CustomAppFactory factory) : IntegrationTestsBase
{
    private readonly TestHelper _helper = new TestHelper(factory);

    string[] _admin_buttons = [
        "Получить статистику",
        "Оповещения об ошибках",
        "↩️ В главное меню"
        ];

    [Fact]
    public async Task SuccessLogin()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = _helper.Model;
        UserChatHelper chatHelper = _helper.CreateRandomUser("test user");
        await _helper.CompleteWelcomeFlow(chatHelper, model);
        int key = System.DateTime.Now.Hour * 5;

        // Act & Assert - Complete initial flow 
        await chatHelper.SentTextWithCheck("/admin", "Введите код доступа");

        await chatHelper.SentTextWithCheck("invalid", "Код доступа введен с ошибкой");
        await chatHelper.SentTextWithCheck(key.ToString(), "Теперь вы администратор", _admin_buttons);
    }

    [Fact]
    public async Task CheckLimitLogin()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = _helper.Model;
        UserChatHelper chatHelper = _helper.CreateRandomUser("test user");
        await _helper.CompleteWelcomeFlow(chatHelper, model);
        int key = System.DateTime.Now.Hour * 5;

        // Act & Assert - Complete initial flow 
        await chatHelper.SentTextWithCheck("/admin", "Введите код доступа");

        for (int i = 0; i < 10; i++)
            await chatHelper.SentTextWithCheck("invalid", "Код доступа введен с ошибкой", valueTitle: $"iteration:{i}");

        await chatHelper.SentTextWithCheck("invalid", "Вам запрещено вводить код доступа", valueTitle: $"iteration:invalid");
        await chatHelper.SentTextWithCheck(key.ToString(), "Вам запрещено вводить код доступа", valueTitle: $"iteration:valid");
    }

    [Fact]
    public async Task CheckLogin()
    {
        // Arrange
        using HttpClient client = await factory.CreateApplication();
        DialogModel model = _helper.Model;
        MenuItem menu = model.Menus.Single(x => x.Key == "↩️ В главное меню");
        string fullName = "иванов иван иванович";
        string menuText = StringHelpers.ReplaceUserName(menu.Text, fullName);
        UserChatHelper chatHelper = _helper.CreateRandomUser("test user");
        await _helper.CompleteWelcomeFlow(chatHelper, model);
        int key = System.DateTime.Now.Hour * 5;
        await chatHelper.SentTextWithCheck("/admin", "Введите код доступа");
        await chatHelper.SentTextWithCheck(key.ToString(), "Теперь вы администратор", _admin_buttons);

        // Act & Assert - Complete initial flow 
        await chatHelper.SentTextWithCheck("↩️ В главное меню", menuText, menu.Buttons);
        await chatHelper.SentTextWithCheck("/admin", "Выберите опцию:", _admin_buttons);
    }
}
