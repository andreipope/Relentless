using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class BoardSpell
    {
        public GameObject gameObject;

        public Transform transform;

        public BoardArrow targetingArrow;

        public WorkingCard Card;

        private readonly OnBehaviourHandler _eventHandler;

        public BoardSpell(GameObject obj, WorkingCard card)
        {
            gameObject = obj;
            transform = obj.transform;

            Card = card;

            _eventHandler = gameObject.GetComponent<OnBehaviourHandler>();

            _eventHandler.OnDestroyEvent += OnDestroyEventHandler;
        }

        public event Action SpellOnUsedEvent;

        private void OnDestroyEventHandler(GameObject obj)
        {
            if (targetingArrow != null)
            {
                Object.Destroy(targetingArrow.gameObject);
                targetingArrow = null;
            }

            SpellOnUsedEvent?.Invoke();
        }
    }
}
