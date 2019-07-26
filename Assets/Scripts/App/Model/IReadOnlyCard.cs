using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public interface IReadOnlyCard
    {
        [JsonProperty(Order = 10)]
        CardKey CardKey { get; }

        [JsonProperty(Order = 11)]
        Enumerators.CardSet Set { get; }

        [JsonProperty(Order = 40)]
        string Name { get; }

        [JsonProperty(Order = 130)]
        int Cost { get; }

        [JsonProperty(Order = 50)]
        string Description { get; }

        [JsonProperty(Order = 60)]
        string FlavorText { get; }

        [JsonProperty(Order = 70)]
        string Picture { get; }

        [JsonProperty(Order = 110)]
        int Damage { get; }

        [JsonProperty(Order = 120)]
        int Defense { get; }

        [JsonProperty(Order = 30)]
        Enumerators.Faction Faction { get; }

        [JsonProperty(Order = 100)]
        string Frame { get; }

        [JsonProperty(Order = 20)]
        Enumerators.CardKind Kind { get; }

        [JsonProperty(Order = 80)]
        Enumerators.CardRank Rank { get; }

        [JsonProperty(Order = 90)]
        Enumerators.CardType Type { get; }

        [JsonProperty(Order = 150)]
        IReadOnlyList<AbilityData> Abilities { get; }

        [JsonProperty(Order = 140)]
        CardPictureTransforms PictureTransforms { get; }

        [JsonProperty(Order = 160)]
        Enumerators.UniqueAnimation UniqueAnimation { get; }

        [JsonProperty(Order = 170)]
        bool Hidden { get; }

        [JsonProperty(Order = 500)]
        CardOverrideData Overrides { get; }
    }
}
