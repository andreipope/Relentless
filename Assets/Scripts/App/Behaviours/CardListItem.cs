// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

using TMPro;
using GrandDevs.CZB.Data;
using System;
using GrandDevs.CZB;
using System.Collections.Generic;
using UnityEngine.UI;

public class CardListItem : MonoBehaviour
{
    public Action<int> OnDeleteCard;

    public Deck deckButton;
    public Card card;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardCostText;
    public TextMeshProUGUI cardAmountText;

    public int count = 1;

    private Transform panelCardCount;

    private Image logoImage; 

    private uint _maxCount;

    private List<CardInDeckAmountItem> _cardAmount;

    public void Init(Deck deck, Card card, int count, uint maxCount)
    {
        logoImage = transform.Find("Image_Logo").GetComponent<Image>();
        logoImage.sprite = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<Sprite>("Images/CardsDeckEditingIcons/" + card.picture.ToLower());
        deckButton = deck;
        this.card = card;
        this.count = count;
        cardNameText.text = card.name;
        cardAmountText.text = "x" + count.ToString();
        _maxCount = maxCount;

        FillCardAmount();
    }

    public void AddCard()
    {
        UpdateCardsCount(1);
    }

    public void OnDeleteButtonPressed()
    {
        UpdateCardsCount(-1);
        OnDeleteCard?.Invoke(card.id);

        if (count > 0)
        {
            foreach(var deckEntry in deckButton.cards)
            {
                if (deckEntry.cardId == card.id)
                {
                    deckEntry.amount--;
                    break;
                }
            }
        }
		    
        else
        {
			deckButton.RemoveCard(card.id);
			Destroy(gameObject);
        }    
    }

    public void UpdateCardsCount(int amount)
    {
		count += amount;
		cardAmountText.text = "x" + count.ToString();
        bool state = amount > 0 ? true : false;
        int index = amount > 0 ? count - 1 : count;
        _cardAmount[index].ChangeActivate(state);
    }

    private void FillCardAmount()
    {
        panelCardCount = transform.Find("Panel_CardCount");
        _cardAmount = new List<CardInDeckAmountItem>();
        CardInDeckAmountItem item = null;
        for (int i = 0; i < _maxCount; i++)
        {
            item = new CardInDeckAmountItem(panelCardCount);
            if (i >= count)
                item.ChangeActivate(false);
            _cardAmount.Add(item);
        }
    }

    class CardInDeckAmountItem
    {
        private GameObject _selfObject;

        private GameObject _activateObject;

        public CardInDeckAmountItem(Transform parent)
        {
            GameObject prefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/UI/Elements/CardInDeckCountPrefab");
            _selfObject = MonoBehaviour.Instantiate(prefab, parent, false);
            _activateObject = _selfObject.transform.Find("Image_Active").gameObject;
        }

        public void ChangeActivate(bool state)
        {
            _activateObject.SetActive(state);
        }

    }
}
