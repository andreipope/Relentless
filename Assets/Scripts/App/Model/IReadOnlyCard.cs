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

        int Health { get; }

        Enumerators.SetType CardSetType { get; }

        string Frame { get; }

        Enumerators.CardKind CardKind { get; }

        Enumerators.CardRank CardRank { get; }

        Enumerators.CardType CardType { get; }

        List<AbilityData> InitialAbilities { get; }

        List<AbilityData> Abilities { get; }

        CardViewInfo CardViewInfo { get; }

        Enumerators.UniqueAnimationType UniqueAnimationType { get; }
    }
}
