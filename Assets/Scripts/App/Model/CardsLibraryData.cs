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

                    if(card.kind != null)
                        card.cardKind = CastStringTuEnum<Enumerators.CardKind>(card.kind);
                    if (card.rarity != null)
                        card.cardRarity = CastStringTuEnum<Enumerators.CardRarity>(card.rarity);
                    if (card.type != null)
                        card.cardType = CastStringTuEnum<Enumerators.CardType>(card.type);

                    foreach (var ability in card.abilities)
                        ability.ParseData();
                    _allCards.Add(card);
                    id++;
                }
            }
        }

        private T CastStringTuEnum<T>(string data)
        {
            //UnityEngine.Debug.Log(typeof(T) + " | " + data);
            return (T)Enum.Parse(typeof(T), data.ToUpper());
        }
    }

    public class CardSet
    {
        public string name;
        public List<Card> cards;
    }

    
}