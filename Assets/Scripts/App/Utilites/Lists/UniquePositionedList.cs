using System;
using System.Collections;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class UniquePositionedList<T> : IPositionedList<T>
    {
        private readonly IPositionedList<T> _list;

        public UniquePositionedList(IPositionedList<T> list)
        {
            _list = list ?? throw new ArgumentNullException(nameof(list));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int Count => _list.Count;

        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        public bool IsReadOnly => _list.IsReadOnly;

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }
    }
}
