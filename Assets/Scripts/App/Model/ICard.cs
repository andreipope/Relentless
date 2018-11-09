using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public interface ICard
    {
        long MouldId { get; set; }

        int Cost { get; set; }

        Enumerators.SetType CardSetType { get; set; }
    }
}
