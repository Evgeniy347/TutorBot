using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TutorBot.TelegramService.BotActions
{
    internal class ALBotAction : IBotAction
    {
        public static ALBotAction Instance = new ALBotAction();
        public bool EnableProlongated => true;

        public string Key => "Спросить нейросеть";

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            if (message.Text == Key)
            {
                await client.SendMessage("Сформулируйте вопрос");
            }
            else
            {
                string answer = await client.App.ALService.TransferQuestionAL(client.ChatEntry.ChatID, message.Text ?? string.Empty, client.ChatEntry.SessionID);

                var answerEscape = TelegramMarkdownHelper.EscapeMarkdownV2(answer);
                await client.SendMessage(answerEscape, parseMode: ParseMode.MarkdownV2);
            }
        }
    }

    public static class TelegramMarkdownHelper
    {
        // Символы, которые нужно экранировать в MarkdownV2
        private static readonly char[] _specialChars = { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

        // Регулярное выражение для поиска уже оформленных Markdown-элементов (**жирный**, _курсив_ и т.д.)
        private static readonly Regex _markdownRegex = new Regex(@"(?<!\\)(\*\*.*?\*\*|__.*?__|```.*?```|`.*?`|\*.*?\*|_.*?_|~~.*?~~)", RegexOptions.Compiled);

        /// <summary>
        /// Экранирует спецсимволы, но сохраняет Markdown-разметку.
        /// </summary>
        public static string EscapeMarkdownV2(string text)
        {
            // Находим все существующие Markdown-элементы
            var matches = _markdownRegex.Matches(text);
            var result = new StringBuilder();
            int lastPos = 0;

            // Обрабатываем текст между Markdown-элементами
            foreach (Match match in matches)
            {
                // Добавляем и экранируем текст перед Markdown-элементом
                result.Append(EscapeText(text.Substring(lastPos, match.Index - lastPos)));
                // Добавляем сам Markdown-элемент без изменений
                result.Append(match.Value);
                lastPos = match.Index + match.Length;
            }

            // Добавляем оставшийся текст после последнего Markdown-элемента
            result.Append(EscapeText(text.Substring(lastPos)));

            return result.ToString();
        }

        /// <summary>
        /// Экранирует все спецсимволы в тексте.
        /// </summary>
        private static string EscapeText(string text)
        {
            var result = new StringBuilder();
            foreach (char c in text)
            {
                if (_specialChars.Contains(c))
                {
                    result.Append('\\');
                }
                result.Append(c);
            }
            return result.ToString();
        }
    }
}
