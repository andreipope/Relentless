using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class AgileAbility : CardAbility
    {
        public override void DoAction()
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        boardUnitModel.SetAgileStatus(true);
                        break;
                }
            }
        }
    }
}
