using Telegram.Bot.Types;
using static Telegram.Bot.Types.Enums.ParseMode;
using TutorBot.Abstractions;

namespace TutorBot.TelegramService.BotActions.Admins
{
    internal class StatisticBotAction : IBotAction
    {
        public string Key => "Получить статистику";
        public bool EnableProlongated => false;

        private const int MaxMessageLength = 4000;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            ChatSummaryReport report = await client.App.ChatService.GetSummaryInfo();

            foreach (string part in GenerateReportParts(report))
                await client.SendMessage(part, parseMode: Html);
        }

        public static IEnumerable<string> GenerateReportParts(ChatSummaryReport report)
        {
            // Часть 1: сводка и статистика по группам
            yield return BuildSummaryPart(report);

            // Часть 2: топ пользователей
            foreach (string part in BuildTopUsersParts(report))
                yield return part;

            // Часть 3: почасовая статистика
            foreach (string part in BuildHourlyStatsParts(report))
                yield return part;
        }

        private static string BuildSummaryPart(ChatSummaryReport report)
        {
            var html = new System.Text.StringBuilder();

            html.AppendLine("<b>📊 Статистика чатов</b>\n");
            html.AppendLine($"<b>Всего чатов:</b> {report.NumberOfChats}");
            html.AppendLine($"<b>Всего сообщений:</b> {report.NumberOfMessages}\n");

            html.AppendLine("<b>📈 По группам</b>");
            html.AppendLine("<pre>");
            html.AppendLine("Группа       | Пользователей | Сообщений");
            html.AppendLine("-----------------------------------------");

            foreach (GroupSummary? group in report.GroupSummaries.OrderByDescending(g => g.MessageCount))
                html.AppendLine($"{group.GroupNumber.PadRight(12)} | {group.UserCount,12} | {group.MessageCount}");

            html.Append("</pre>");
            return html.ToString();
        }

        private static IEnumerable<string> BuildTopUsersParts(ChatSummaryReport report)
        {
            var html = new System.Text.StringBuilder();
            html.AppendLine("<b>🏆 Топ пользователей</b>");
            html.AppendLine("<pre>");
            html.AppendLine("#  | Сообщ | ФИО");
            html.AppendLine("------------------------------");

            int rank = 1;
            foreach (UserMessageCount? user in report.TopUsers.Take(100))
            {
                string name = user.FullName.Length > 20 ? user.FullName[..20] + ".." : user.FullName;
                html.AppendLine($"{rank,2} | {user.MessageCount,4} | {name}");

                if (html.Length >= MaxMessageLength && rank < 100)
                {
                    html.Append("</pre>");
                    yield return html.ToString();
                    html.Clear();
                    html.AppendLine("<b>🏆 Топ пользователей (продолжение)</b>");
                    html.AppendLine("<pre>");
                    html.AppendLine("#  | Сообщ | ФИО");
                    html.AppendLine("------------------------------");
                }
                rank++;
            }

            html.Append("</pre>");
            yield return html.ToString();
        }

        private static IEnumerable<string> BuildHourlyStatsParts(ChatSummaryReport report)
        {
            IEnumerable<GroupSummary> mainGroups = report.GroupSummaries
                .Where(g => !string.IsNullOrEmpty(g.GroupNumber) && g.GroupNumber != "Unknown")
                .OrderBy(g => g.GroupNumber)
                .Take(5);

            foreach (GroupSummary? group in mainGroups)
            {
                var html = new System.Text.StringBuilder();
                html.AppendLine($"<b>⏰ Группа {group.GroupNumber} (среднее за час)</b>");
                html.AppendLine("<pre>");
                html.AppendLine("Час  | Сообщений");
                html.AppendLine("-------------------");

                foreach (HourlyAverage? avg in report.HourlyAverages
                    .Where(a => a.GroupNumber == group.GroupNumber)
                    .OrderBy(a => a.Hour))
                {
                    html.AppendLine($"{avg.Hour,2}:00 | {avg.MessageCount,6:F2}");
                }

                html.Append("</pre>");
                yield return html.ToString();
            }
        }
    }
}
