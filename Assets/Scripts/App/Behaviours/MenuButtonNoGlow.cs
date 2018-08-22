// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using DG.Tweening;
using TMPro;

public class MenuButtonNoGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    protected Image button;
    [SerializeField]
    protected Image onHoverOverlay;
	[SerializeField]
	protected Image onClickOverlay;

    public bool _interactable = true;

    public UnityEvent onClickEvent;

    public bool isHovered = true;

    public bool interactable
    {
        get { return _interactable; }
        set { _interactable = value;

            //Debug.Log("_interactable = "+ _interactable);

            if (!_interactable)
            {
                onHoverOverlay.DOKill();
                onClickOverlay.DOKill();
                onHoverOverlay.DOFade(0.0f, 0.3f);
                onClickOverlay.DOFade(0.0f, 0.3f);
                button.DOFade(0.5f, 0.3f);

                if (button == null) return;

                DoFadeForChildren(button, 0.5f, 0.3f);
                DoFadeForChildren(onHoverOverlay, 0, 0.3f);
                DoFadeForChildren(onClickOverlay, 0, 0.3f);
            }
            else
            {
                onHoverOverlay.DOKill();
                onClickOverlay.DOKill();
                button.DOKill();
                onHoverOverlay.DOFade(0.0f, 0.3f);
                onClickOverlay.DOFade(0.0f, 0.3f);
                button.DOFade(1f, 0.3f);

                if (button == null) return;

                DoFadeForChildren(button, 1f, 0.3f);
                DoFadeForChildren(onHoverOverlay, 0, 0.3f);
                DoFadeForChildren(onClickOverlay, 0, 0.3f);
            }
        }
    }

    public void DoFadeForChildren(Image parent, float val, float duration)
    {
		TextMeshProUGUI[] tms = parent.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var item in tms)
        {
            item.DOKill();
            item.DOFade(val, duration);
        }
		Image[] imgs = parent.GetComponentsInChildren<Image> ();
		foreach (var item in imgs) {
			item.DOKill ();
			item.DOFade (val, duration);
		}
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("OnPointerEnter");

        if (_interactable)
        {
            onHoverOverlay.DOKill();
            onHoverOverlay.DOFade(1.0f, 0.5f);

            if (button == null) return;

            DoFadeForChildren(button, 0f, 0.25f);
            DoFadeForChildren(onHoverOverlay, 1, 0.25f);
            if (!Input.GetMouseButton(0))
                DoFadeForChildren(onClickOverlay, 0, 0.25f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("OnPointerExit");

        if (_interactable)
        {
            onHoverOverlay.DOKill();
            onHoverOverlay.DOFade(0.0f, 0.25f);

            if (button == null) return;

            DoFadeForChildren(button, 1f, 0.25f);
            DoFadeForChildren(onHoverOverlay, 0, 0.25f);
            if (!Input.GetMouseButton(0))
                DoFadeForChildren(onClickOverlay, 0, 0.25f);
        }
    }

	public void OnPointerDown(PointerEventData eventData)
	{
        //Debug.Log("OnPointerDown");

        if (_interactable)
        {
            if (isHovered)
            {
                onHoverOverlay.DOKill();
                onHoverOverlay.DOFade(0.0f, 0.25f);
            }
            
            onClickOverlay.DOKill();
            onClickOverlay.DOFade(1.0f, 0.2f);

            if (button == null) return;

            DoFadeForChildren(button, 0f, 0.25f);
            if (isHovered)
                DoFadeForChildren(onHoverOverlay, 0, 0.25f);
            DoFadeForChildren(onClickOverlay, 1, 0.25f);
        }
	}

    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("OnPointerUp");

        if (_interactable)
        {
            onClickOverlay.DOKill();
            onClickOverlay.DOFade(0.0f, 0.25f);
            onClickEvent.Invoke();

            if (button == null) return;

            DoFadeForChildren(button, 0f, 0.25f);
            DoFadeForChildren(onHoverOverlay, 1, 0.25f);
            DoFadeForChildren(onClickOverlay, 0, 0.25f);
        }
    }
}