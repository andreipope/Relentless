// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

using TMPro;
using GrandDevs.CZB.Data;
using System;

public class CardListItem : MonoBehaviour
{
    public Action<int, int> DeleteCard;

    public Deck deckButton;
    public Card card;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardCostText;
    public TextMeshProUGUI cardAmountText;

    public int count = 1;

    public void AddCard()
    {
        UpdateCardsCount(1);
    }

    public void OnDeleteButtonPressed()
    {
        UpdateCardsCount(-1);
        DeleteCard?.Invoke(card.id, count);

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
    }
}
