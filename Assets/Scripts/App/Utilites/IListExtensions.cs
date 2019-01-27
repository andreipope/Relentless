using System;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public static class IListExtensions
    {
        public static UniqueList<TSource> ToUniqueList<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new UniqueList<TSource>(source);
        }
    }
}
