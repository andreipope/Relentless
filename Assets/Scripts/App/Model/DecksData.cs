// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using System.Linq;

namespace LoomNetwork.CZB.Data
{
    public class DecksData
    {
        public List<Deck> decks;

        public DecksData()
        {
            decks = new List<Deck>();
        }
    }

    public class Deck
    {
        public long id;

        public int heroId;

        public string name;

        public List<DeckCardData> cards;

        public Deck()
        {
            cards = new List<DeckCardData>();
        }

        public void AddCard(string cardId)
        {
            bool wasAdded = false;
            foreach (DeckCardData card in cards)
            {
                if (card.cardName == cardId)
                {
                    card.amount++;
                    wasAdded = true;
                }
            }

            if (!wasAdded)
            {
                DeckCardData cardData = new DeckCardData();
                cardData.cardName = cardId;
                cardData.amount = 1;
                cards.Add(cardData);
            }
        }

        public void RemoveCard(string cardId)
        {
            foreach (DeckCardData card in cards)
            {
                if (card.cardName == cardId)
                {
                    card.amount--;
                    if (card.amount < 1)
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
            foreach (DeckCardData card in cards)
            {
                amount += card.amount;
            }

            return amount;
        }

        public Deck Clone()
        {
            Deck deck = new Deck { id = id, heroId = heroId, name = name, cards = cards.Select(c => c.Clone()).ToList() };
            return deck;
        }
    }

    public class DeckCardData
    {
        public string cardName;

        public int amount;

        public DeckCardData Clone()
        {
            DeckCardData deckCardData = new DeckCardData { cardName = cardName, amount = amount };
            return deckCardData;
        }
    }
}
