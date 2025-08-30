using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.TelegramService.BotActions
{
    internal class SimpleSubMenuBotAction(MenuItem menu) : IBotAction
    {
        public string Key => menu.Key;
        public bool EnableProlongated => true;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            ReplyKeyboardMarkup replyMarkup = menu.Buttons.Select(x => new[] { new KeyboardButton(x) }).ToArray();

            await client.SendMessage(menu.Text, replyMarkup: replyMarkup);
        }
    }
}
