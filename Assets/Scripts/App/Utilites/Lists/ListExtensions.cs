using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Loom.ZombieBattleground
{
    public static class ListExtensions
    {
        public static UniqueList<TSource> ToUniqueList<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new UniqueList<TSource>(new List<TSource>(source));
        }

        public static UniquePositionedList<TSource> ToUniquePositionedList<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new UniquePositionedList<TSource>(new PositionedList<TSource>(source));
        }

        public static void ShuffleList<T>(this IList<T> list)
        {
            Random rnd = new Random();
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void ShuffleList<T>(this IPositionedList<T> list)
        {
            Random rnd = new Random();
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static List<T> GetRandomElementsFromList<T>(this IReadOnlyList<T> list, int count)
        {
            List<T> shuffledList = new List<T>(count);
            shuffledList.AddRange(list);

            if (list.Count <= count)
                return shuffledList;

            ShuffleList(shuffledList);
            return shuffledList.GetRange(0, count);
        }

        public static List<TItem> FindAll<TItem>(this IReadOnlyList<TItem> list, Predicate<TItem> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            List<TItem> filteredList = new List<TItem>();
            for (int index = 0; index < list.Count; ++index)
            {
                if (match(list[index]))
                    filteredList.Add(list[index]);
            }

            return filteredList;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, T item) {
            EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
            return FindIndex(list, obj => equalityComparer.Equals(obj, item));
        }

        public static int FindIndex<T>(this IReadOnlyList<T> list, Predicate<T> match)
        {
            return FindIndex(list, 0, list.Count, match);
        }

        public static int FindIndex<T>(this IReadOnlyList<T> list, int startIndex, Predicate<T> match)
        {
            return FindIndex(list, startIndex, list.Count - startIndex, match);
        }

        public static int FindIndex<T>(this IReadOnlyList<T> list, int startIndex, int count, Predicate<T> match)
        {
            if ((uint) startIndex > (uint) list.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || startIndex > list.Count - count)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            int num = startIndex + count;
            for (int index = startIndex; index < num; ++index)
            {
                if (match(list[index]))
                    return index;
            }

            return -1;
        }
        
        public static List<TItem> FindAll<TItem>(this IList<TItem> list, Predicate<TItem> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            List<TItem> filteredList = new List<TItem>();
            for (int index = 0; index < list.Count; ++index)
            {
                if (match(list[index]))
                    filteredList.Add(list[index]);
            }

            return filteredList;
        }

        public static int IndexOf<T>(this IList<T> list, T item) {
            EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
            return FindIndex(list, obj => equalityComparer.Equals(obj, item));
        }

        public static int FindIndex<T>(this IList<T> list, Predicate<T> match)
        {
            return FindIndex(list, 0, list.Count, match);
        }

        public static int FindIndex<T>(this IList<T> list, int startIndex, Predicate<T> match)
        {
            return FindIndex(list, startIndex, list.Count - startIndex, match);
        }

        public static int FindIndex<T>(this IList<T> list, int startIndex, int count, Predicate<T> match)
        {
            if ((uint) startIndex > (uint) list.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || startIndex > list.Count - count)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            int num = startIndex + count;
            for (int index = startIndex; index < num; ++index)
            {
                if (match(list[index]))
                    return index;
            }

            return -1;
        }

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            if (list == null) 
                throw new ArgumentNullException(nameof(list));
            
            if (items == null) 
                throw new ArgumentNullException(nameof(items));

            if (list is List<T> fullList)
            {
                fullList.AddRange(items);
            }
            else
            {
                foreach (T item in items)
                {
                    list.Add(item);
                }
            }
        }
    }
}
