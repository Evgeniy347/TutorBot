using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.TelegramService.BotActions.Admins;

namespace TutorBot.TelegramService.BotActions;

internal class BotActionHub
{
    public static IBotAction[] AdminHandles { get; } = [
        new StatisticBotAction(),
        new NotifyBotAction()
    ];

    public static IBotAction? FindHandler(DialogModel model   , string? key, bool throwNotFound = false)
    {
        try
        {
            IBotAction[] handlers = [
                .. model.Handlers.SimpleText.Select(x => new SimpleTextBotAction(x.Key, x.Text.JoinString(Environment.NewLine))),
                .. model.Menus.Select(x => new SimpleSubMenuBotAction(x)),
                .. model.Handlers.YandexSearchText.Select(x => new YandexSearchAction(x)),
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
            throw new Exception($"invalid key '{key}'", e);
        }
    }

    public static ReplyKeyboardMarkup GetAdminMenuKeyboard()
    {
        return new ReplyKeyboardMarkup(
        [
            [ new KeyboardButton("Получить статистику") ],
            [ new KeyboardButton("Оповещения об ошибках") ],
            [ new KeyboardButton("На главную") ]
        ]);
    }
}
