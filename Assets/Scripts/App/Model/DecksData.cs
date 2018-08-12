// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Loom.Newtonsoft.Json;
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
            foreach (var card in cards)
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
            foreach (var card in cards)
            {
                if (card.cardName == cardId)
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

        public Deck Clone() {
            Deck deck = new Deck();
            deck.id = id;
            deck.heroId = heroId;
            deck.name = name;
            deck.cards = deck.cards.Select(c => c.Clone()).ToList();
            return deck;
        }
    }

    public class DeckCardData
    {
        public string cardName;
        public int amount;

        public DeckCardData()
        {
        }

        public DeckCardData Clone() {
            DeckCardData deckCardData = new DeckCardData();
            deckCardData.cardName = cardName;
            deckCardData.amount = amount;
            return deckCardData;
        }
    }
}