using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleScrollNotifier : MonoBehaviour, IScrollHandler
{
    public event Action<Vector2> Scrolled;

    public void OnScroll(PointerEventData eventData)
    {
        Scrolled?.Invoke(eventData.scrollDelta);
    }
}
