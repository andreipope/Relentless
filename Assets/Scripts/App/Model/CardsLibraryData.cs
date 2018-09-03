using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using LoomNetwork.Internal;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class CardsLibraryData
    {
        public List<CardSet> Sets;

        private List<Card> _allCards;

        [JsonIgnore]
        public List<Card> Cards
        {
            get
            {
                if (_allCards == null)
                {
                    FillAllCards();
                }

                return _allCards;
            }
        }

        public Card GetCard(int id)
        {
            return Cards.Find(x => x.Id == id);
        }

        public int GetCardIdFromName(string name)
        {
            return Cards.Find(x => x.Name == name).Id;
        }

        public Card GetCardFromName(string name)
        {
            return Cards.Find(x => x.Name == name);
        }

        public void FillAllCards()
        {
            _allCards = new List<Card>();
            int id = 0;
            if (Sets != null)
            {
                foreach (CardSet set in Sets)
                {
                    foreach (Card card in set.Cards)
                    {
                        card.CardSetType =
                            (Enumerators.SetType) Enum.Parse(typeof(Enumerators.SetType),
                                set.Name.ToUpper()); // todo improve this shit!

                        if (card.Kind != null)
                        {
                            card.CardKind = Utilites.CastStringTuEnum<Enumerators.CardKind>(card.Kind);
                        }

                        if (card.Rank != null)
                        {
                            card.CardRank = Utilites.CastStringTuEnum<Enumerators.CardRank>(card.Rank);
                        }

                        if (card.Type != null)
                        {
                            card.CardType = Utilites.CastStringTuEnum<Enumerators.CardType>(card.Type);
                        }

                        foreach (AbilityData ability in card.Abilities)
                        {
                            ability.ParseData();
                        }

                        _allCards.Add(card);

                        if (card.CardSetType != Enumerators.SetType.OTHERS)
                        {
                            card.Id = id;
                        }

                        id++;
                    }
                }
            }

            SortCardsByRank();
        }

        public void SortCardsByRank()
        {
            _allCards = _allCards?.OrderBy(x => (int) x.CardRank).ToList();

            foreach (CardSet set in Sets)
            {
                set.Cards = set.Cards.OrderBy(x => (int) x.CardRank).ToList();
            }
        }
    }

    public class CardSet
    {
        public string Name;

        public List<Card> Cards;

        public override string ToString()
        {
            return $"({nameof(Name)}: {Name}, {Cards.Count} cards)";
        }
    }
}
