using Telegram.Bot.Types;

namespace TutorBot.TelegramService.BotActions.Admins
{
    internal class NotifyBotAction : IBotAction
    {
        public string Key => "Оповещения об ошибках";
        public bool EnableProlangate => false;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            client.ChatEntry.EnableAdminError = !client.ChatEntry.EnableAdminError;
            await client.SendMessage("EnableAdminError:" + client.ChatEntry.EnableAdminError);
        }
    }
}
