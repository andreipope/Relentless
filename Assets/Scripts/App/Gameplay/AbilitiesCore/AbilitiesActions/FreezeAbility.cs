using Loom.ZombieBattleground.Common;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class FreezeAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        boardUnitModel.Stun(Enumerators.StunType.FREEZE, 1);
                        break;
                    case Player player:
                        player.Stun(Enumerators.StunType.FREEZE, 1);
                        break;
                }
            }
        }
    }
}
