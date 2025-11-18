using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.TelegramService.Helpers;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.TelegramService.BotActions
{
    internal class SimpleTextBotAction(DialogModel model, SimpleTextItem item) : IBotAction
    {
        public string Key => item.Key;
        public bool EnableProlongated => false;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            MenuItem? menu = model.Menus.FirstOrDefault(x => x.Buttons.Contains(Key));

            if (menu == null)
            {
                await client.WriteError($"not found menu '{Key}'");
                return;
            }

            ReplyKeyboardMarkup replyMarkup = menu.Buttons.Select(x => new[] { new KeyboardButton(x) }).ToArray();

            string resultText = StringHelpers.ReplaceUserName(item.GetText(), client.ChatEntry.FullName);

            await client.SendMessage(resultText, replyMarkup: replyMarkup, parseMode: ParseMode.Html);
        }
    }
}
