// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

using DG.Tweening;
using GrandDevs.CZB;
using TMPro;
using GrandDevs.CZB.Common;

public class EndTurnButton : MonoBehaviour
{
    [SerializeField]
    private Sprite defaultSprite, pressedSprite;

    [SerializeField]
    private TextMeshPro buttonText;

    [SerializeField]
    private Vector3 textPressedPosition = new Vector3(0, -0.12f, 0),
                    textDefaultPosition = new Vector3(0, -0.00f, 0);

    private bool hovering = false;
    private bool active;
    private SpriteRenderer thisRenderer;

    private void Awake()
    {
        Assert.IsNotNull(defaultSprite);
        Assert.IsNotNull(pressedSprite);
        thisRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetEnabled(bool enabled)
    {
        active = enabled;
        buttonText.text = enabled ? "END\nTURN" : "WAIT";
    }

    private void OnMouseEnter()
    {
        hovering = true;
        //if (active)
        //{
        //    shineSprite.DOKill();
        //    shineSprite.DOFade(1.0f, 0.4f);
        //    hoverSprite.DOKill();
        //    hoverSprite.DOFade(1.0f, 0.4f);
        //}
    }

    private void OnMouseExit()
    {
        hovering = false;
        thisRenderer.sprite = defaultSprite;
        buttonText.transform.localPosition = textDefaultPosition;
        //if (active)
        //{
        //    shineSprite.DOKill();
        //    shineSprite.DOFade(0.0f, 0.2f);
        //    hoverSprite.DOKill();
        //    hoverSprite.DOFade(0.0f, 0.2f);
        //}
    }

    private void OnMouseDown()
    {
        if (!active) return;

        thisRenderer.sprite = pressedSprite;
        buttonText.transform.localPosition = textPressedPosition;
        GameClient.Get<ISoundManager>().PlaySound(GrandDevs.CZB.Common.Enumerators.SoundType.END_TURN, 128, Constants.END_TURN_CLICK_SOUND_VOLUME, dropOldBackgroundMusic: false);
    }

    // was OnMouseDown
    private void OnMouseUp()
    {
        if (GameClient.Get<ITutorialManager>().IsTutorial && (GameClient.Get<ITutorialManager>().CurrentStep != 10 && 
                                                              GameClient.Get<ITutorialManager>().CurrentStep != 16 &&
                                                              GameClient.Get<ITutorialManager>().CurrentStep != 21))
            return;
        if (active && hovering)
        {
            GameClient.Get<IGameplayManager>().GetController<BattlegroundController>().StopTurn();
            //shineSprite.DOKill();
            //hoverSprite.DOKill();
            //var newColor = shineSprite.color;
            //newColor.a = 0.0f;
            //shineSprite.color = newColor;
            //newColor = hoverSprite.color;
            //newColor.a = 0.0f;
            //hoverSprite.color = newColor;
            SetEnabled(false);
        }

        thisRenderer.sprite = defaultSprite;
        buttonText.transform.localPosition = textDefaultPosition;
    }
}