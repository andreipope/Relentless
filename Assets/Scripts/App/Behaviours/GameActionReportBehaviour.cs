using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameActionReportBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action OnPointerEnterEvent;
    public event Action OnPointerExitEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke();
    }
}
