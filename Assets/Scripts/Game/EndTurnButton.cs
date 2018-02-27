// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

using DG.Tweening;

public class EndTurnButton : MonoBehaviour
{
    [HideInInspector]
    public DemoHumanPlayer player;

    [SerializeField]
    private SpriteRenderer shineSprite;

    [SerializeField]
    private SpriteRenderer hoverSprite;

    [SerializeField]
    private SpriteRenderer disabledSprite;

    private bool active;

    private void Awake()
    {
        Assert.IsNotNull(shineSprite);
        Assert.IsNotNull(hoverSprite);
        Assert.IsNotNull(disabledSprite);
    }

    public void SetEnabled(bool enabled)
    {
        disabledSprite.gameObject.SetActive(!enabled);
        active = enabled;
    }

    private void OnMouseEnter()
    {
        if (active)
        {
            shineSprite.DOKill();
            shineSprite.DOFade(1.0f, 0.4f);
            hoverSprite.DOKill();
            hoverSprite.DOFade(1.0f, 0.4f);
        }
    }

    private void OnMouseExit()
    {
        if (active)
        {
            shineSprite.DOKill();
            shineSprite.DOFade(0.0f, 0.2f);
            hoverSprite.DOKill();
            hoverSprite.DOFade(0.0f, 0.2f);
        }
    }

    private void OnMouseDown()
    {
        if (active)
        {
            player.StopTurn();
            shineSprite.DOKill();
            hoverSprite.DOKill();
            var newColor = shineSprite.color;
            newColor.a = 0.0f;
            shineSprite.color = newColor;
            newColor = hoverSprite.color;
            newColor.a = 0.0f;
            hoverSprite.color = newColor;
            SetEnabled(false);
        }
    }
}