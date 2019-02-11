using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class FreezeUnitsAbility : AbilityBase
    {
        public int Value { get; }

        public FreezeUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType == Enumerators.AbilityCallType.ENTRY && AbilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
            {
                AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

                InvokeActionTriggered(GetOpponentOverlord());
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                TargetUnit.Stun(Enumerators.StunType.FREEZE, Value);
                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                {
                    TargetUnit
                }, AbilityData.AbilityType, Enumerators.AffectObjectType.Character);

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = GetCaller(),
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.Freeze,
                            Target = TargetUnit,
                        }
                    }
                });
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (BoardUnitView unit in GetOpponentOverlord().BoardCards)
            {
                unit.Model.Stun(Enumerators.StunType.FREEZE, Value);

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.Freeze,
                    Target = unit.Model,
                });
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Character);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });

            AbilityProcessingAction?.ForceActionDone();
        }
    }
}
