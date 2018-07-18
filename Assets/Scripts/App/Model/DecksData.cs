// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB.Data
{
    public class DecksData {
        public List<Deck> decks;

        public DecksData()
        {
           decks = new List<Deck>();
        }
    }

    public class Deck
    {
        public int heroId;
        public string name;
        public List<DeckCardData> cards;

        public Deck()
        {
            cards = new List<DeckCardData>();
        }

        public void AddCard(int cardId)
        {
            bool wasAdded = false;
            foreach (var card in cards)
            {
                if (card.cardId == cardId)
                {
                    card.amount++;
                    wasAdded = true;
                }
            }
            if (!wasAdded)
            {
                DeckCardData cardData = new DeckCardData();
                cardData.cardId = cardId;
                cardData.amount = 1;
                cards.Add(cardData);
            }
        }

        public void RemoveCard(int cardId)
        {
            foreach (var card in cards)
            {
                if (card.cardId == cardId)
                {
                    card.amount--;
                    if(card.amount < 1)
                    {
                        cards.Remove(card);
                        break;
                    }
                }
            }
        }

        public int GetNumCards()
        {
            int amount = 0;
            foreach (var card in cards)
                amount += card.amount;
            return amount;
        }
    }

    public class DeckCardData
    {
        public int cardId;
        public int amount;

        public DeckCardData()
        {
        }
    }
}