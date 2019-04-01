using Loom.ZombieBattleground.Common;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class FreezeAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            if (AbilitiesController.HasParameter(genericParameters, Common.Enumerators.AbilityParameter.Turns))
            {
                if (AbilitiesController.GetParameterValue<int>(genericParameters,
                        Common.Enumerators.AbilityParameter.Turns) == 0)
                    return;
            }

            foreach (BoardObject target in Targets)
            {
                if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Turns))
                {
                    int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                                  Common.Enumerators.AbilityParameter.Turns);

                    switch (target)
                    {
                        case BoardUnitModel boardUnitModel:
                            boardUnitModel.Stun(Enumerators.StunType.FREEZE, value);
                            break;
                        case Player player:
                            player.Stun(Enumerators.StunType.FREEZE, value);
                            break;
                    }
                }
            }
        }
    }
}
