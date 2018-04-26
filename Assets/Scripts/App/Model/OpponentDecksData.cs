using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;
using Newtonsoft.Json;
using GrandDevs.Internal;

namespace GrandDevs.CZB.Data
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
        public string name;
        public string actions;
        public List<DeckCardData> cards;

        [JsonIgnore]
        public List<Enumerators.ActionType> opponentActions;

        public OpponentDeck()
        {
        }

        public void ParseData()
        {
            opponentActions = Utilites.CastList<Enumerators.ActionType>(actions);
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