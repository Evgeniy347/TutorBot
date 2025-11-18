using System.Text;
using Telegram.Bot.Types;
using TutorBot.Abstractions;

namespace TutorBot.TelegramService.BotActions.Admins
{
    internal class StatisticBotAction : IBotAction
    {
        public string Key => "Получить статистику";
        public bool EnableProlongated => false;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            var chats = await client.App.ChatService.GetChats();

            StringBuilder sb = new StringBuilder();

            ChatSummaryReport report = await client.App.ChatService.GetSummaryInfo();

            string htmlReport = GenerateHtmlReport(report);

            await client.SendMessage(htmlReport, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }

        public static string GenerateHtmlReport(ChatSummaryReport report)
        {
            var html = new StringBuilder();

            // Заголовок
            html.AppendLine("<b>📊 Статистика чатов</b>\n");

            // Суммарная информация
            html.AppendLine($"<b>Всего чатов:</b> {report.NumberOfChats}");
            html.AppendLine($"<b>Всего сообщений:</b> {report.NumberOfMessages}\n");

            // Статистика по группам
            html.AppendLine("<b>📈 Статистика по группам</b>");
            html.AppendLine("<pre>");
            html.AppendLine("Группа      | Пользователей | Сообщений");
            html.AppendLine("----------------------------------------");

            foreach (var group in report.GroupSummaries.OrderByDescending(g => g.MessageCount))
            {
                html.AppendLine($"{group.GroupNumber.PadRight(10)} | {group.UserCount.ToString().PadRight(13)} | {group.MessageCount}");
            }

            html.AppendLine("</pre>\n");

            // Топ 100 пользователей
            html.AppendLine("<b>🏆 Топ 100 пользователей по количеству сообщений</b>");
            html.AppendLine("<pre>");
            html.AppendLine("#   | Сообщений | ФИО");
            html.AppendLine("----------------------------------------");

            int rank = 1;
            foreach (var user in report.TopUsers.Take(100))
            {
                var truncatedName = user.FullName.Length > 25 ? user.FullName.Substring(0, 25) + "..." : user.FullName;
                html.AppendLine($"{rank.ToString().PadRight(3)} | {user.MessageCount.ToString().PadRight(9)} | {truncatedName}");
                rank++;
            }

            html.AppendLine("</pre>\n");

            // Средние обращения по часам (только для основных групп)
            var mainGroups = report.GroupSummaries
                .Where(g => !string.IsNullOrEmpty(g.GroupNumber) && g.GroupNumber != "Unknown")
                .OrderBy(g => g.GroupNumber)
                .Take(5); // Ограничиваем количество групп для читаемости

            html.AppendLine("<b>⏰ Среднее количество обращений по часам (последние 20 дней)</b>");

            foreach (var group in mainGroups)
            {
                html.AppendLine($"\n<b>Группа {group.GroupNumber}:</b>");
                html.AppendLine("<pre>");
                html.AppendLine("Час | Сообщений");
                html.AppendLine("----------------------");

                var groupAverages = report.HourlyAverages
                    .Where(a => a.GroupNumber == group.GroupNumber)
                    .OrderBy(a => a.Hour);

                foreach (var avg in groupAverages)
                {
                    html.AppendLine($"{avg.Hour.ToString().PadRight(2)}:00 | {avg.MessageCount:F2}");
                }

                html.AppendLine("</pre>");
            }

            return html.ToString();
        }
    }
}
