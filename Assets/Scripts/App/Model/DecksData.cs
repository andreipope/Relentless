using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB.Data
{
    public class DecksData {
        public List<Deck> decks;

        public DecksData()
        {

        }
    }

    public class Deck
    {
        public int heroId;
        public string name;
        public List<DeckCardData> cards;

        public Deck()
        {
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