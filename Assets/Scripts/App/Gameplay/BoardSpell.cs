using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class BoardSpell
    {
        public GameObject GameObject;

        public Transform Transform;

        public BoardArrow TargetingArrow;

        public WorkingCard Card;

        private readonly OnBehaviourHandler _eventHandler;

        public BoardSpell(GameObject obj, WorkingCard card)
        {
            GameObject = obj;
            Transform = obj.transform;

            Card = card;

            _eventHandler = GameObject.GetComponent<OnBehaviourHandler>();

            _eventHandler.Destroying += DestroyingHandler;
        }

        public event Action Used;

        private void DestroyingHandler(GameObject obj)
        {
            if (TargetingArrow != null)
            {
                Object.Destroy(TargetingArrow.gameObject);
                TargetingArrow = null;
            }

            Used?.Invoke();
        }
    }
}
