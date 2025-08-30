using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TutorBot.TelegramService.BotActions
{
    internal class SimpleTextBotAction(string key, string text) : IBotAction
    {
        public string Key => key;
        public bool EnableProlongated => false;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        { 
            await client.SendMessage(text, parseMode: ParseMode.Html);
        }
    }
}
