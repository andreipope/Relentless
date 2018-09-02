using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuButtonNoGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent OnClickEvent;

    public bool IsHovered = true;

    [SerializeField]
    [FormerlySerializedAs("button")]
    private Image _button;

    [SerializeField]
    [FormerlySerializedAs("onHoverOverlay")]
    private Image _onHoverOverlay;

    [SerializeField]
    [FormerlySerializedAs("onClickOverlay")]
    private Image _onClickOverlay;

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
                _onHoverOverlay.DOKill();
                _onClickOverlay.DOKill();
                _onHoverOverlay.DOFade(0.0f, 0.3f);
                _onClickOverlay.DOFade(0.0f, 0.3f);
                _button.DOFade(0.5f, 0.3f);

                if (_button == null)
                    return;

                DoFadeForChildren(_button, 0.5f, 0.3f);
                DoFadeForChildren(_onHoverOverlay, 0, 0.3f);
                DoFadeForChildren(_onClickOverlay, 0, 0.3f);
            }
            else
            {
                _onHoverOverlay.DOKill();
                _onClickOverlay.DOKill();
                _button.DOKill();
                _onHoverOverlay.DOFade(0.0f, 0.3f);
                _onClickOverlay.DOFade(0.0f, 0.3f);
                _button.DOFade(1f, 0.3f);

                if (_button == null)
                    return;

                DoFadeForChildren(_button, 1f, 0.3f);
                DoFadeForChildren(_onHoverOverlay, 0, 0.3f);
                DoFadeForChildren(_onClickOverlay, 0, 0.3f);
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
                _onHoverOverlay.DOKill();
                _onHoverOverlay.DOFade(0.0f, 0.25f);
            }

            _onClickOverlay.DOKill();
            _onClickOverlay.DOFade(1.0f, 0.2f);

            if (_button == null)
                return;

            DoFadeForChildren(_button, 0f, 0.25f);
            if (IsHovered)
            {
                DoFadeForChildren(_onHoverOverlay, 0, 0.25f);
            }

            DoFadeForChildren(_onClickOverlay, 1, 0.25f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("OnPointerEnter");
        if (Interactable)
        {
            _onHoverOverlay.DOKill();
            _onHoverOverlay.DOFade(1.0f, 0.5f);

            if (_button == null)
                return;

            DoFadeForChildren(_button, 0f, 0.25f);
            DoFadeForChildren(_onHoverOverlay, 1, 0.25f);
            if (!Input.GetMouseButton(0))
            {
                DoFadeForChildren(_onClickOverlay, 0, 0.25f);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Debug.Log("OnPointerExit");
        if (Interactable)
        {
            _onHoverOverlay.DOKill();
            _onHoverOverlay.DOFade(0.0f, 0.25f);

            if (_button == null)
                return;

            DoFadeForChildren(_button, 1f, 0.25f);
            DoFadeForChildren(_onHoverOverlay, 0, 0.25f);
            if (!Input.GetMouseButton(0))
            {
                DoFadeForChildren(_onClickOverlay, 0, 0.25f);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Debug.Log("OnPointerUp");
        if (Interactable)
        {
            _onClickOverlay.DOKill();
            _onClickOverlay.DOFade(0.0f, 0.25f);
            OnClickEvent.Invoke();

            if (_button == null)
                return;

            DoFadeForChildren(_button, 0f, 0.25f);
            DoFadeForChildren(_onHoverOverlay, 1, 0.25f);
            DoFadeForChildren(_onClickOverlay, 0, 0.25f);
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
