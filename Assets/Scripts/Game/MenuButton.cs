// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using DG.Tweening;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Range(0.0f, 1.0f)]
    public float glowIntensity = 0.3f;

    [SerializeField]
    protected Image onHoverGlow;

    [SerializeField]
    protected Image onHoverOverlay;

    public UnityEvent onClickEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverGlow.DOKill();
        onHoverOverlay.DOKill();
        onHoverGlow.DOFade(glowIntensity, 0.5f);
        onHoverOverlay.DOFade(1.0f, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverGlow.DOKill();
        onHoverOverlay.DOKill();
        onHoverGlow.DOFade(0.0f, 0.25f);
        onHoverOverlay.DOFade(0.0f, 0.25f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickEvent.Invoke();
    }
}