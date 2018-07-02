// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using Newtonsoft.Json;
using LoomNetwork.Internal;

namespace LoomNetwork.CZB.Data
{
    public class OpponentDecksData
    {
        public List<OpponentDeck> decks;

        public OpponentDecksData()
        {

        }

        public void ParseData()
        {
            foreach (var deck in decks)
            {
                deck.ParseData();
            }
        }
    }

    public class OpponentDeck
    {
        public int heroId;
        public string type;
        public string actions;
        public List<DeckCardData> cards;

        [JsonIgnore]
        public List<Enumerators.AIActionType> opponentActions;

        public OpponentDeck()
        {
        }

        public void ParseData()
        {
            opponentActions = Utilites.CastList<Enumerators.AIActionType>(actions);
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
            foreach (var card in cards)
                amount += card.amount;
            return amount;
        }
    }
}