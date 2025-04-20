using System.Text;
using Telegram.Bot.Types;

namespace TutorBot.TelegramService.BotActions.Admins
{
    internal class StatisticBotAction : IBotAction
    {
        public string Key => "Получить статистику";
        public bool EnableProlangate => false;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            var chats = await client.App.ChatService.GetChats();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@$"ChatsCount:{chats.Length}
MessagesCount:{chats.Select(x => x.MessagesCount).Sum()}
");

            await client.SendMessage(sb.ToString());
        }
    }
}
