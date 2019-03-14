using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground {
    public interface IReadOnlyCardInstanceSpecificData {
        int Damage { get; }

        int Defense { get; }

        Enumerators.Faction Faction { get; }

        Enumerators.CardType CardType { get; }

        int Cost { get; }
    }
}
