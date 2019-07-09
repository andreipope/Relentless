using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class Card : IReadOnlyCard
    {
        [JsonProperty(Order = 1)]
        public CardKey CardKey { get; }

        [JsonProperty(Order = 4)]
        public string Name { get; }

        [JsonProperty(Order = 13)]
        public int Cost { get; }

        [JsonProperty(Order = 5)]
        public string Description { get; }

        [JsonProperty(Order = 6)]
        public string FlavorText { get; }

        [JsonProperty(Order = 7)]
        public string Picture { get; }

        [JsonProperty(Order = 11)]
        public int Damage { get;  }

        [JsonProperty(Order = 12)]
        public int Defense { get; }

        [JsonProperty(Order = 3)]
        public Enumerators.Faction Faction { get; }

        [JsonProperty(Order = 10)]
        public string Frame { get; }

        [JsonProperty(Order = 2)]
        public Enumerators.CardKind Kind { get; }

        [JsonProperty(Order = 8)]
        public Enumerators.CardRank Rank { get; }

        [JsonProperty(Order = 9)]
        public Enumerators.CardType Type { get; }

        [JsonProperty(Order = 15)]
        public IReadOnlyList<AbilityData> Abilities { get; }

        [JsonProperty(Order = 14)]
        public PictureTransform PictureTransform { get; }

        [JsonProperty(Order = 16)]
        public Enumerators.UniqueAnimation UniqueAnimation { get; }

        [JsonProperty(Order = 17)]
        public bool Hidden { get; }

        [JsonConstructor]
        public Card(
            CardKey cardKey,
            string name,
            int cost,
            string description,
            string flavorText,
            string picture,
            int damage,
            int defense,
            Enumerators.Faction faction,
            string frame,
            Enumerators.CardKind kind,
            Enumerators.CardRank rank,
            Enumerators.CardType type,
            IReadOnlyList<AbilityData> abilities,
            PictureTransform pictureTransform,
            Enumerators.UniqueAnimation uniqueAnimation,
            bool hidden
            )
        {
            CardKey = cardKey;
            Name = name;
            Cost = cost;
            Description = description;
            FlavorText = flavorText;
            Picture = picture;
            Damage = damage;
            Defense = defense;
            Faction = faction;
            Frame = frame;
            Kind = kind;
            Rank = rank;
            Type = type;
            Abilities = abilities ?? new List<AbilityData>();
            PictureTransform = pictureTransform;
            UniqueAnimation = uniqueAnimation;
            Hidden = hidden;
        }

        public Card(IReadOnlyCard sourceCard)
        {
            CardKey = sourceCard.CardKey;
            Name = sourceCard.Name;
            Cost = sourceCard.Cost;
            Description = sourceCard.Description;
            FlavorText = sourceCard.FlavorText;
            Picture = sourceCard.Picture;
            Damage = sourceCard.Damage;
            Defense = sourceCard.Defense;
            Faction = sourceCard.Faction;
            Frame = sourceCard.Frame;
            Kind = sourceCard.Kind;
            Rank = sourceCard.Rank;
            Type = sourceCard.Type;
            Abilities =
                sourceCard.Abilities
                    .Select(a => new AbilityData(a))
                    .ToList();
            PictureTransform = new PictureTransform(sourceCard.PictureTransform);
            UniqueAnimation = sourceCard.UniqueAnimation;
            Hidden = sourceCard.Hidden;
        }

        public override string ToString()
        {
            return $"({nameof(Name)}: {Name}, {nameof(CardKey)}: {CardKey}, {nameof(Faction)}: {Faction})";
        }
    }

    public class PictureTransform
    {
        [JsonProperty]
        public FloatVector3 Position { get; } = FloatVector3.Zero;

        [JsonProperty]
        public FloatVector3 Scale { get; } = new FloatVector3(0.38f);

        public PictureTransform()
        {
        }

        [JsonConstructor]
        public PictureTransform(FloatVector3 position, FloatVector3 scale)
        {
            Position = position;
            Scale = scale;
        }

        public PictureTransform(PictureTransform source)
        {
            Position = source.Position;
            Scale = source.Scale;
        }
    }
}
