using System.Globalization;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// Расширения текстового формтирования.
    /// </summary>
    internal static class FormattingExtensions
    {
        /// <summary>
        /// Заключает строку <paramref name="str"/> в двойные кавычки.
        /// </summary>
        /// <param name="str">Строка.</param>
        public static string DoubleQuotes(this string str) => $"\"{str}\"";

        /// <summary>
        /// Заключает текстовое значение <paramref name="intValue"/> в двойные кавычки.
        /// </summary>
        /// <param name="intValue">Целочисленное значение.</param>
        public static string DoubleQuotes(this int intValue) => $"\"{intValue.ToString(CultureInfo.InvariantCulture)}\"";

        /// <summary>
        /// Заключает строку <paramref name="str"/> в одинарные кавычки.
        /// </summary>
        /// <param name="str">Строка.</param>
        public static string SingleQuotes(this string str) => $"'{str}'";

        /// <summary>
        /// Заключает строку <paramref name="str"/> в квадратные скобки.
        /// </summary>
        /// <param name="str">Строка.</param>
        public static string SquareBrackets(this string str) => $"[{str}]";

        /// <summary>
        /// Заключает строку <paramref name="str"/> в круглые скобки.
        /// </summary>
        /// <param name="str">Строка.</param>
        public static string RoundBrackets(this string str) => $"({str})";

        /// <summary>
        /// Соединяет строки перечисления строк при помощи заданного разделителя.
        /// </summary>
        /// <param name="values">Перечисление строк.</param>
        /// <param name="separator">Разделитель, при помощи которого соединяются строки.</param>
        public static string JoinStrings(this IEnumerable<string> values, string separator = ",") =>
            string.Join(separator, Check.NotNull(values));

        /// <summary>
        /// Соединяет строки перечисления строк при помощи заданного разделителя.
        /// </summary>
        /// <param name="sourses">Перечисление строк.</param>
        /// <param name="separator">Разделитель, при помощи которого соединяются строки.</param>
        public static string JoinStrings(this IEnumerable<string> values, char separator = ',') =>
            string.Join(separator, Check.NotNull(values));

        /// <summary>
        /// Соединяет строки перечисления строк при помощи заданного разделителя.
        /// </summary>
        /// <param name="values">Перечисление строк.</param>
        /// <param name="separator">Разделитель, при помощи которого соединяются строки.</param>
        public static string JoinString<TSourse>(this IEnumerable<TSourse> values, string separator = ",") =>
            string.Join(separator, Check.NotNull(values));

        /// <summary>
        /// Удаляет набор строк из начала и конца строки.
        /// </summary>
        /// <param name="value">строки</param>
        /// <param name="trimValues">набор строк</param>
        public static string Trim(this string value, params string[] trimValues)
        {
            if (string.IsNullOrEmpty(value) || trimValues == null || trimValues.Length == 0)
                return value;
            New:

            foreach (string trimValue in trimValues)
            {
                bool change = false;
                while (value.StartsWith(trimValue))
                {
                    value = value.Substring(trimValue.Length);
                    change = true;
                }
                while (value.EndsWith(trimValue))
                {
                    value = value.Remove(value.Length - trimValue.Length);
                    change = true;
                }
                if (change)
                    goto New;
            }

            return value;
        }

        /// <summary>
        /// Заключает строку <paramref name="str"/> в двойные кавычки.
        /// </summary>
        /// <param name="str">Строка.</param>
        public static string DoubleQuotes(this IStringWrapper str) => $"\"{str}\"";

        /// <summary>
        /// Заключает строку <paramref name="str"/> в одинарные кавычки.
        /// </summary>
        /// <param name="str">Строка.</param>
        public static string SingleQuotes(this IStringWrapper str) => $"'{str}'";

        /// <summary>
        /// Заключает строку <paramref name="str"/> в квадратные скобки.
        /// </summary>
        /// <param name="str">Строка.</param>
        public static string SquareBrackets(this IStringWrapper str) => $"[{str}]";

        /// <summary>
        /// Заключает строку <paramref name="str"/> в круглые скобки.
        /// </summary>
        /// <param name="str">Строка.</param>
        public static string RoundBrackets(this IStringWrapper str) => $"({str})";

        /// <summary>
        /// Соединяет строки перечисления строк при помощи заданного разделителя.
        /// </summary>
        /// <param name="values">Перечисление строк.</param>
        /// <param name="separator">Разделитель, при помощи которого соединяются строки.</param>
        public static string JoinStrings(this IEnumerable<IStringWrapper> values, string separator)
        {
            ArgumentNullException.ThrowIfNull(values);
            return string.Join(separator, values);
        }

        /// <summary>
        /// Приводит строку к нижнему регистру.
        /// </summary>
        /// <param name="str">Строка.</param>
        /// <returns></returns>
        public static string ToLower(this IStringWrapper str) =>
            str.ToString()?.ToLowerInvariant() ?? string.Empty;

        public static bool Equals(this IStringWrapper str, string value) =>
            str.ToString()?.Equals(value) ?? value == null;

        public static bool Equals(this IStringWrapper str, string value, StringComparison comparisonType) =>
            str.ToString()?.Equals(value, comparisonType) ?? value == null;

        /// <summary>
        /// Удаляет избыточные знаки табуляции, пробелы, вставленные после переноса строк при форматировании внутреннего содержимого Xml-элемента.
        /// </summary>
        /// <param name="innerText">Внутреннее содержимое Xml-элемента.</param>
        public static string RemoveContentWhitespaces(this string innerText)
        {
            if (string.IsNullOrEmpty(innerText))
                return innerText;

            //выполняем замену пустых символов.
            string encodedText = Regex.Replace(innerText, @"^\r?\n([ \t]*)", string.Empty);
            encodedText = Regex.Replace(encodedText, @"\r?\n([ \t]*)", Environment.NewLine);

            //заменяем повторяющиеся пробелы и знаки табуляции на одиночный символ.
            //заменить их нужно вместе, поскольку одиночный пробел рядом со знаком табуляции не отобразится.
            encodedText = Regex.Replace(encodedText, @"\t{2,}", "\t");
            encodedText = Regex.Replace(encodedText, @" {2,}", " ");

            //возвращаем закодированный текст.
            return encodedText;
        }

        /// <summary>
        /// Возвращает полный стэк исключений в виде списка, начиная с текущего исключения и включая все вложенные исключения.
        /// </summary>
        /// <param name="stackTopException">Исключение, расположенное вверху стэка исключений.</param>
        public static IReadOnlyList<Exception> FullExceptionStack(this Exception stackTopException)
        {
            ArgumentNullException.ThrowIfNull(stackTopException);

            List<Exception> fullExceptionStack = new();
            Exception? currentException = stackTopException;
            while (currentException != null)
            {
                fullExceptionStack.Add(currentException);
                currentException = currentException.InnerException;
            }
            return fullExceptionStack.AsReadOnly();
        }

        /// <summary>
        /// Возвращает стэк сообщений исключений из всего стэка исключений.
        /// </summary>
        /// <param name="stackTopException">Исключение, расположенное вверху стэка исключений.</param>
        public static string GetMessageStack(this Exception stackTopException) =>
            stackTopException.FullExceptionStack().Select(ex => $"{ex.GetType().FullName}: {ex.Message}").JoinStrings(Environment.NewLine);
    }

    /// <summary>
    /// Представляет обёртку экземпляра <see cref="string"/>.
    /// Используется для реализации расширений над <see cref="IStringWrapper"/>, аналогичных расширениям над <see cref="string"/>.
    /// Класс реализующий интерфейс <see cref="IStringWrapper"/> должен переопределить метод <see cref="object.ToString"/>, возвращающим значение, которое будет использовано в методах расширения над <see cref="IStringWrapper"/>.
    /// </summary>
    public interface IStringWrapper
    {
    }
}