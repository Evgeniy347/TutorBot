using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace System
{
    /// <summary>
    /// Содержит методы проверки аргументов.
    /// </summary>
    internal static class Check
    {
        /// <summary>
        /// Проверяет, является ли значение отличным от <see langword="null"/>. Генерирует исключение, если проверяемое значение равно <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="value">Значение.</param>
        /// <param name="valueTitle">Заголовок значения.</param>
        /// <returns>Возвращает значение, успешно прошедшее проверку.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        [return: NotNull]
        public static T NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string valueTitle = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(valueTitle);
            ArgumentNullException.ThrowIfNull(value, valueTitle);
            return value;
        }

        /// <summary>
        /// Проверяет, является ли значение отличным от <see langword="null"/>. Генерирует исключение, если проверяемое значение равно <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="value">Значение.</param>
        /// <param name="valueTitle">Заголовок значения.</param>
        /// <returns>Возвращает значение, успешно прошедшее проверку.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        [return: NotNull]
        public static T[] NotEmpty<T>([NotNull] T[]? value, [CallerArgumentExpression(nameof(value))] string valueTitle = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(valueTitle);
            ArgumentNullException.ThrowIfNull(value, valueTitle);

            if (value.Length == 0)
                throw new ArgumentException("len is 0");

            return value;
        }

        /// <summary>
        /// Проверяет, является ли значение отличным от пустой строки или строки, состоящей из пустых символов. Генерирует исключение, если проверяемое значение является пустой строкой или строкой, состоящей из пустых символов.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="valueTitle">Заголовок значения.</param>
        /// <returns>Возвращает значение, успешно прошедшее проверку.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        [return: NotNull]
        public static string NotEmpty(string? value, [CallerArgumentExpression(nameof(value))] string valueTitle = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(valueTitle);
            ArgumentException.ThrowIfNullOrEmpty(value, valueTitle);
            return value;
        }

        /// <summary>
        /// Проверяет, является ли значение отличным от <see cref="Guid.Empty"/>. Генерирует исключение, если проверяемое значение равно <see cref="Guid.Empty"/>.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="valueTitle">Заголовок значения.</param>
        /// <returns>Возвращает значение, успешно прошедшее проверку.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        [return: NotNull]
        public static Guid NotEmpty(Guid value, [CallerArgumentExpression(nameof(value))] string valueTitle = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(valueTitle);

            if (value == Guid.Empty)
                throw new ArgumentException($"The {valueTitle.DoubleQuotes()} value cannot be empty.", valueTitle);
            return value;
        }

        /// <summary>
        /// Проверяет, является ли значение отличным от <see cref="DateTime.MinValue"/>. Генерирует исключение, если проверяемое значение равно <see cref="DateTime.MinValue"/>.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="valueTitle">Заголовок значения.</param>
        /// <returns>Возвращает значение, успешно прошедшее проверку.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        [return: NotNull]
        public static DateTime NotEmpty(DateTime value, [CallerArgumentExpression(nameof(value))] string valueTitle = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(valueTitle);

            if (value == DateTime.MinValue)
                throw new ArgumentException($"The {valueTitle.DoubleQuotes()} value cannot be empty.", valueTitle);
            return value;
        }

        /// <summary>
        /// Проверяет, является ли значение отличным от <see cref="TimeSpan.Zero"/>. Генерирует исключение, если проверяемое значение равно <see cref="TimeSpan.Zero"/>.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="valueTitle">Заголовок значения.</param>
        /// <returns>Возвращает значение, успешно прошедшее проверку.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        [return: NotNull]
        public static TimeSpan NotZero(TimeSpan value, [CallerArgumentExpression(nameof(value))] string valueTitle = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(valueTitle);

            if (value == TimeSpan.Zero)
                throw new ArgumentException($"The {valueTitle.DoubleQuotes()} value cannot be empty.", valueTitle);
            return value;
        }

        /// <summary>
        /// Проверяет, является ли значение положительным числом. Генерирует исключение, если значение не является положительным числом.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="valueTitle">Заголовок значения.</param>
        /// <returns>Возвращает значение, успешно прошедшее проверку.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static int IsPositive(int value, [CallerArgumentExpression(nameof(value))] string valueTitle = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(valueTitle);

            if (value <= 0)
                throw new ArgumentException($"The {valueTitle.DoubleQuotes()} value must be greater than zero.", valueTitle);
            return value;
        }

        /// <summary>
        /// Проверяет, является ли значение положительным числом. Генерирует исключение, если значение не является положительным числом.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="valueTitle">Заголовок значения.</param>
        /// <returns>Возвращает значение, успешно прошедшее проверку.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static int NotZero(int value, [CallerArgumentExpression(nameof(value))] string valueTitle = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(valueTitle);

            if (value == 0)
                throw new ArgumentException($"The {valueTitle.DoubleQuotes()} value must not equals to zero.", valueTitle);
            return value;
        }

        /// <summary>
        /// Проверяет, является ли значение положительным интервалом времени. Генерирует исключение, если значение не является положительным интервалом времени.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="valueTitle">Заголовок значения.</param>
        /// <returns>Возвращает значение, успешно прошедшее проверку.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static TimeSpan IsPositive(TimeSpan value, [CallerArgumentExpression(nameof(value))] string valueTitle = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(valueTitle);

            if (value <= TimeSpan.Zero)
                throw new ArgumentException($"The {valueTitle.DoubleQuotes()} value must be greater than {TimeSpan.Zero}.", valueTitle);
            return value;
        }

        /// <summary>
        /// Проверяет, является ли значение положительным числом или равным 0. Генерирует исключение, если значение меньше, чем 0.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="valueTitle">Заголовок значения.</param>
        /// <returns>Возвращает значение, успешно прошедшее проверку.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static int IsPositiveOrZero(int value, [CallerArgumentExpression(nameof(value))] string valueTitle = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(valueTitle);

            if (value < 0)
                throw new ArgumentException($"The {valueTitle.DoubleQuotes()} value must be greater or equal to zero.", valueTitle);
            return value;
        }

        /// <summary>
        /// Проверяет, является ли значение положительным интервалом времени или равным <see cref="TimeSpan.Zero"/>. Генерирует исключение, если значение меньше, чем <see cref="TimeSpan.Zero"/>.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="valueTitle">Заголовок значения.</param>
        /// <returns>Возвращает значение, успешно прошедшее проверку.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static TimeSpan IsPositiveOrZero(TimeSpan value, [CallerArgumentExpression(nameof(value))] string valueTitle = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(valueTitle);

            if (value < TimeSpan.Zero)
                throw new ArgumentException($"The {valueTitle.DoubleQuotes()} value must be greater or equal to {TimeSpan.Zero}.", valueTitle);
            return value;
        }
    }
}