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

        // TODO: get rid of this
        IList<AbilityData> InitialAbilities { get; }

        // FIXME: should be readonly 
        IList<AbilityData> Abilities { get; }

        CardViewInfo CardViewInfo { get; }

        Enumerators.UniqueAnimationType UniqueAnimationType { get; }

        Enumerators.SetType HiddenCardSetType { get; }
    }
}
