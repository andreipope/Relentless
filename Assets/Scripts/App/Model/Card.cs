using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class Card : IReadOnlyCard
    {
        public CardKey CardKey { get; }

        public Enumerators.CardSet Set { get; }

        public string Name { get; }

        public int Cost { get; }

        public string Description { get; }

        public string FlavorText { get; }

        public string Picture { get; }

        public int Damage { get;  }

        public int Defense { get; }

        public Enumerators.Faction Faction { get; }

        public string Frame { get; }

        public Enumerators.CardKind Kind { get; }

        public Enumerators.CardRank Rank { get; }

        public Enumerators.CardType Type { get; }

        public IReadOnlyList<AbilityData> Abilities { get; }

        public CardPictureTransforms PictureTransforms { get; }

        public Enumerators.UniqueAnimation UniqueAnimation { get; }

        public bool Hidden { get; }

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
            CardPictureTransforms pictureTransforms,
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
            PictureTransforms = pictureTransforms;
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
                sourceCard
                    .Abilities?
                    .Select(a => new AbilityData(a))
                    .ToList();
            PictureTransforms = sourceCard.PictureTransforms == null ? null : new CardPictureTransforms(sourceCard.PictureTransforms);
            UniqueAnimation = sourceCard.UniqueAnimation;
            Hidden = sourceCard.Hidden;
            Overrides = Overrides;
        }

        public override string ToString()
        {
            return $"({nameof(Name)}: {Name}, {nameof(CardKey)}: {CardKey}, {nameof(Faction)}: {Faction})";
        }
    }

    public class CardPictureTransforms
    {
        [JsonProperty]
        public PictureTransform Battleground { get; } = new PictureTransform();

        [JsonProperty("deckUI")]
        public PictureTransform DeckUI { get; } = new PictureTransform(FloatVector2.Zero, 1f);

        [JsonProperty]
        public PictureTransform PastAction { get; } = new PictureTransform(FloatVector2.Zero, 1f);

        public CardPictureTransforms()
        {
        }

        [JsonConstructor]
        public CardPictureTransforms(PictureTransform battleground, PictureTransform deckUI, PictureTransform pastAction)
        {
            Battleground = battleground;
            DeckUI = deckUI ?? DeckUI;
            PastAction = pastAction ?? PastAction;
        }

        public CardPictureTransforms(CardPictureTransforms source)
        {
            Battleground = source.Battleground;
            DeckUI = source.DeckUI;
            PastAction = source.PastAction;
        }
    }

    public class PictureTransform
    {
        [JsonProperty]
        public FloatVector2 Position { get; } = FloatVector2.Zero;

        [JsonProperty]
        public float Scale { get; } = 0.38f;

        public PictureTransform()
        {
        }

        [JsonConstructor]
        public PictureTransform(FloatVector2 position, float scale)
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
