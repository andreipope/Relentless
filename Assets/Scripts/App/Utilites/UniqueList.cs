using System;
using System.Collections;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    /// <summary>
    /// A list that can only contain unique items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UniqueList<T> : IList<T>
    {
        private readonly IList<T> _list;

        public UniqueList()
        {
            _list = new List<T>();
        }

        public UniqueList(int capacity)
        {
            _list = new List<T>(capacity);
        }

        public UniqueList(IEnumerable<T> collection)
        {
            _list = new List<T>();
            foreach (T item in collection)
            {
                ThrowIfContains(item);
                _list.Add(item);
            }
        }

        public UniqueList(IList<T> list)
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

        public void Add(T item)
        {
            ThrowIfContains(item);
            _list.Add(item);
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

        public int Count => _list.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ThrowIfContains(item);
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public T this[int index]
        {
            get => _list[index];
            set
            {
                if (!Equal(_list[index], value))
                {
                    ThrowIfContains(value);
                }

                _list[index] = value;
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                Add(item);
            }
        }

        public UniqueList<T> FindAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            UniqueList<T> objList = new UniqueList<T>();
            for (int index = 0; index < _list.Count; ++index)
            {
                if (match(_list[index]))
                    objList.Add(_list[index]);
            }

            return objList;
        }

        public int FindIndex(Predicate<T> match)
        {
            return FindIndex(0, _list.Count, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return FindIndex(startIndex, _list.Count - startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            if ((uint) startIndex > (uint) _list.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || startIndex > _list.Count - count)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            int num = startIndex + count;
            for (int index = startIndex; index < num; ++index)
            {
                if (match(_list[index]))
                    return index;
            }

            return -1;
        }

        private void ThrowIfContains(T item)
        {
            if (_list.Contains(item))
                throw new ArgumentException($"Item '{item}' is already in the list");
        }

        private static bool Equal(T item1, T item2)
        {
            return EqualityComparer<T>.Default.Equals(item1, item2);
        }
    }
}
