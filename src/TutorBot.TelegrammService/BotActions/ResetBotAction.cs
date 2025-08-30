using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TutorBot.TelegramService.BotActions
{
    internal class ResetBotAction(string welcomeText) : IBotAction
    {
        public string Key => "Перезапустить";
        public bool EnableProlongated => false;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            client.ChatEntry.FullName = string.Empty;
            client.ChatEntry.GroupNumber = string.Empty;
            client.ChatEntry.SessionID = Guid.NewGuid();

            await client.App.ChatService.Update(client.ChatEntry);

            await client.SendMessage(welcomeText, replyMarkup: new ReplyKeyboardRemove());
        }
    }
}
