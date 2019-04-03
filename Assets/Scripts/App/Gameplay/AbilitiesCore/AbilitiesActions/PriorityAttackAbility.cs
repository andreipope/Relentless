using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class PriorityAttackAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        boardUnitModel.AttackAsFirst = true;
                        break;
                }
            }
        }
    }
}
