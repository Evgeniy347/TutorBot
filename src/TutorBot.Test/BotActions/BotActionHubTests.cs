using Shouldly;
using TutorBot.TelegramService.BotActions;
using TutorBot.TelegramService.BotActions.Admins;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.Test.BotActions;

[Trait("Category", "Unit")]
public class BotActionHubTests
{
    private static DialogModel CreateTestModel()
    {
        return new DialogModel
        {
            Start = new StartNodeModel { Handler = "start", NextStep = "menu" },
            Handlers = new HandlersModel
            {
                Welcome = new WelcomeHandler
                {
                    Key = "/start",
                    WelcomeText = "Привет!",
                    ErrorText = "Ошибка!",
                    GroupNumbers = ["101", "102"]
                },
                Schedule = new ScheduleItem { Key = "Расписание", Text = ["расписание"] },
                SimpleText = [
                    new SimpleTextItem { Key = "Помощь", Text = ["справка"] },
                ],
                YandexSearchText = []
            },
            Menus = [
                new MenuItem { Key = "Главное меню", Text = "Выберите пункт", Buttons = ["Помощь", "Расписание"] },
            ]
        };
    }

    [Fact]
    public void FindHandler_SimpleTextKey_ReturnsSimpleTextBotAction()
    {
        var model = CreateTestModel();
        var handler = BotActionHub.FindHandler(model, "Помощь");
        handler.ShouldNotBeNull();
        handler.ShouldBeOfType<SimpleTextBotAction>();
        handler.Key.ShouldBe("Помощь");
    }

    [Fact]
    public void FindHandler_ScheduleKey_ReturnsScheduleAction()
    {
        var model = CreateTestModel();
        var handler = BotActionHub.FindHandler(model, "Расписание");
        handler.ShouldNotBeNull();
        handler.Key.ShouldBe("Расписание");
    }

    [Fact]
    public void FindHandler_ALKey_ReturnsALBotAction()
    {
        var model = CreateTestModel();
        var handler = BotActionHub.FindHandler(model, "Спросить нейросеть");
        handler.ShouldNotBeNull();
        handler.ShouldBeOfType<ALBotAction>();
    }

    [Fact]
    public void FindHandler_AdminKey_ReturnsAdminBotAction()
    {
        var model = CreateTestModel();
        var handler = BotActionHub.FindHandler(model, "/admin");
        handler.ShouldNotBeNull();
        handler.ShouldBeOfType<AdminBotAction>();
    }

    [Fact]
    public void FindHandler_ResetKey_ReturnsResetBotAction()
    {
        var model = CreateTestModel();
        var handler = BotActionHub.FindHandler(model, "Перезапустить");
        handler.ShouldNotBeNull();
        handler.ShouldBeOfType<ResetBotAction>();
    }

    [Fact]
    public void FindHandler_SubMenuKey_ReturnsSimpleSubMenuBotAction()
    {
        var model = CreateTestModel();
        var handler = BotActionHub.FindHandler(model, "Главное меню");
        handler.ShouldNotBeNull();
        handler.ShouldBeOfType<SimpleSubMenuBotAction>();
    }

    [Fact]
    public void FindHandler_NotFound_ReturnsNull()
    {
        var model = CreateTestModel();
        var handler = BotActionHub.FindHandler(model, "Несуществующий ключ");
        handler.ShouldBeNull();
    }

    [Fact]
    public void FindHandler_NotFoundWithThrow_Throws()
    {
        var model = CreateTestModel();
        Should.Throw<Exception>(() => BotActionHub.FindHandler(model, "Несуществующий ключ", throwNotFound: true));
    }

    [Fact]
    public void AdminHandles_ContainsStatisticBotAction()
    {
        BotActionHub.AdminHandles.ShouldContain(h => h is StatisticBotAction);
    }

    [Fact]
    public void AdminHandles_ContainsNotifyBotAction()
    {
        BotActionHub.AdminHandles.ShouldContain(h => h is NotifyBotAction);
    }

    [Fact]
    public void AdminHandles_HasExactlyTwoActions()
    {
        BotActionHub.AdminHandles.Length.ShouldBe(2);
    }

    [Fact]
    public void GetAdminMenuKeyboard_ReturnsThreeButtons()
    {
        var keyboard = BotActionHub.GetAdminMenuKeyboard();
        var rows = keyboard.Keyboard.ToList();
        rows.Count.ShouldBe(3);
        rows[0].First().Text.ShouldBe("Получить статистику");
        rows[1].First().Text.ShouldBe("Оповещения об ошибках");
        rows[2].First().Text.ShouldBe("↩️ В главное меню");
    }
}
