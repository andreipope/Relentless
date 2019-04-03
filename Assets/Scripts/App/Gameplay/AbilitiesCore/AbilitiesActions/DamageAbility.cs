using Loom.ZombieBattleground.Data;
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

            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

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

                    targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Common.Enumerators.ActionEffectType.ShieldDebuff,
                        Target = target,
                        HasValue = true,
                        Value = -value
                    });
                }
            }

            PostGameActionReport(Common.Enumerators.ActionType.CardAffectingCardsWithOverlord, targetEffects);
        }
    }
}
