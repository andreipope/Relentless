// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using Newtonsoft.Json;
using System;
using LoomNetwork.Internal;

namespace LoomNetwork.CZB.Data
{
    public class CardsLibraryData
    {
        public List<CardSet> sets;

        private List<Card> _allCards;

        [JsonIgnore]
        public List<Card> Cards
        {
            get {
                if (_allCards == null)
                    FillAllCards();
                return _allCards; }
        }

        public CardsLibraryData()
        {
        }

        public Card GetCard(int id)
        {
            return Cards.Find(x => x.id == id);
        }

        public int GetCardIdFromName(string name)
        {
            return Cards.Find(x => x.name == name).id;
        }

        public void FillAllCards()
        {
            _allCards = new List<Card>();
            int id = 0;
            foreach (var set in sets)
            {
                foreach (var card in set.cards)
                {
                    card.id = id;
                    card.cardSetType = (Enumerators.SetType)Enum.Parse(typeof(Enumerators.SetType), set.name.ToUpper()); //todo improve this shit!

                    if(card.kind != null)
                        card.cardKind = Utilites.CastStringTuEnum<Enumerators.CardKind>(card.kind);
                    if (card.rank != null)
                        card.cardRank = Utilites.CastStringTuEnum<Enumerators.CardRank>(card.rank);
                    if (card.type != null)
                        card.cardType = Utilites.CastStringTuEnum<Enumerators.CardType>(card.type);

                    foreach (var ability in card.abilities)
                        ability.ParseData();
                    _allCards.Add(card);
                    id++;
                }
            }
        }

        
    }

    public class CardSet
    {
        public string name;
        public List<Card> cards;
    }

    
}