using System;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class BoardSpell
    {
        public event Action SpellOnUsedEvent;

        private OnBehaviourHandler _eventHandler;

        public GameObject gameObject;
        public Transform transform;

        public TargetingArrow targetingArrow;

        public BoardSpell(GameObject obj)
        {
            gameObject = obj;
            transform = obj.transform;

            _eventHandler = gameObject.AddComponent<OnBehaviourHandler>();

            _eventHandler.OnDestroyEvent += OnDestroyEventHandler;
        }

        private void OnDestroyEventHandler(GameObject obj)
        {
            if (targetingArrow != null)
            {
                MonoBehaviour.Destroy(targetingArrow.gameObject);
                targetingArrow = null;
            }

            SpellOnUsedEvent?.Invoke();
        }
    }
}