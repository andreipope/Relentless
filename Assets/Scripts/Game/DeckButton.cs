// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;
using TMPro;

using CCGKit;

public class DeckButton : MonoBehaviour
{
    [SerializeField]
    protected Image activeBackground;

    [SerializeField]
    protected TextMeshProUGUI nameText;

    [SerializeField]
    protected TextMeshProUGUI numCardsText;

    [SerializeField]
    protected TextMeshProUGUI numSpellsText;

    [SerializeField]
    protected TextMeshProUGUI numCreaturesText;

    [HideInInspector]
    public DeckBuilderScene scene;

    public Deck deck { get; private set; }

    public void OnButtonPressed()
    {
        scene.SetActiveDeck(this);
    }

    public void OnDeleteButtonPressed()
    {
        scene.RemoveDeck(deck);
        Destroy(gameObject);
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            activeBackground.DOFade(1.0f, 0.5f);
        }
        else
        {
            activeBackground.DOFade(0.0f, 0.2f);
        }
    }

    public void SetDeck(Deck deck)
    {
        this.deck = deck;
        nameText.text = deck.name;
        UpdateDeckInfo();
    }

    public void SetDeckName(string name)
    {
        deck.name = name;
        nameText.text = name;
    }

    public void UpdateDeckInfo()
    {
        numCardsText.text = deck.GetNumCards().ToString() + " cards";
        numCreaturesText.text = deck.GetNumCards(GameManager.Instance.config, 0).ToString();
        numSpellsText.text = deck.GetNumCards(GameManager.Instance.config, 1).ToString();
        scene.UpdateNumCardsText();
    }
}
