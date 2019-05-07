using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class DecksData
    {
        public List<Deck> Decks { get; }

        public DecksData(List<Deck> decks)
        {
            Decks = decks;
        }
    }

    public class AIDeck
    {
        public Deck Deck { get; }

        public Enumerators.AIType Type { get; }

        public AIDeck(Deck deck, Enumerators.AIType type)
        {
            Deck = deck;
            Type = type;
        }
    }

    public class Deck
    {
        public long Id { get; set; }

        [JsonProperty("HeroId")]
        public OverlordId OverlordId { get; set; }

        public string Name { get; set; }

        public List<DeckCardData> Cards;

        public Enumerators.Skill PrimarySkill { get; set; }

        public Enumerators.Skill SecondarySkill { get; set; }

        public Deck(
            long id,
            OverlordId overlordId,
            string name,
            List<DeckCardData> cards,

            Enumerators.Skill primarySkill,
            Enumerators.Skill secondarySkill
            )
        {
            Id = id;
            OverlordId = overlordId;
            Name = name;
            Cards = cards ?? new List<DeckCardData>();
            PrimarySkill = primarySkill;
            SecondarySkill = secondarySkill;
        }

        public void AddCard(MouldId mouldId)
        {
            bool wasAdded = false;
            foreach (DeckCardData card in Cards)
            {
                if (card.MouldId == mouldId)
                {
                    card.Amount++;
                    wasAdded = true;
                }
            }

            if (!wasAdded)
            {
                DeckCardData cardData = new DeckCardData(mouldId, 1);
                Cards.Add(cardData);
            }
        }

        public void RemoveCard(MouldId mouldId)
        {
            foreach (DeckCardData card in Cards)
            {
                if (card.MouldId == mouldId)
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
            (
                Id,
                OverlordId,
                Name,
                Cards.Select(c => c.Clone()).ToList(),
                PrimarySkill,
                SecondarySkill
            );
            return deck;
        }
    }

    public class DeckCardData
    {
        public MouldId MouldId { get; set; }

        public int Amount { get; set; }

        public DeckCardData(MouldId mouldId, int amount)
        {
            MouldId = mouldId;
            Amount = amount;
        }

        public DeckCardData Clone()
        {
            return new DeckCardData(MouldId, Amount);
        }
    }
}
