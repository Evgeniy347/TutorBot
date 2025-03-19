using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups; 

namespace TutorBot.TelegramService.BotActions
{
    internal class SimpleSubMenuBotAction(string key, ReplyKeyboardMarkup subMenuKeyboard) : IBotAction
    {
        public string Key => key;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            await client.SendMessage("Выберите опцию:", replyMarkup: subMenuKeyboard);
        }
    }
}
