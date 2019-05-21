using System;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public static class EnumerableExtensions
    {
        public static T FirstOrDefault<T>(this IEnumerable<T> items, T defaultValue)
        {
            foreach (T item in items)
            {
                return item;
            }
            return defaultValue;
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> items, Func<T, bool> predicate, T defaultValue)
        {
            return items.Where(predicate).FirstOrDefault(defaultValue);
        }

        public static T LastOrDefault<T>(this IEnumerable<T> items, T defaultValue)
        {
            return items.Reverse().FirstOrDefault(defaultValue);
        }

        public static T LastOrDefault<T>(this IEnumerable<T> items, Func<T, bool> predicate, T defaultValue)
        {
            return items.Where(predicate).LastOrDefault(defaultValue);
        }
    }
}
