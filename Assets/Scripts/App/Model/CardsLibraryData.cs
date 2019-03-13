using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class CardsLibraryData
    {
        public List<CardSet> Sets { get; private set; }

        public IList<Card> Cards { get; private set; }

        public int CardsInActiveSetsCount { get; private set; }

        public CardsLibraryData(IList<Card> cards)
        {
            Cards = cards ?? throw new ArgumentNullException(nameof(cards));
            InitData();
        }

        public Card GetCardFromName(string name)
        {
            Card card = Cards.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
            if (card == null)
                throw new Exception($"Card '{name}' not found");

            return card;
        }
        
        public Card GetCardFromMouldId(int mouldId)
        {
            Card card =  Cards.FirstOrDefault(x => x.MouldId == mouldId);
            if (card == null)
                throw new Exception($"Card '{mouldId}' not found");

            return card;
        }

        private void InitData()
        {
            Cards = Cards.OrderBy(card => card.Cost).ToList();
            Sets =
                Cards
                    .GroupBy(card => card.Faction)
                    .Select(group => new CardSet(group.Key, group.ToList()))
                    .OrderBy(set => set.Name)
                    .ToList();

            foreach (CardSet set in Sets)
            {
                foreach (Card card in set.Cards)
                {
                    CardsInActiveSetsCount++;
                }
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            InitData();
        }
    }

    public class CardSet
    {
        public Enumerators.Faction Name { get; }
        public List<Card> Cards { get; }

        public CardSet(Enumerators.Faction name, List<Card> cards)
        {
            Name = name;
            Cards = cards;
        }

        public override string ToString()
        {
            return $"({nameof(Name)}: {Name}, {Cards.Count} cards)";
        }
    }

    public class CardList
    {
        [JsonProperty("cards")]
        public List<Card> Cards;
    }
}
