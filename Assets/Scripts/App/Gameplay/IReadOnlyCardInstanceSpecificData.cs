using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground {
    public interface IReadOnlyCardInstanceSpecificData {
        int Attack { get; }

        int Defense { get; }

        Enumerators.Faction CardSetType { get; }

        Enumerators.CardType CardType { get; }

        int Cost { get; }
    }
}
