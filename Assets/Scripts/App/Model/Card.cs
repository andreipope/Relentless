using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class Card : IReadOnlyCard
    {
        [JsonProperty]
        public long MouldId { get; }

        [JsonProperty]
        public string Name { get; }

        [JsonProperty]
        public int Cost { get; }

        [JsonProperty]
        public string Description { get; }

        [JsonProperty]
        public string FlavorText { get; }

        [JsonProperty]
        public string Picture { get; }

        [JsonProperty]
        public int Damage { get;  }

        [JsonProperty]
        public int Defense { get; }

        [JsonProperty]
        public Enumerators.Faction Faction { get; }

        [JsonProperty]
        public string Frame { get; }

        [JsonProperty]
        public Enumerators.CardKind Kind { get; }

        [JsonProperty]
        public Enumerators.CardRank Rank { get; }

        [JsonProperty]
        public Enumerators.CardType Type { get; }

        [JsonProperty]
        public IReadOnlyList<AbilityData> Abilities { get; }

        [JsonProperty]
        public PictureTransform PictureTransform { get; }

        [JsonProperty]
        public Enumerators.UniqueAnimation UniqueAnimation { get; }

        [JsonProperty]
        public bool Hidden { get; }

        [JsonConstructor]
        public Card(
            long mouldId,
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
            List<AbilityData> abilities,
            PictureTransform pictureTransform,
            Enumerators.UniqueAnimation uniqueAnimation,
            bool hidden
            )
        {
            MouldId = mouldId;
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
            MouldId = sourceCard.MouldId;
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
            return $"({nameof(Name)}: {Name}, {nameof(MouldId)}: {MouldId}, {nameof(Faction)}: {Faction})";
        }
    }

    public class PictureTransform
    {
        [JsonProperty]
        public FloatVector3 Position { get; protected set; } = FloatVector3.Zero;

        [JsonProperty]
        public FloatVector3 Scale { get; protected set; } = new FloatVector3(0.38f);

        public PictureTransform()
        {
        }

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
