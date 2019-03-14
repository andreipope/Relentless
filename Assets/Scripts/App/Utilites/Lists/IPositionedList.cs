using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public interface IPositionedList<T> : IReadOnlyList<T>
    {
        new T this[int index] { get; set; }

        bool IsReadOnly { get; }

        void Clear();

        bool Contains(T item);

        void CopyTo(T[] array, int arrayIndex);

        bool Remove(T item);

        int IndexOf(T item);

        void Insert(ItemPosition index, T item);

        void RemoveAt(ItemPosition index);
    }
}
