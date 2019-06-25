using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ObjectList<T> : MonoBehaviour where T : Object
    {
        [SerializeField]
        private T[] _items = new T[0];

        public IReadOnlyList<T> Items => _items;
    }
}
