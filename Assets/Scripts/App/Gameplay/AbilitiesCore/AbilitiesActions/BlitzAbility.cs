using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loom.ZombieBattleground
{
    internal class BlitzAbility : CardAbility
    {
        public override void DoAction()
        {
            UnitModelOwner.AddBuff(Common.Enumerators.BuffType.BLITZ);
        }
    }
}
