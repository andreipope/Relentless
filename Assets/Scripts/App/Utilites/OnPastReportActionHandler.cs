using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Loom.ZombieBattleground
{
    public class OnPastReportActionHandler : MonoBehaviour, IPointerDownHandler
    {
        public event Action<PointerEventData> PointerDowned;

        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDowned?.Invoke(eventData);
        }
    }
}
