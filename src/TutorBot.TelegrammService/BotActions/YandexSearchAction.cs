using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.TelegramService.BotActions
{
    internal class YandexSearchAction(MenuItem menu, YandexSearchTextItem item) : IBotAction
    {
        public string Key => item.Key;
        public bool EnableProlongated => true;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            ReplyKeyboardMarkup replyMarkup = menu.Buttons.Select(x => new[] { new KeyboardButton(x) }).ToArray();

            if (message.Text == Key || string.IsNullOrEmpty(message.Text))
            {
                await client.SendMessage(item.Descriptions, replyMarkup: replyMarkup, parseMode: ParseMode.Html);
            }
            else
            {
                if (!string.IsNullOrEmpty(item.Pattern))
                {
                    bool isValid = Regex.IsMatch(message.Text, item.Pattern);

                    if (!isValid)
                    {
                        await client.SendMessage(item.InvalidPatternMessage!, replyMarkup: replyMarkup, parseMode: ParseMode.Html);
                        return;
                    }
                }

                string text = item.GetText().Replace("{Text}", message.Text).Replace("{Text:URI}", message.Text.Replace(" ", "+"));
                await client.SendMessage(text, replyMarkup: replyMarkup, parseMode: ParseMode.Html);
            }
        }
    }
}
