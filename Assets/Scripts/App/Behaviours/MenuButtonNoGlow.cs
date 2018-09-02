using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonNoGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent OnClickEvent;

    public bool IsHovered = true;

    [SerializeField]
    protected Image Button;

    [SerializeField]
    protected Image OnHoverOverlay;

    [SerializeField]
    protected Image OnClickOverlay;

    private bool _interactable = true;

    public bool Interactable
    {
        get => _interactable;
        set
        {
            _interactable = value;

            // Debug.Log("_interactable = "+ _interactable);
            if (!_interactable)
            {
                OnHoverOverlay.DOKill();
                OnClickOverlay.DOKill();
                OnHoverOverlay.DOFade(0.0f, 0.3f);
                OnClickOverlay.DOFade(0.0f, 0.3f);
                Button.DOFade(0.5f, 0.3f);

                if (Button == null)
                    return;

                DoFadeForChildren(Button, 0.5f, 0.3f);
                DoFadeForChildren(OnHoverOverlay, 0, 0.3f);
                DoFadeForChildren(OnClickOverlay, 0, 0.3f);
            }
            else
            {
                OnHoverOverlay.DOKill();
                OnClickOverlay.DOKill();
                Button.DOKill();
                OnHoverOverlay.DOFade(0.0f, 0.3f);
                OnClickOverlay.DOFade(0.0f, 0.3f);
                Button.DOFade(1f, 0.3f);

                if (Button == null)
                    return;

                DoFadeForChildren(Button, 1f, 0.3f);
                DoFadeForChildren(OnHoverOverlay, 0, 0.3f);
                DoFadeForChildren(OnClickOverlay, 0, 0.3f);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Debug.Log("OnPointerDown");
        if (Interactable)
        {
            if (IsHovered)
            {
                OnHoverOverlay.DOKill();
                OnHoverOverlay.DOFade(0.0f, 0.25f);
            }

            OnClickOverlay.DOKill();
            OnClickOverlay.DOFade(1.0f, 0.2f);

            if (Button == null)
                return;

            DoFadeForChildren(Button, 0f, 0.25f);
            if (IsHovered)
            {
                DoFadeForChildren(OnHoverOverlay, 0, 0.25f);
            }

            DoFadeForChildren(OnClickOverlay, 1, 0.25f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("OnPointerEnter");
        if (Interactable)
        {
            OnHoverOverlay.DOKill();
            OnHoverOverlay.DOFade(1.0f, 0.5f);

            if (Button == null)
                return;

            DoFadeForChildren(Button, 0f, 0.25f);
            DoFadeForChildren(OnHoverOverlay, 1, 0.25f);
            if (!Input.GetMouseButton(0))
            {
                DoFadeForChildren(OnClickOverlay, 0, 0.25f);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Debug.Log("OnPointerExit");
        if (Interactable)
        {
            OnHoverOverlay.DOKill();
            OnHoverOverlay.DOFade(0.0f, 0.25f);

            if (Button == null)
                return;

            DoFadeForChildren(Button, 1f, 0.25f);
            DoFadeForChildren(OnHoverOverlay, 0, 0.25f);
            if (!Input.GetMouseButton(0))
            {
                DoFadeForChildren(OnClickOverlay, 0, 0.25f);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Debug.Log("OnPointerUp");
        if (Interactable)
        {
            OnClickOverlay.DOKill();
            OnClickOverlay.DOFade(0.0f, 0.25f);
            OnClickEvent.Invoke();

            if (Button == null)
                return;

            DoFadeForChildren(Button, 0f, 0.25f);
            DoFadeForChildren(OnHoverOverlay, 1, 0.25f);
            DoFadeForChildren(OnClickOverlay, 0, 0.25f);
        }
    }

    public void DoFadeForChildren(Image parent, float val, float duration)
    {
        TextMeshProUGUI[] tms = parent.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI item in tms)
        {
            item.DOKill();
            item.DOFade(val, duration);
        }

        Image[] imgs = parent.GetComponentsInChildren<Image>();
        foreach (Image item in imgs)
        {
            item.DOKill();
            item.DOFade(val, duration);
        }
    }
}
