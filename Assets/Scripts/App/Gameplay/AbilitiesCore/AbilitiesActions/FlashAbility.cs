using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    internal class FlashAbility : CardAbility
    {
        private int _numberOfAttacksWas;

        private Common.Enumerators.AttackRestriction _attackInfo;

        public override void DoAction()
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Attack))
                        {
                            int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                                       Common.Enumerators.AbilityParameter.Attack);

                            _numberOfAttacksWas++;

                            if (_numberOfAttacksWas < value)
                            {
                                UnitModelOwner.ForceSetCreaturePlayable();
                            }
                            else
                            {
                                _numberOfAttacksWas = 0;
                            }
                        }
                       
                        break;
                }
            }
        }

        public override void AbilityInitializedAction()
        {
            base.AbilityInitializedAction();

            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.AttackRestriction))
                        {
                            _attackInfo = AbilitiesController.GetParameterValue<Common.Enumerators.AttackRestriction>(GenericParameters,
                                                                       Common.Enumerators.AbilityParameter.AttackRestriction);

                            boardUnitModel.AttackRestriction = _attackInfo;
                        }
                        break;
                }
            }

            PostGameActionReport(Common.Enumerators.ActionType.CardAffectingCard, Targets.Select(target =>
                new PastActionsPopup.TargetEffectParam
                {
                    ActionEffectType = Common.Enumerators.ActionEffectType.Blitz,
                    Target = target
                }).ToList()
            );
        }
    }
}
