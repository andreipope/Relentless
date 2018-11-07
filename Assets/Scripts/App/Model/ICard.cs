using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public interface ICard
    {
        long MouldId { get; set; }

        string Name { get; set; }

        int Cost { get; set; }

        string Description { get; set; }

        string FlavorText { get; set; }

        string Picture { get; set; }

        int Damage { get; set; }

        int Health { get; set; }

        Enumerators.SetType CardSetType { get; set; }

        string Frame { get; set; }

        Enumerators.CardKind CardKind { get; set; }

        Enumerators.CardRank CardRank { get; set; }

        Enumerators.CardType CardType { get; set; }

        List<AbilityData> Abilities { get; set; }

        CardViewInfo CardViewInfo { get; set; }
    }
}
