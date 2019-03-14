using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public interface ICard : IReadOnlyCard
    {
        new long MouldId { get; set; }

        new int Cost { get; set; }

        new Enumerators.Faction Faction { get; set; }

        new IList<AbilityData> Abilities { get; }
    }
}
