using System;
using System.Collections.Generic;

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
        
        public static List<TItem> FindAll<TList, TItem>(this IList<TItem> list, Predicate<TItem> match)
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
    }
}
