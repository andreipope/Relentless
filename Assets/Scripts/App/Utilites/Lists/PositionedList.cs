using System.Collections;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class PositionedList<T> : IPositionedList<T>
    {
        private readonly List<T> _list;

        public PositionedList()
        {
            _list = new List<T>();
        }

        public PositionedList(int capacity)
        {
            _list = new List<T>(capacity);
        }

        public PositionedList(IEnumerable<T> collection)
        {
            _list = new List<T>(collection);
        }

        public bool IsReadOnly => false;

        public int Count => _list.Count;

        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

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

        public void Insert(ItemPosition position, T item)
        {
            Insert(position.GetIndex(this), item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public void RemoveAt(ItemPosition position)
        {
            RemoveAt(position.GetIndex(this));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _list).GetEnumerator();
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _list.InsertRange(index, collection);
        }

        public void InsertRange(ItemPosition position, IEnumerable<T> collection)
        {
            InsertRange(position.GetIndex(this), collection);
        }
    }
}
