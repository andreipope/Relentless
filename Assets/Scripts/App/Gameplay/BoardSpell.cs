using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
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

            _eventHandler.OnDestroyEvent += OnDestroyEventHandler;
        }

        public event Action SpellOnUsedEvent;

        private void OnDestroyEventHandler(GameObject obj)
        {
            if (TargetingArrow != null)
            {
                Object.Destroy(TargetingArrow.gameObject);
                TargetingArrow = null;
            }

            SpellOnUsedEvent?.Invoke();
        }
    }
}
