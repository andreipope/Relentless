using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class ShieldAbility : CardAbility
    {
        public override void DoAction()
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        boardUnitModel.AddBuffShield();
                        break;
                }
            }
        }
    }
}
