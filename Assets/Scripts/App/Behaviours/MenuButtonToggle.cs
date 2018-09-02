using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuButtonToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool IsToggleEnabled;

    public MenuButtonToggleEvent OnValueChangedEvent = new MenuButtonToggleEvent();

    [SerializeField]
    [FormerlySerializedAs("onHoverOverlayToggleDisabled")]
    protected Image OnHoverOverlayToggleDisabled;

    [SerializeField]
    [FormerlySerializedAs("ClickOverlayToggleDisabled")]
    protected Image OnClickOverlayToggleDisabled;

    [SerializeField]
    [FormerlySerializedAs("onHoverOverlayToggleEnabled")]
    protected Image OnHoverOverlayToggleEnabled;

    [SerializeField]
    [FormerlySerializedAs("onClickOverlayToggleEnabled")]
    protected Image OnClickOverlayToggleEnabled;

    [SerializeField]
    [FormerlySerializedAs("buttonEnabled")]
    protected Image ButtonEnabled;

    [SerializeField]
    [FormerlySerializedAs("buttonDisabled")]
    protected Image ButtonDisabled;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsToggleEnabled)
        {
            OnHoverOverlayToggleEnabled.DOKill();
            OnHoverOverlayToggleEnabled.DOFade(0.0f, 0.25f);
            OnClickOverlayToggleEnabled.DOKill();
            OnClickOverlayToggleEnabled.DOFade(1.0f, 0.2f);
        }
        else
        {
            OnHoverOverlayToggleDisabled.DOKill();
            OnHoverOverlayToggleDisabled.DOFade(0.0f, 0.25f);
            OnClickOverlayToggleDisabled.DOKill();
            OnClickOverlayToggleDisabled.DOFade(1.0f, 0.2f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsToggleEnabled)
        {
            OnHoverOverlayToggleEnabled.DOKill();
            OnHoverOverlayToggleEnabled.DOFade(1.0f, 0.5f);
        }
        else
        {
            OnHoverOverlayToggleDisabled.DOKill();
            OnHoverOverlayToggleDisabled.DOFade(1.0f, 0.5f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsToggleEnabled)
        {
            OnHoverOverlayToggleEnabled.DOKill();
            OnHoverOverlayToggleEnabled.DOFade(0.0f, 0.25f);
        }
        else
        {
            OnHoverOverlayToggleDisabled.DOKill();
            OnHoverOverlayToggleDisabled.DOFade(0.0f, 0.25f);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsToggleEnabled)
        {
            OnHoverOverlayToggleEnabled.enabled = false;
            OnClickOverlayToggleEnabled.enabled = false;
            ButtonEnabled.enabled = false;

            OnHoverOverlayToggleDisabled.enabled = true;
            OnClickOverlayToggleDisabled.enabled = true;
            ButtonDisabled.enabled = true;

            OnHoverOverlayToggleDisabled.color = new Color(1, 1, 1, 0);
            OnClickOverlayToggleDisabled.color = new Color(1, 1, 1, 0);
        }
        else
        {
            OnHoverOverlayToggleDisabled.enabled = false;
            OnClickOverlayToggleDisabled.enabled = false;
            ButtonDisabled.enabled = false;

            OnHoverOverlayToggleEnabled.enabled = true;
            OnClickOverlayToggleEnabled.enabled = true;
            ButtonEnabled.enabled = true;

            OnHoverOverlayToggleEnabled.color = new Color(1, 1, 1, 0);
            OnClickOverlayToggleEnabled.color = new Color(1, 1, 1, 0);
        }

        IsToggleEnabled = !IsToggleEnabled;

        OnValueChangedEvent?.Invoke(IsToggleEnabled);
    }

    public void SetStatus(bool status)
    {
        IsToggleEnabled = !status;
        OnPointerUp(null);
    }

    private void Awake()
    {
    }

    private void Start()
    {
        if (IsToggleEnabled)
        {
            ButtonEnabled.enabled = true;
            ButtonDisabled.enabled = false;
        }
        else
        {
            ButtonEnabled.enabled = false;
            ButtonDisabled.enabled = true;
        }
    }

    [Serializable]
    public class MenuButtonToggleEvent : UnityEvent<bool>
    {
    }
}
