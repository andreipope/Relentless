using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class SimpleScrollNotifier : MonoBehaviour, IScrollHandler
{
    public event Action<Vector2> Scrolled;

    public void OnScroll(PointerEventData eventData) {
        Scrolled?.Invoke(eventData.scrollDelta);
    }
}
