using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.TelegramService.BotActions
{
    internal class YandexSearchAction(YandexSearchTextItem item) : IBotAction
    {
        public string Key => item.Key;
        public bool EnableProlongated => true;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            if (message.Text == Key || string.IsNullOrEmpty(message.Text))
            {
                await client.SendMessage(item.Descriptions);
            }
            else
            {
                if (!string.IsNullOrEmpty(item.Pattern))
                {
                    bool isValid = Regex.IsMatch(message.Text, item.Pattern);

                    if (!isValid)
                    {
                        await client.SendMessage(item.InvalidPatternMessage!);
                        return;
                    }
                }

                await client.SendMessage(item.Text.Replace("{Text}", message.Text).Replace("{Text:URI}", message.Text.Replace(" ", "+")));
            }
        }
    }
}
