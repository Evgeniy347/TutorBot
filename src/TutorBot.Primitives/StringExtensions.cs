namespace System
{
    /// <summary>
    /// Расширения строки.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Возвращает строку в нижнем регистре, если строка отличается от <see langword="null"/>, в ином случае возвращает <see langword="null"/>.
        /// </summary>
        /// <param name="str">Строка.</param>
#pragma warning disable CA1308 // intentional lowercase conversion
        public static string TryToLower(this string str)
        {
            if (str == null)
                return string.Empty;
            return str.ToLowerInvariant();
        }
#pragma warning restore CA1308

        /// <summary>
        /// Возвращает строку в верхнем регистре, если строка отличается от <see langword="null"/>, в ином случае возвращает <see langword="null"/>.
        /// </summary>
        /// <param name="str">Строка.</param>
        public static string TryToUpper(this string str)
        {
            if (str == null)
                return string.Empty;
            return str.ToUpperInvariant();
        }

        public static string TryTrim(this string str)
        {
            if (str == null)
                return string.Empty;
            return str.Trim();
        }

        public static string Replace(this string str, string key, object value) =>
            str?.Replace(key, value?.ToString(), StringComparison.Ordinal) ?? string.Empty;

        public static string ReplaceKey(this string str, string key, object value)
        {
            string replaceValue = string.Empty;
            if (value != null)
                replaceValue = value.ToString()!;
            if (!string.IsNullOrEmpty(str))
                str = str.Replace("{" + key + "}", replaceValue, StringComparison.Ordinal);
            return str;
        }
    }
}