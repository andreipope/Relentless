using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class ChangeStatAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Defense))
                        {
                            int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                                       Common.Enumerators.AbilityParameter.Defense);

                            boardUnitModel.BuffedDefense += value;
                            boardUnitModel.CurrentDefense += value;
                        }
                        if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Attack))
                        {
                            int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                                          Common.Enumerators.AbilityParameter.Attack);

                            boardUnitModel.BuffedDamage += value;
                            boardUnitModel.CurrentDamage += value;
                        }
                        break;
                }
            }
        }
    }
}
