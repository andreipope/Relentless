using Loom.ZombieBattleground.Common;
using System.Collections.Generic;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground {
    public interface IReadOnlyCardInstanceSpecificData {
        int Attack { get; }

        int Defense { get; }

        Enumerators.SetType CardSetType { get; }

        Enumerators.CardType CardType { get; }

        int Cost { get; }

        IList<AbilityData> Abilities { get; set; }
    }
}
