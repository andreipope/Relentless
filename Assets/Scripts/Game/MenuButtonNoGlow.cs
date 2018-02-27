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
    protected Image onHoverOverlay;
	[SerializeField]
	protected Image onClickOverlay;

    public UnityEvent onClickEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverOverlay.DOKill();
        onHoverOverlay.DOFade(1.0f, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverOverlay.DOKill();
        onHoverOverlay.DOFade(0.0f, 0.25f);
    }

	public void OnPointerDown(PointerEventData eventData)
	{
		onHoverOverlay.DOKill();
		onHoverOverlay.DOFade(0.0f, 0.25f);
		onClickOverlay.DOKill();
		onClickOverlay.DOFade(1.0f, 0.2f);
	}

    public void OnPointerUp(PointerEventData eventData)
    {
		onClickOverlay.DOKill();
		onClickOverlay.DOFade(0.0f, 0.25f);
        onClickEvent.Invoke();
    }
}