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

            if (AbilityTrigger == Enumerators.AbilityTrigger.ENTRY && AbilityActivity == Enumerators.AbilityActivity.PASSIVE)
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
                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = AbilityUnitOwner,
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

            foreach (CardModel unit in GetOpponentOverlord().CardsOnBoard)
            {
                unit.Stun(Enumerators.StunType.FREEZE, Value);
                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.Freeze,
                    Target = unit
                });
            }
            
            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = AbilityUnitOwner,
                TargetEffects = TargetEffects
            });

            InvokeUseAbilityEvent();

            AbilityProcessingAction?.TriggerActionManually();
        }

    }
}
