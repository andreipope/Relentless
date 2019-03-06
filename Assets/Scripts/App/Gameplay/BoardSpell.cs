using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class BoardSpell : OwnableBoardObject
    {
        public GameObject GameObject;

        public Transform Transform;

        public WorkingCard Card;

        private readonly OnBehaviourHandler _eventHandler;

        public BoardSpell(GameObject obj, WorkingCard card)
        {
            GameObject = obj;
            Card = card;

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
