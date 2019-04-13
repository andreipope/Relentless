using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class ShieldAbility : AbilityBase
    {
        public ShieldAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger == Enumerators.AbilityTrigger.ENTRY && AbilityActivity == Enumerators.AbilityActivity.PASSIVE)
            {
                TakeGuardToTarget(AbilityUnitOwner);
                InvokeUseAbilityEvent();
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                TakeGuardToTarget(TargetUnit);
                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });
            }
        }

        private void TakeGuardToTarget(IBoardObject boardObject)
        {
            if (boardObject is CardModel unit)
            {
                unit.AddBuffShield();

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = AbilityUnitOwner,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.Guard,
                            Target = unit,
                        }
                    }
                });
            }
        }
    }
}
