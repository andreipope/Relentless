using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class BlitzAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            UnitModelOwner.ApplyBuff(Common.Enumerators.BuffType.Blitz);
        }
    }
}
