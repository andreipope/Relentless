using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class BoardItem1 : OwnableBoardObject, IBoardUnitView
    {
        public GameObject GameObject { get; }

        public Transform Transform { get; }

        public BoardUnitModel Model { get; }

        private readonly OnBehaviourHandler _eventHandler;

        public BoardItem1(GameObject obj, BoardUnitModel boardUnitModel)
        {
            GameObject = obj;
            Model = boardUnitModel;

            if (GameObject != null)
            {
                Transform = obj.transform;

                _eventHandler = GameObject.GetComponent<OnBehaviourHandler>();

                _eventHandler.Destroying += DestroyingHandler;
            }
        }

        public event Action Used;

        private void DestroyingHandler(GameObject obj)
        {
            Used?.Invoke();
        }

        public override string ToString()
        {
            return $"([{GetType().Name}] {nameof(Model)}: {Model})";
        }
    }
}
