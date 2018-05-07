// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using DG.Tweening;

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

    public bool interactable
    {
        get { return _interactable; }
        set { _interactable = value;
            if (!_interactable)
            {
                onHoverOverlay.DOKill();
                onClickOverlay.DOKill();
                onHoverOverlay.DOFade(0.0f, 0.3f);
                onClickOverlay.DOFade(0.0f, 0.3f);
                button.DOFade(0.5f, 0.3f);
            }
            else
            {
                button.DOFade(1f, 0.3f);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_interactable)
        {
            onHoverOverlay.DOKill();
            onHoverOverlay.DOFade(1.0f, 0.5f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_interactable)
        {
            onHoverOverlay.DOKill();
            onHoverOverlay.DOFade(0.0f, 0.25f);
        }
    }

	public void OnPointerDown(PointerEventData eventData)
	{
        if (_interactable)
        {
            onHoverOverlay.DOKill();
            onHoverOverlay.DOFade(0.0f, 0.25f);
            onClickOverlay.DOKill();
            onClickOverlay.DOFade(1.0f, 0.2f);
        }
	}

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_interactable)
        {
            onClickOverlay.DOKill();
            onClickOverlay.DOFade(0.0f, 0.25f);
            onClickEvent.Invoke();
        }
    }
}