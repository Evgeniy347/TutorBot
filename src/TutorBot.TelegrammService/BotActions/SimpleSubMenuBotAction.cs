using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.TelegramService.Helpers;
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

            string text = StringHelpers.ReplaceUserName(menu.Text, client.ChatEntry.FullName);

            await client.SendMessage(text, replyMarkup: replyMarkup, parseMode: ParseMode.Html);
        }
    }
}
