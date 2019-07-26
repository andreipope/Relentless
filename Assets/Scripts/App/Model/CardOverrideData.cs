using System.Collections.Generic;
using System.Linq;

using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class CardOverrideData
    {
        [JsonProperty(Order = 11)]
        [JsonConverter(typeof(NullableWrapperJsonConverter<Enumerators.CardSet>))]
        public Enumerators.CardSet? Set { get; }

        [JsonProperty(Order = 40)]
        [JsonConverter(typeof(NullableReferenceWrapperJsonConverter<string>))]
        public string Name { get; }

        [JsonProperty(Order = 130)]
        [JsonConverter(typeof(NullableWrapperJsonConverter<int>))]
        public int? Cost { get; }

        [JsonProperty(Order = 50)]
        [JsonConverter(typeof(NullableReferenceWrapperJsonConverter<string>))]
        public string Description { get; }

        [JsonProperty(Order = 60)]
        [JsonConverter(typeof(NullableReferenceWrapperJsonConverter<string>))]
        public string FlavorText { get; }

        [JsonProperty(Order = 70)]
        [JsonConverter(typeof(NullableReferenceWrapperJsonConverter<string>))]
        public string Picture { get; }

        [JsonProperty(Order = 110)]
        [JsonConverter(typeof(NullableWrapperJsonConverter<int>))]
        public int? Damage { get;  }

        [JsonProperty(Order = 120)]
        [JsonConverter(typeof(NullableWrapperJsonConverter<int>))]
        public int? Defense { get; }

        [JsonProperty(Order = 30)]
        [JsonConverter(typeof(NullableWrapperJsonConverter<Enumerators.Faction>))]
        public Enumerators.Faction? Faction { get; }

        [JsonProperty(Order = 100)]
        [JsonConverter(typeof(NullableReferenceWrapperJsonConverter<string>))]
        public string Frame { get; }

        [JsonProperty(Order = 20)]
        [JsonConverter(typeof(NullableWrapperJsonConverter<Enumerators.CardKind>))]
        public Enumerators.CardKind? Kind { get; }

        [JsonProperty(Order = 80)]
        [JsonConverter(typeof(NullableWrapperJsonConverter<Enumerators.CardRank>))]
        public Enumerators.CardRank? Rank { get; }

        [JsonProperty(Order = 90)]
        [JsonConverter(typeof(NullableWrapperJsonConverter<Enumerators.CardType>))]
        public Enumerators.CardType? Type { get; }

        [JsonProperty(Order = 150, ItemConverterType = typeof(NullableReferenceWrapperJsonConverter<AbilityData>))]
        public IReadOnlyList<AbilityData> Abilities { get; }

        [JsonProperty(Order = 140)]
        [JsonConverter(typeof(NullableReferenceWrapperJsonConverter<PictureTransform>))]
        public PictureTransform PictureTransform { get; }

        [JsonProperty(Order = 160)]
        [JsonConverter(typeof(NullableWrapperJsonConverter<Enumerators.UniqueAnimation>))]
        public Enumerators.UniqueAnimation? UniqueAnimation { get; }

        [JsonProperty(Order = 170)]
        [JsonConverter(typeof(NullableWrapperJsonConverter<bool>))]
        public bool? Hidden { get; }

        [JsonConstructor]
        public CardOverrideData(
            Enumerators.CardSet? set,
            string name,
            int? cost,
            string description,
            string flavorText,
            string picture,
            int? damage,
            int? defense,
            Enumerators.Faction? faction,
            string frame,
            Enumerators.CardKind? kind,
            Enumerators.CardRank? rank,
            Enumerators.CardType? type,
            IReadOnlyList<AbilityData> abilities,
            PictureTransform pictureTransform,
            Enumerators.UniqueAnimation? uniqueAnimation,
            bool? hidden
            )
        {
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
        }

        public CardOverrideData(IReadOnlyCard sourceCard)
        {
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
        }

        public override string ToString()
        {
            return $"({nameof(Name)}: {Name}, {nameof(Faction)}: {Faction})";
        }
    }
}
