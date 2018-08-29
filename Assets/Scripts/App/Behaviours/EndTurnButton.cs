// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using UnityEngine;
using UnityEngine.Assertions;

using LoomNetwork.CZB;
using TMPro;
using LoomNetwork.CZB.Common;

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
        buttonText.text = enabled ? "END\nTURN" : "\nWAIT";
        thisRenderer.sprite = enabled ? defaultSprite : pressedSprite;
    }

    private void OnMouseEnter()
    {
        hovering = true;
    }

    private void OnMouseExit()
    {
        if (!active) return;
        
        hovering = false;
        thisRenderer.sprite = defaultSprite;
        buttonText.transform.localPosition = textDefaultPosition;
    }

    private void OnMouseDown()
    {
        if (!active) return;

        thisRenderer.sprite = pressedSprite;
        buttonText.transform.localPosition = textPressedPosition;
        GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.END_TURN, 128, Constants.END_TURN_CLICK_SOUND_VOLUME, dropOldBackgroundMusic: false);
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
            SetEnabled(false);
        }

        //thisRenderer.sprite = defaultSprite;
        buttonText.transform.localPosition = textDefaultPosition;
    }
}