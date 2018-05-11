// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

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

                DoFadeForChildren(button.GetComponentsInChildren<TextMeshProUGUI>(), 0.5f, 0.3f);
                DoFadeForChildren(onHoverOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 0, 0.3f);
                DoFadeForChildren(onClickOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 0, 0.3f);
            }
            else
            {
                button.DOFade(1f, 0.3f);

                if (button == null) return;

                DoFadeForChildren(button.GetComponentsInChildren<TextMeshProUGUI>(), 1f, 0.3f);
                DoFadeForChildren(onHoverOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 0, 0.3f);
                DoFadeForChildren(onClickOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 0, 0.3f);
            }
        }
    }

    public void DoFadeForChildren(TextMeshProUGUI[] tms, float val, float duration)
    {
        foreach (var item in tms)
        {
            item.DOKill();
            item.DOFade(val, duration);
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

            DoFadeForChildren(button.GetComponentsInChildren<TextMeshProUGUI>(), 0f, 0.25f);
            DoFadeForChildren(onHoverOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 1, 0.25f);
            if (!Input.GetMouseButton(0))
                DoFadeForChildren(onClickOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 0, 0.25f);
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

            DoFadeForChildren(button.GetComponentsInChildren<TextMeshProUGUI>(), 1f, 0.25f);
            DoFadeForChildren(onHoverOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 0, 0.25f);
            if (!Input.GetMouseButton(0))
                DoFadeForChildren(onClickOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 0, 0.25f);
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

            DoFadeForChildren(button.GetComponentsInChildren<TextMeshProUGUI>(), 0f, 0.25f);
            if (isHovered)
                DoFadeForChildren(onHoverOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 0, 0.25f);
            DoFadeForChildren(onClickOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 1, 0.25f);
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

            DoFadeForChildren(button.GetComponentsInChildren<TextMeshProUGUI>(), 0f, 0.25f);
            DoFadeForChildren(onHoverOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 1, 0.25f);
            DoFadeForChildren(onClickOverlay.GetComponentsInChildren<TextMeshProUGUI>(), 0, 0.25f);
        }
    }
}