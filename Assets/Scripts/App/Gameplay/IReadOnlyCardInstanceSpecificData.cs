using Loom.ZombieBattleground.Common;
using System.Collections.Generic;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground {
    public interface IReadOnlyCardInstanceSpecificData {
        int Damage { get; }

        int Defense { get; }

        Enumerators.Faction Faction { get; }

        Enumerators.CardType CardType { get; }

        int Cost { get; }

        CardAbilities Abilities { get; set; }
    }
}
