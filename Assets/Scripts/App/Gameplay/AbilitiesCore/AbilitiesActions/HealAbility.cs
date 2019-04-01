using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class HealAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            if (AbilitiesController.HasParameter(genericParameters, Common.Enumerators.AbilityParameter.Defense))
            {
                if (AbilitiesController.GetParameterValue<int>(genericParameters,
                        Common.Enumerators.AbilityParameter.Defense) == 0)
                    return;
            }

            foreach (BoardObject target in Targets)
            {
                if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Defense))
                {
                    int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                               Common.Enumerators.AbilityParameter.Defense);

                    switch (target)
                    {
                        case BoardUnitModel boardUnitModel:
                            BattleController.HealUnitByAbility(UnitModelOwner, boardUnitModel, value);
                            break;
                        case Player player:
                            BattleController.HealPlayerByAbility(UnitModelOwner, player, value);
                            break;
                    }
                }
            }
        }
    }
}
