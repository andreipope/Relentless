using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Loom.ZombieBattleground
{
    public class UIBehaviourEventNotifier : MonoBehaviour, IScrollHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<PointerEventData> OnScrollInvoked;
        public event Action<PointerEventData> OnPointerDownInvoked;
        public event Action<PointerEventData> OnPointerUpInvoked;

        public void OnScroll(PointerEventData eventData)
        {
            OnScrollInvoked?.Invoke(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownInvoked?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpInvoked?.Invoke(eventData);
        }
    }
}
