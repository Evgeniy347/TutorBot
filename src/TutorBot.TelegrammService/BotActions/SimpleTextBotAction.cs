using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.TelegramService.Helpers;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.TelegramService.BotActions
{
    internal class SimpleTextBotAction(MenuItem menu, string key, string text) : IBotAction
    {
        public string Key => key;
        public bool EnableProlongated => false;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            ReplyKeyboardMarkup replyMarkup = menu.Buttons.Select(x => new[] { new KeyboardButton(x) }).ToArray();

            string resultText = StringHelpers.ReplaceUserName(text, client.ChatEntry.FullName);

            await client.SendMessage(resultText, replyMarkup: replyMarkup, parseMode: ParseMode.Html);
        }
    }
}
