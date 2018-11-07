using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class Card : ICard, IReadOnlyCard
    {
        [JsonProperty("Id")]
        public long MouldId { get; set; }

        public string Name { get; set; }

        public int Cost { get; set; }

        public string Description { get; set; }

        public string FlavorText { get; set; }

        public string Picture { get; set; }

        public int Damage { get; set; }

        public int Health { get; set; }

        public Enumerators.SetType CardSetType { get; set; }

        public string Frame { get; set; }

        [JsonProperty("Kind")]
        public Enumerators.CardKind CardKind { get; set; }

        [JsonProperty("Rank")]
        public Enumerators.CardRank CardRank { get; set; }

        [JsonProperty("Type")]
        public Enumerators.CardType CardType { get; set; }

        public List<AbilityData> Abilities { get; set; }

        public CardViewInfo CardViewInfo { get; set; }

        public Card(
            long mouldId,
            string name,
            int cost,
            string description,
            string flavorText,
            string picture,
            int damage,
            int health,
            Enumerators.SetType cardSetType,
            string frame,
            Enumerators.CardKind cardKind,
            Enumerators.CardRank cardRank,
            Enumerators.CardType cardType,
            List<AbilityData> abilities,
            CardViewInfo cardViewInfo)
        {
            MouldId = mouldId;
            Name = name;
            Cost = cost;
            Description = description;
            FlavorText = flavorText;
            Picture = picture;
            Damage = damage;
            Health = health;
            CardSetType = cardSetType;
            Frame = frame;
            CardKind = cardKind;
            CardRank = cardRank;
            CardType = cardType;
            Abilities = abilities;
            CardViewInfo = cardViewInfo;
        }

        public Card(IReadOnlyCard sourceCard)
        {
            MouldId = sourceCard.MouldId;
            Name = sourceCard.Name;
            Cost = sourceCard.Cost;
            Description = sourceCard.Description;
            FlavorText = sourceCard.FlavorText;
            Picture = sourceCard.Picture;
            Damage = sourceCard.Damage;
            Health = sourceCard.Health;
            CardSetType = sourceCard.CardSetType;
            Frame = sourceCard.Frame;
            CardKind = sourceCard.CardKind;
            CardRank = sourceCard.CardRank;
            CardType = sourceCard.CardType;
            Abilities =
                sourceCard.Abilities
                    .Select(a => new AbilityData(a))
                    .ToList();
            CardViewInfo = new CardViewInfo(sourceCard.CardViewInfo);
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

        public CardViewInfo()
        {
        }

        public CardViewInfo(CardViewInfo source)
        {
            Position = source.Position;
            Scale = source.Scale;
        }
    }
}
