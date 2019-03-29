using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class Card : ICard
    {
        private readonly CardAbilities _abilities;

        [JsonProperty]
        public long MouldId { get; set; }

        [JsonProperty]
        public string Name { get; protected set; }

        [JsonProperty]
        public int Cost { get; set; }

        [JsonProperty]
        public string Description { get; protected set; }

        [JsonProperty]
        public string FlavorText { get; protected set; }

        [JsonProperty]
        public string Picture { get; protected set; }

        [JsonProperty]
        public int Damage { get; protected set; }

        [JsonProperty]
        public int Defense { get; protected set; }

        [JsonProperty]
        public Enumerators.Faction Faction { get; set; }

        [JsonProperty]
        public string Frame { get; protected set; }

        [JsonProperty]
        public Enumerators.CardKind Kind { get; protected set; }

        [JsonProperty]
        public Enumerators.CardRank Rank { get; protected set; }

        [JsonProperty]
        public Enumerators.CardType Type { get; protected set; }

        [JsonProperty]
        public CardAbilities Abilities => _abilities;

        [JsonProperty]
        public PictureTransform PictureTransform { get; protected set; }

        [JsonProperty]
        public Enumerators.UniqueAnimation UniqueAnimation { get; protected set; }

        [JsonProperty]
        public bool Hidden { get; protected set; }

        CardAbilities IReadOnlyCard.Abilities => _abilities;

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
            CardAbilities abilities,
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
            _abilities = abilities ?? new CardAbilities(new List<GenericParameter>(), new List<CardAbilityData>());
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
            _abilities = sourceCard.Abilities;
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
