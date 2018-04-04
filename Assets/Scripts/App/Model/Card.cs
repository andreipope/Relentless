using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB.Data
{
    public class Card {
        public int id;
        public Enumerators.SetType cardSetType;
        public int cardTypeId;
        public string name;
        public int cost;
        public string description;
        public string picture;
        public int damage;
        public int health;
        public Enumerators.CardType type;
        public Enumerators.CardRarity rarity;
        public List<AbilityData> abilities = new List<AbilityData>();

        public Card()
        {
        }
    }
}