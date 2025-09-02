namespace System.Collections.Generic
{
    /// <summary>
    /// Расширения типа <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Возвращает <see langword="true"/>, если в перечислении содержится более одного элемента.
        /// </summary>
        /// <typeparam name="T">Тип элемента перечисления.</typeparam>
        /// <param name="source">Перечисление.</param>
        public static bool IsMultiple<T>(this IEnumerable<T> source) =>
            source.PeekCount(2);

        #region PeekCount

        /// <summary>
        /// Возвращает <see langword="true"/>, если перечисление содержит не менее чем проверяемое количество <paramref name="testCount"/> элементов.
        /// </summary>
        /// <typeparam name="T">Тип элемента перечисления.</typeparam>
        /// <param name="source">Перечисление.</param>
        /// <param name="testCount">Проверяемое количество элементов.</param>
        public static bool PeekCount<T>(this IEnumerable<T> source, int testCount) =>
            source.PeekCount(testCount, out _);

        /// <summary>
        /// Возвращает <see langword="true"/>, если перечисление содержит не менее чем проверяемое количество <paramref name="testCount"/> элементов.
        /// </summary>
        /// <typeparam name="T">Тип элемента перечисления.</typeparam>
        /// <param name="source">Перечисление.</param>
        /// <param name="testCount">Проверяемое количество элементов.</param>
        /// <param name="enumeratedCount">Проверенное количество элементов.</param>
        public static bool PeekCount<T>(this IEnumerable<T> source, int testCount, out int enumeratedCount)
        {
            Check.IsPositive(testCount);

            enumeratedCount = 0;
            if (source == null)
                return false;

            //проверяем количество элементов коллекции.
            if (source is ICollection<T> collection)
            {
                int collectionCount = collection.Count;
                if (collectionCount >= testCount)
                {
                    enumeratedCount = testCount;
                    return true;
                }
                else
                {
                    enumeratedCount = collectionCount;
                    return false;
                }
            }

            //выполняем цикл максимум в testCount итераций.
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumeratedCount++;
                    if (enumeratedCount == testCount)
                        return true;
                }
            }
            return false;
        }

        #endregion

        #region Traverse

        /// <summary>
        /// Рекурсивно получить элемент из элемента, с защитой от бесконечной рекурсии
        /// </summary>
        /// <typeparam name="T">Тип элемента перечисления.</typeparam>
        /// <param name="item">Элемент</param>
        /// <param name="childSelector">Делегат для рекурсивного извлечения элементов</param>
        /// <returns>Извлеченные рекурсивно элементы</returns>
        public static IEnumerable<T> RecursiveSelect<T>(this T item, Func<T, T?> childSelector) =>
            RecursiveSelect(item, childSelector, new HashSet<T>());

        /// <summary>
        /// Рекурсивно получить элемент из списка элементов, с защитой от бесконечной рекурсии
        /// </summary>
        /// <typeparam name="T">Тип элемента перечисления.</typeparam>
        /// <param name="items">Список элементов</param>
        /// <param name="childSelector">Делегат для рекурсивного извлечения элементов</param>
        /// <returns>Извлеченные рекурсивно элементы</returns>
        public static IEnumerable<T> RecursiveSelect<T>(this IEnumerable<T> items, Func<T, T?> childSelector)
        {
            if (items != null)
            {
                HashSet<T> set = new HashSet<T>();
                foreach (var item in items)
                {
                    foreach (var childTraverse in item.RecursiveSelect(childSelector, set))
                        yield return childTraverse;
                }
            }
        }

        /// <summary>
        /// Рекурсивно получить элементы из элемента, с защитой от бесконечной рекурсии
        /// </summary>
        /// <typeparam name="T">Тип элемента перечисления.</typeparam>
        /// <param name="items">Элемент</param>
        /// <param name="childSelector">Делегат для рекурсивного извлечения элементов</param>
        /// <returns>Извлеченные рекурсивно элементы</returns>
        public static IEnumerable<T> RecursiveSelect<T>(this T item, Func<T, IEnumerable<T>> childSelector) =>
             RecursiveSelect(item, childSelector, new HashSet<T>());

        /// <summary>
        /// Рекурсивно получить элементы из списка элементов, с защитой от бесконечной рекурсии
        /// </summary>
        /// <typeparam name="T">Тип элемента перечисления.</typeparam>
        /// <param name="items">Список элементов</param>
        /// <param name="childSelector">Делегат для рекурсивного извлечения элементов</param>
        /// <returns>Извлеченные рекурсивно элементы</returns>
        public static IEnumerable<T> RecursiveSelect<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> childSelector)
        {
            if (items != null)
            {
                HashSet<T> set = new HashSet<T>();
                foreach (var item in items)
                {
                    foreach (var childTraverse in item.RecursiveSelect(childSelector, set))
                        yield return childTraverse;
                }
            }
        }

        private static IEnumerable<T> RecursiveSelect<T>(this T item, Func<T, IEnumerable<T>> childSelector, HashSet<T> set)
        {
            if (item != null && !set.Contains(item))
            {
                set.Add(item);
                yield return item;

                var childs = childSelector(item);
                if (childs != null)
                {
                    foreach (var child in childs)
                        foreach (var childTraverse in RecursiveSelect(child, childSelector, set))
                            yield return childTraverse;
                }
            }
        }

        private static IEnumerable<T> RecursiveSelect<T>(this T item, Func<T, T?> childSelector, HashSet<T> set)
        {
            if (item != null && !set.Contains(item))
            {
                set.Add(item);
                yield return item;

                var child = childSelector(item);

                if (child != null)
                    foreach (var childTraverse in RecursiveSelect(child, childSelector, set))
                        yield return childTraverse;
            }
        }

        #endregion

        #region ChunkBy

        /// <summary>
        /// Разбить большую коллекцию на коллекции размером chunkSize
        /// </summary>
        /// <typeparam name="T">Тип элемента списка</typeparam>
        /// <param name="source">Исходный список</param>
        /// <param name="chunkSize">Размер списков</param>
        /// <returns>Списки</returns>
        /// <exception cref="ArgumentNullException">Был передан пустой список</exception>
        /// <exception cref="ArgumentException">Был передан размер списка менее единицы</exception>
        public static IEnumerable<List<T>> ChunkByList<T>(this IEnumerable<T> source, int chunkSize) =>
            source.ChunkBy(chunkSize).Select(x => x.ToList());

        /// <summary>
        /// Разбить большую коллекцию на коллекции размером chunkSize
        /// </summary>
        /// <typeparam name="T">Тип элемента списка</typeparam>
        /// <param name="source">Исходный список</param>
        /// <param name="chunkSize">Размер списков</param>
        /// <returns>Списки</returns>
        /// <exception cref="ArgumentNullException">Был передан пустой список</exception>
        /// <exception cref="ArgumentException">Был передан размер списка менее единицы</exception>
        public static IEnumerable<T[]> ChunkByArray<T>(this IEnumerable<T> source, int chunkSize) =>
            source.ChunkBy(chunkSize).Select(x => x.ToArray());

        /// <summary>
        /// Разбить большую коллекцию на коллекции размером chunkSize
        /// </summary>
        /// <typeparam name="T">Тип элемента списка</typeparam>
        /// <param name="source">Исходный список</param>
        /// <param name="chunkSize">Размер списков</param>
        /// <returns>Списки</returns>
        /// <exception cref="ArgumentNullException">Был передан пустой список</exception>
        /// <exception cref="ArgumentException">Был передан размер списка менее единицы</exception>
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (chunkSize < 1)
                throw new ArgumentException(nameof(chunkSize));

            using (EnumeratorContext<T> context = new EnumeratorContext<T>(source, chunkSize))
            {
                while (!context.End)
                {
                    context.End = !context.Enumerator.MoveNext();
                    if (!context.End)
                        yield return Take(context);
                }
            }
        }

        private static IEnumerable<T> Take<T>(EnumeratorContext<T> context)
        {
            int currentIndex = 1;
            yield return context.Enumerator.Current;
            while (currentIndex++ < context.Count)
            {
                context.End = !context.Enumerator.MoveNext();
                if (!context.End)
                {
                    context.CurrentIndex++;
                    yield return context.Enumerator.Current;
                    continue;
                }

                break;
            }
        }

        private class EnumeratorContext<T> : IDisposable
        {
            public readonly IEnumerator<T> Enumerator;
            public readonly int Count;
            public int CurrentIndex;
            public bool End;

            public EnumeratorContext(IEnumerable<T> source, int count)
            {
                Enumerator = source.GetEnumerator();
                Count = count;
            }

            public void Dispose() => Enumerator.Dispose();
        }

        #endregion


    }
}