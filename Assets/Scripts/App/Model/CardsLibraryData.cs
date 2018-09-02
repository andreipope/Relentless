// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using System.Collections.Generic;
using System.Linq;
using Loom.Newtonsoft.Json;
using LoomNetwork.CZB.Common;
using LoomNetwork.Internal;
using UnityEngine;

namespace LoomNetwork.CZB.Data
{
    public class CardsLibraryData
    {
        public List<CardSet> sets;

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
            return Cards.Find(x => x.id == id);
        }

        public int GetCardIdFromName(string name)
        {
            return Cards.Find(x => x.name == name).id;
        }

        public Card GetCardFromName(string name)
        {
            return Cards.Find(x => x.name == name);
        }

        public void FillAllCards()
        {
            bool removeCardsWithoutGraphics = false;

            // remove cards without iamges
            List<Card> cardsToRemoveFromSet = new List<Card>();

            _allCards = new List<Card>();
            int id = 0;
            if (sets != null)
            {
                foreach (CardSet set in sets)
                {
                    foreach (Card card in set.cards)
                    {
                        if (removeCardsWithoutGraphics)
                        {
                            // remove cards without iamges
                            if (GameClient.Get<ILoadObjectsManager>().GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", set.name.ToLower(), card.rank.ToLower(), card.picture.ToLower())) == null)
                            {
                                cardsToRemoveFromSet.Add(card);
                                continue;
                            }
                        }

                        card.cardSetType = (Enumerators.SetType)Enum.Parse(typeof(Enumerators.SetType), set.name.ToUpper()); // todo improve this shit!

                        if (card.kind != null)
                        {
                            card.cardKind = Utilites.CastStringTuEnum<Enumerators.CardKind>(card.kind);
                        }

                        if (card.rank != null)
                        {
                            card.cardRank = Utilites.CastStringTuEnum<Enumerators.CardRank>(card.rank);
                        }

                        if (card.type != null)
                        {
                            card.cardType = Utilites.CastStringTuEnum<Enumerators.CardType>(card.type);
                        }

                        foreach (AbilityData ability in card.abilities)
                        {
                            ability.ParseData();
                        }

                        _allCards.Add(card);

                        if (card.cardSetType != Enumerators.SetType.OTHERS)
                        {
                            card.id = id;
                        }

                        id++;
                    }
                }
            }

            if (removeCardsWithoutGraphics)
            {
                // remove cards without iamges
                foreach (Card card in cardsToRemoveFromSet)
                {
                    foreach (CardSet set in sets)
                    {
                        if (set.cards.Contains(card))
                        {
                            set.cards.Remove(card);
                        }
                    }
                }

                cardsToRemoveFromSet.Clear();
            }

            SortCardsByRank();
        }

        public void SortCardsByRank()
        {
            if (_allCards != null)
            {
                _allCards = _allCards.OrderBy(x => (int)x.cardRank).ToList();
            }

            foreach (CardSet set in sets)
            {
                set.cards = set.cards.OrderBy(x => (int)x.cardRank).ToList();
            }
        }
    }

    public class CardSet
    {
        public string name;

        public List<Card> cards;

        public override string ToString()
        {
            return $"({nameof(name)}: {name}, {cards.Count} cards)";
        }
    }
}
