using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public interface ICard : IReadOnlyCard
    {
        new long MouldId { get; set; }

        new int Cost { get; set; }

        new Enumerators.SetType CardSetType { get; set; }

        void ForceUpdateAbilities(List<AbilityData> abilities);
    }
}
