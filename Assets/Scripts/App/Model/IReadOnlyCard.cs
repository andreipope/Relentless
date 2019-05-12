using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public interface IReadOnlyCard
    {
        MouldId MouldId { get; }

        string Name { get; }

        int Cost { get; }

        string Description { get; }

        string FlavorText { get; }

        string Picture { get; }

        int Damage { get; }

        int Defense { get; }

        Enumerators.Faction Faction { get; }

        string Frame { get; }

        Enumerators.CardKind Kind { get; }

        Enumerators.CardRank Rank { get; }

        Enumerators.CardType Type { get; }

        // FIXME: should be readonly 
        IReadOnlyList<AbilityData> Abilities { get; }

        PictureTransform PictureTransform { get; }

        Enumerators.UniqueAnimation UniqueAnimation { get; }

        bool Hidden { get; }
    }
}
