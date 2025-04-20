using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TutorBot.TelegramService.BotActions
{
    internal class ResetBotAction : IBotAction
    {
        public string Key => "Перезапустить";

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            client.ChatEntry.GroupNumber = string.Empty;
            client.ChatEntry.SessionID = Guid.NewGuid();

            await client.SendMessage(TextMessages.WelcomeMessage, replyMarkup: new ReplyKeyboardRemove());
        }
    }
}
