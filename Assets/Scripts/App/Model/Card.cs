using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class Card : ICard
    {
        [JsonProperty("id")]
        public long MouldId { get; set; }

        [JsonProperty("name")]
        public string Name { get; protected set; }

        [JsonProperty("cost")]
        public int Cost { get; set; }

        [JsonProperty("description")]
        public string Description { get; protected set; }

        [JsonProperty("flavor_text")]
        public string FlavorText { get; protected set; }

        [JsonProperty("picture")]
        public string Picture { get; protected set; }

        [JsonProperty("damage")]
        public int Damage { get; protected set; }

        [JsonProperty("health")]
        public int Defense { get; protected set; }

        [JsonProperty("set")]
        public Enumerators.Faction Faction { get; set; }

        [JsonProperty("frame")]
        public string Frame { get; protected set; }

        [JsonProperty("kind")]
        public Enumerators.CardKind CardKind { get; protected set; }

        [JsonProperty("rank")]
        public Enumerators.CardRank CardRank { get; protected set; }

        [JsonProperty("type")]
        public Enumerators.CardType CardType { get; protected set; }

        [JsonProperty("abilities")]
        public List<AbilityData> Abilities { get; private set; }

        [JsonProperty("card_view_info")]
        public PictureTransform PictureTransform { get; protected set; }

        [JsonProperty("unique_animation_type")]
        public Enumerators.UniqueAnimation UniqueAnimation { get; protected set; }

        [JsonProperty("hidden")]
        public bool Hidden { get; protected set; }

        IList<AbilityData> IReadOnlyCard.Abilities => Abilities;

        IList<AbilityData> ICard.Abilities { get; }

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
            Enumerators.CardKind cardKind,
            Enumerators.CardRank cardRank,
            Enumerators.CardType cardType,
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
            CardKind = cardKind;
            CardRank = cardRank;
            CardType = cardType;
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
            CardKind = sourceCard.CardKind;
            CardRank = sourceCard.CardRank;
            CardType = sourceCard.CardType;
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
        [JsonProperty("position")]
        public FloatVector3 Position { get; protected set; } = FloatVector3.Zero;
        [JsonProperty("scale")]
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
