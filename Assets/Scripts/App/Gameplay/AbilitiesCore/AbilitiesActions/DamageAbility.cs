using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class DamageAbility : CardAbility
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
                if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Damage))
                {
                    int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                               Common.Enumerators.AbilityParameter.Damage);

                    switch (target)
                    {
                        case BoardUnitModel boardUnitModel:
                            BattleController.AttackUnitByAbility(UnitModelOwner, boardUnitModel, value);
                            break;
                        case Player player:
                            BattleController.AttackPlayerByAbility(UnitModelOwner, player, value);
                            break;
                    }
                }
            }
        }
    }
}
