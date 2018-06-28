// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using DG.Tweening;
using TMPro;
using GrandDevs.CZB;
using GrandDevs.CZB.Common;

/// <summary>
/// This class wraps the game scene's user interface and it is mostly updated when the server
/// sends updated information to the client.
/// </summary>
public class GameUI : MonoBehaviour
{

    public TextMeshPro playerNameText;
    public TextMeshPro opponentNameText;

    public TextMeshPro playerHealthText;
    public TextMeshPro opponentHealthText;

    public TextMeshPro playerDeckText;
    public TextMeshPro opponentDeckText;

    public PlayerManaBar playerManaBar;
    public PlayerManaBar opponentManaBar;

    //public SpriteRenderer endTurnSprite;
    //public TextMeshPro endTurnTitleText;
    //public TextMeshPro endTurnTimeText;
    public EndTurnButton endTurnButton;

    public SpriteRenderer opponentDeckCardView;
    public SpriteRenderer playerDeckCardView;

    private void Awake()
    {
        Assert.IsNotNull(playerNameText);
        Assert.IsNotNull(opponentNameText);
        Assert.IsNotNull(playerHealthText);
        Assert.IsNotNull(opponentHealthText);
        Assert.IsNotNull(playerDeckText);
        Assert.IsNotNull(opponentDeckText);
        Assert.IsNotNull(playerManaBar);
        Assert.IsNotNull(opponentManaBar);
        //Assert.IsNotNull(endTurnSprite);
        //Assert.IsNotNull(endTurnTitleText);
        //Assert.IsNotNull(endTurnTimeText);
        Assert.IsNotNull(endTurnButton);
        Assert.IsNotNull(opponentDeckCardView);
        Assert.IsNotNull(playerDeckCardView);
    }

    public void SetPlayerActive(bool active)
    {
    }

    public void SetOpponentActive(bool active)
    {
    }

    public void SetPlayerName(string text)
    {
        playerNameText.text = text;
    }

    public void SetOpponentName(string text)
    {
        opponentNameText.text = text;
    }

    public void SetPlayerHealth(int health)
    {
        playerHealthText.text = health.ToString();
        if (health > 9)
            playerHealthText.color = Color.white;
        else
            playerHealthText.color = Color.red;
    }

    public void SetOpponentHealth(int health)
    {
        opponentHealthText.text = health.ToString();
        if (health > 9)
            opponentHealthText.color = Color.white;
        else
            opponentHealthText.color = Color.red;
    }

    public void SetPlayerDeckCards(int cards)
    {
        playerDeckText.text = cards.ToString();
        if (cards == 0 && playerDeckCardView.gameObject.activeInHierarchy)
            playerDeckCardView.gameObject.SetActive(false);
    }

    public void SetPlayerHandCards(int cards)
    {
    }

    public void SetPlayerGraveyardCards(int cards)
    {
    }

    public void SetOpponentDeckCards(int cards)
    {
        opponentDeckText.text = cards.ToString();
        if (cards == 0 && opponentDeckCardView.gameObject.activeInHierarchy)
            opponentDeckCardView.gameObject.SetActive(false);
    }

    public void SetOpponentHandCards(int cards)
    {
    }

    public void SetOpponentGraveyardCards(int cards)
    {
    }

    public void SetPlayerMana(int manaRows, int mana)
    {
        playerManaBar.SetMana(manaRows, mana);
    }

    public void SetOpponentMana(int manaRows, int mana)
    {
        opponentManaBar.SetMana(manaRows, mana);
    }

}