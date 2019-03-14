using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public interface IReadOnlyCard
    {
        long MouldId { get; }

        string Name { get; }

        int Cost { get; }

        string Description { get; }

        string FlavorText { get; }

        string Picture { get; }

        int Damage { get; }

        int Defense { get; }

        Enumerators.Faction Faction { get; }

        string Frame { get; }

        Enumerators.CardKind CardKind { get; }

        Enumerators.CardRank CardRank { get; }

        Enumerators.CardType CardType { get; }

        // FIXME: should be readonly 
        IList<AbilityData> Abilities { get; }

        PictureTransform PictureTransform { get; }

        Enumerators.UniqueAnimation UniqueAnimation { get; }

        bool Hidden { get; }
    }
}
