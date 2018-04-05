using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;
using Newtonsoft.Json;
using System;

namespace GrandDevs.CZB.Data
{
    public class CardsLibraryData {
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
                    UnityEngine.Debug.Log(card.abilities.Count);
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