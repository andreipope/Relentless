using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool isToggleEnabled;

    public MenuButtonToggleEvent onValueChangedEvent = new MenuButtonToggleEvent();

    [SerializeField]
    protected Image onHoverOverlayToggleDisabled, onClickOverlayToggleDisabled, onHoverOverlayToggleEnabled, onClickOverlayToggleEnabled, buttonEnabled, buttonDisabled;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isToggleEnabled)
        {
            onHoverOverlayToggleEnabled.DOKill();
            onHoverOverlayToggleEnabled.DOFade(0.0f, 0.25f);
            onClickOverlayToggleEnabled.DOKill();
            onClickOverlayToggleEnabled.DOFade(1.0f, 0.2f);
        } else
        {
            onHoverOverlayToggleDisabled.DOKill();
            onHoverOverlayToggleDisabled.DOFade(0.0f, 0.25f);
            onClickOverlayToggleDisabled.DOKill();
            onClickOverlayToggleDisabled.DOFade(1.0f, 0.2f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isToggleEnabled)
        {
            onHoverOverlayToggleEnabled.DOKill();
            onHoverOverlayToggleEnabled.DOFade(1.0f, 0.5f);
        } else
        {
            onHoverOverlayToggleDisabled.DOKill();
            onHoverOverlayToggleDisabled.DOFade(1.0f, 0.5f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isToggleEnabled)
        {
            onHoverOverlayToggleEnabled.DOKill();
            onHoverOverlayToggleEnabled.DOFade(0.0f, 0.25f);
        } else
        {
            onHoverOverlayToggleDisabled.DOKill();
            onHoverOverlayToggleDisabled.DOFade(0.0f, 0.25f);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isToggleEnabled)
        {
            onHoverOverlayToggleEnabled.enabled = false;
            onClickOverlayToggleEnabled.enabled = false;
            buttonEnabled.enabled = false;

            onHoverOverlayToggleDisabled.enabled = true;
            onClickOverlayToggleDisabled.enabled = true;
            buttonDisabled.enabled = true;

            onHoverOverlayToggleDisabled.color = new Color(1, 1, 1, 0);
            onClickOverlayToggleDisabled.color = new Color(1, 1, 1, 0);
        } else
        {
            onHoverOverlayToggleDisabled.enabled = false;
            onClickOverlayToggleDisabled.enabled = false;
            buttonDisabled.enabled = false;

            onHoverOverlayToggleEnabled.enabled = true;
            onClickOverlayToggleEnabled.enabled = true;
            buttonEnabled.enabled = true;

            onHoverOverlayToggleEnabled.color = new Color(1, 1, 1, 0);
            onClickOverlayToggleEnabled.color = new Color(1, 1, 1, 0);
        }

        isToggleEnabled = !isToggleEnabled;

        if (onValueChangedEvent != null)
        {
            onValueChangedEvent.Invoke(isToggleEnabled);
        }
    }

    public void SetStatus(bool status)
    {
        isToggleEnabled = !status;
        OnPointerUp(null);
    }

    private void Awake()
    {
    }

    private void Start()
    {
        if (isToggleEnabled)
        {
            buttonEnabled.enabled = true;
            buttonDisabled.enabled = false;
        } else
        {
            buttonEnabled.enabled = false;
            buttonDisabled.enabled = true;
        }
    }

    [Serializable]
    public class MenuButtonToggleEvent : UnityEvent<bool>
    {
    }
}
