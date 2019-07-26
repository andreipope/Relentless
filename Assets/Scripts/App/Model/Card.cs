using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class Card : IReadOnlyCard
    {
        [JsonProperty(Order = 10)]
        public CardKey CardKey { get; }

        [JsonProperty(Order = 11)]
        public Enumerators.CardSet Set { get; }

        [JsonProperty(Order = 40)]
        public string Name { get; }

        [JsonProperty(Order = 130)]
        public int Cost { get; }

        [JsonProperty(Order = 50)]
        public string Description { get; }

        [JsonProperty(Order = 60)]
        public string FlavorText { get; }

        [JsonProperty(Order = 70)]
        public string Picture { get; }

        [JsonProperty(Order = 110)]
        public int Damage { get;  }

        [JsonProperty(Order = 120)]
        public int Defense { get; }

        [JsonProperty(Order = 30)]
        public Enumerators.Faction Faction { get; }

        [JsonProperty(Order = 100)]
        public string Frame { get; }

        [JsonProperty(Order = 20)]
        public Enumerators.CardKind Kind { get; }

        [JsonProperty(Order = 80)]
        public Enumerators.CardRank Rank { get; }

        [JsonProperty(Order = 90)]
        public Enumerators.CardType Type { get; }

        [JsonProperty(Order = 150)]
        public IReadOnlyList<AbilityData> Abilities { get; }

        [JsonProperty(Order = 140)]
        public PictureTransform PictureTransform { get; }

        [JsonProperty(Order = 160)]
        public Enumerators.UniqueAnimation UniqueAnimation { get; }

        [JsonProperty(Order = 170)]
        public bool Hidden { get; }

        [JsonProperty(Order = 500)]
        public CardOverrideData Overrides { get; }

        [JsonConstructor]
        public Card(
            CardKey cardKey,
            Enumerators.CardSet set,
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
            bool hidden,
            CardOverrideData overrides
            )
        {
            CardKey = cardKey;
            Set = set;
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
            Overrides = overrides;
        }

        public Card(IReadOnlyCard sourceCard)
        {
            CardKey = sourceCard.CardKey;
            Set = sourceCard.Set;
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
            Overrides = Overrides;
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
