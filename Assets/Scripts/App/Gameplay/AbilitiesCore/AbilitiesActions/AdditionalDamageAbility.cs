using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AdditionalDamageAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            if (AbilitiesController.HasParameter(genericParameters, Common.Enumerators.AbilityParameter.Damage))
            {
                if (AbilitiesController.GetParameterValue<int>(genericParameters,
                        Common.Enumerators.AbilityParameter.Damage) == 0)
                    return;
            }

            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Damage))
                        {
                            int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                                          Common.Enumerators.AbilityParameter.Damage);

                            boardUnitModel.CurrentDamage -= value;
                        }
                        break;
                }
            }
        }
    }
}
