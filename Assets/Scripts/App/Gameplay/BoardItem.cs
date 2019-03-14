using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class BoardItem : OwnableBoardObject, IBoardUnitView
    {
        public GameObject GameObject;

        public Transform Transform { get; }

        public BoardUnitModel Model { get; }

        private readonly OnBehaviourHandler _eventHandler;

        public BoardItem(GameObject obj, BoardUnitModel boardUnitModel)
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
    }
}
