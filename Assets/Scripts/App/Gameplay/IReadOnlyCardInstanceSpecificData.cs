using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground {
    public interface IReadOnlyCardInstanceSpecificData {
        int Attack { get; }

        int Defense { get; }

        Enumerators.SetType CardSetType { get; }

        Enumerators.CardType CardType { get; }

        int Cost { get; }
    }
}
