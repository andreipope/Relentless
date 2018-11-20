using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class Card
    {
        public int Id;

        public Enumerators.SetType CardSetType;

        public string Kind;

        public string Name;

        public int Cost;

        public string Description;

        public string FlavorText;

        public string Picture;

        public int Damage;

        public int Health;

        public string Rank;

        public string Type;

        public string Frame;

        public string UniqueAnimation;

        public List<AbilityData> Abilities = new List<AbilityData>();
        public List<AbilityData> InitialAbilities = new List<AbilityData>();

        public CardViewInfo CardViewInfo = new CardViewInfo();

        [JsonIgnore]
        public Enumerators.CardRank CardRank;

        [JsonIgnore]
        public Enumerators.CardType CardType;

        [JsonIgnore]
        public Enumerators.CardKind CardKind;

        [JsonIgnore]
        public Enumerators.UniqueAnimationType UniqueAnimationType = Enumerators.UniqueAnimationType.None;


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
                InitialAbilities = InitialAbilities,
                CardViewInfo = CardViewInfo,
                Frame = Frame,
                UniqueAnimationType = UniqueAnimationType
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
