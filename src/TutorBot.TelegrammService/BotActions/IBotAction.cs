using Telegram.Bot.Types;

namespace TutorBot.TelegramService.BotActions
{
    internal interface IBotAction
    {
        string Key { get; }
        bool EnableProlangate { get; }
        Task ExecuteAsync(Message message, TutorBotContext client);
    }
}
