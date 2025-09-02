
namespace TutorBot.Test.Helpers
{
    internal class UniqueRandomGenerator
    {
        // Хранит список уникальных выданных случайных чисел
        private HashSet<long> _generatedNumbers = new HashSet<long>();

        // Генератор случайных чисел
        private readonly Random _random = new Random();

        private readonly object _randomLock = new object();

        public long NextUniqueInt64()
        {
            lock (_randomLock)
            {
                while (true)
                {
                    // Генерируем случайное 64-разрядное целое число
                    long number = _random.NextInt64();

                    // Проверяем, было ли оно уже выдано раньше
                    if (!_generatedNumbers.Contains(number))
                    {
                        // Если число уникальное, добавляем его в список и возвращаем
                        _generatedNumbers.Add(number);
                        return number;
                    }
                }
            }
        }
    }
}
