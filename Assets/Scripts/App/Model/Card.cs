using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Helpers;
using Newtonsoft.Json;

namespace LoomNetwork.CZB.Data
{
    public class Card
    {
        // [JsonIgnore]
        public int Id;

        public Enumerators.SetType CardSetType;

        public string Kind;

        public string Name;

        public int Cost;

        public string Description;

        public string FlavorText; // new

        public string Picture;

        public int Damage;

        public int Health;

        public string Rank;

        public string Type;

        public string Frame;

        public List<AbilityData> Abilities = new List<AbilityData>();

        public CardViewInfo CardViewInfo = new CardViewInfo();

        [JsonIgnore]
        public Enumerators.CardRank CardRank;

        [JsonIgnore]
        public Enumerators.CardType CardType;

        [JsonIgnore]
        public Enumerators.CardKind CardKind;

        public Card Clone()
        {
            Card card = new Card
            {
                Id = Id,
                Kind = Kind,
                Name = Name,
                Cost = Cost,
                Description = Description,
                FlavorText = FlavorText,
                Picture = Picture,
                Damage = Damage,
                Health = Health,
                Rank = Rank,
                Type = Type,
                CardSetType = CardSetType,
                CardKind = CardKind,
                CardRank = CardRank,
                CardType = CardType,
                Abilities = Abilities,
                CardViewInfo = CardViewInfo,
                Frame = Frame
            };

            return card;
        }

        public override string ToString()
        {
            return $"({nameof(Name)}: {Name}, {nameof(CardSetType)}: {CardType})";
        }
    }

    public class CardViewInfo
    {
        public FloatVector3 Position = FloatVector3.Zero;

        public FloatVector3 Scale = new FloatVector3(0.38f);
    }
}
