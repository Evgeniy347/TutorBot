using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.TelegramService.BotActions.Admins;

namespace TutorBot.TelegramService.BotActions;

internal class BotActionHub
{
    public static IBotAction[] AdminHandles { get; } = [
        new StatisticBotAction(),
        new NotifyBotAction()
    ];

    public static IBotAction? FindHandler(DialogModel model, string? key, bool throwNotFound = false)
    {
        try
        {
            IBotAction[] handlers = [
                .. model.Handlers.SimpleText.Select(x => new SimpleTextBotAction(model, x)),
                .. model.Menus.Select(x => new SimpleSubMenuBotAction(x)),
                .. model.Handlers.YandexSearchText.Select(x => new YandexSearchAction(model, x) ),
                new ScheduleAction(model),
                ALBotAction.Instance,
                new AdminBotAction(),
                new ResetBotAction(model.Handlers.Welcome.WelcomeText)
            ];

            if (throwNotFound)
                return handlers.Single(x => x.Key == key);

            return handlers.SingleOrDefault(x => x.Key == key);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"invalid key '{key}'", e);
        }
    }

    public static ReplyKeyboardMarkup GetAdminMenuKeyboard(bool enableAdminError = false)
    {
        string notifyText = enableAdminError
            ? "Выключить оповещение об ошибках"
            : "Включить оповещение об ошибках";

        return new ReplyKeyboardMarkup(
        [
            [ new KeyboardButton("Получить статистику") ],
            [ new KeyboardButton(notifyText) ],
            [ new KeyboardButton("↩️ В главное меню") ]
        ]);
    }
}
