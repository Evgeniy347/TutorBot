using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TutorBot.TelegramService.BotActions.Admins
{
    internal class NotifyBotAction : IBotAction
    {
        public string Key => "Оповещения об ошибках";
        public bool EnableProlongated => false;

        public bool MatchesKey(string? text) =>
            text is "Включить оповещение об ошибках" or "Выключить оповещение об ошибках" or "Оповещения об ошибках";

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            client.ChatEntry.EnableAdminError = !client.ChatEntry.EnableAdminError;

            string statusText = client.ChatEntry.EnableAdminError
                ? "🔔 Оповещение об ошибках включено"
                : "🔕 Оповещение об ошибках выключено";

            ReplyKeyboardMarkup keyboard = BotActionHub.GetAdminMenuKeyboard(client.ChatEntry.EnableAdminError);

            await client.SendMessage(statusText, replyMarkup: keyboard);
        }
    }
}
