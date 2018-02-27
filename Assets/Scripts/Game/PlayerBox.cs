// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// This class wraps the information of a player box from the game scene and it is updated by the
/// game user interface in response to the server sending new state updates to the client.
/// </summary>
public class PlayerBox : MonoBehaviour
{
    public Text PlayerNameText;
    public Image AvatarGlow;
    public Text LivesText;
    public Text NumCardsInDeckText;
    public Text NumCardsDeadText;
    public Text NumCardsInHandText;

    private void Awake()
    {
        Assert.IsTrue(PlayerNameText != null);
        Assert.IsTrue(AvatarGlow != null);
        Assert.IsTrue(LivesText != null);
        Assert.IsTrue(NumCardsInDeckText != null);
        Assert.IsTrue(NumCardsDeadText != null);
        Assert.IsTrue(NumCardsInHandText != null);

        SetAvatarGlowEnabled(false);
    }

    public void SetPlayerNameText(string text)
    {
        PlayerNameText.text = text;
    }

    public void SetAvatarGlowEnabled(bool enabled)
    {
        AvatarGlow.gameObject.SetActive(enabled);
    }

    public void SetLivesText(int lives)
    {
        LivesText.text = lives.ToString();
    }

    public void SetNumCardsInDeckText(int numCards)
    {
        NumCardsInDeckText.text = numCards.ToString();
    }

    public void SetNumCardsDeadText(int numCards)
    {
        NumCardsDeadText.text = numCards.ToString();
    }

    public void SetNumCardsInHandText(int numCards)
    {
        NumCardsInHandText.text = numCards.ToString();
    }
}