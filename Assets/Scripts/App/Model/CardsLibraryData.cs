using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class CardsLibraryData
    {
        public List<CardSet> Sets;

        private List<Card> _allCards;

        public int CardsInActiveSetsCount;

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

        public Card GetCardFromName(string name)
        {
            return Cards.Find(x => x.Name.ToLowerInvariant() == name.ToLower());
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
                        // TODO: improve this shit!
                        card.CardSetType = set.Name;

                        _allCards.Add(card);

                        // FIXME: why are we setting mould IDs manually?
                        if (card.CardSetType != Enumerators.SetType.OTHERS)
                        {
                            card.MouldId = id;
                            CardsInActiveSetsCount++;
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
        public Enumerators.SetType Name;

        public List<Card> Cards;

        public override string ToString()
        {
            return $"({nameof(Name)}: {Name}, {Cards.Count} cards)";
        }
    }
}
