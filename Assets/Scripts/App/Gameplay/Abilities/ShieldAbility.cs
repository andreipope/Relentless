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

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Character);

            if (AbilityCallType == Enumerators.AbilityCallType.ENTRY && AbilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
            {
                TakeGuardToTarget(AbilityUnitOwner);
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                TakeGuardToTarget(TargetUnit);
            }
        }

        private void TakeGuardToTarget(BoardObject boardObject)
        {
            if (boardObject is BoardUnitModel unit)
            {
                unit.AddBuffShield();

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = GetCaller(),
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
