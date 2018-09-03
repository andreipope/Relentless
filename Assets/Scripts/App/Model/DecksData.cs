using System.Collections.Generic;
using System.Linq;

namespace LoomNetwork.CZB.Data
{
    public class DecksData
    {
        public List<Deck> Decks;

        public DecksData()
        {
            Decks = new List<Deck>();
        }
    }

    public class Deck
    {
        public long Id;

        public int HeroId;

        public string Name;

        public List<DeckCardData> Cards;

        public Deck()
        {
            Cards = new List<DeckCardData>();
        }

        public void AddCard(string cardId)
        {
            bool wasAdded = false;
            foreach (DeckCardData card in Cards)
            {
                if (card.CardName == cardId)
                {
                    card.Amount++;
                    wasAdded = true;
                }
            }

            if (!wasAdded)
            {
                DeckCardData cardData = new DeckCardData();
                cardData.CardName = cardId;
                cardData.Amount = 1;
                Cards.Add(cardData);
            }
        }

        public void RemoveCard(string cardId)
        {
            foreach (DeckCardData card in Cards)
            {
                if (card.CardName == cardId)
                {
                    card.Amount--;
                    if (card.Amount < 1)
                    {
                        Cards.Remove(card);
                        break;
                    }
                }
            }
        }

        public int GetNumCards()
        {
            int amount = 0;
            foreach (DeckCardData card in Cards)
            {
                amount += card.Amount;
            }

            return amount;
        }

        public Deck Clone()
        {
            Deck deck = new Deck
            {
                Id = Id,
                HeroId = HeroId,
                Name = Name,
                Cards = Cards.Select(c => c.Clone()).ToList()
            };
            return deck;
        }
    }

    public class DeckCardData
    {
        public string CardName;

        public int Amount;

        public DeckCardData Clone()
        {
            DeckCardData deckCardData = new DeckCardData
            {
                CardName = CardName,
                Amount = Amount
            };
            return deckCardData;
        }
    }
}
