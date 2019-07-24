using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class DestroyUnitByTypeAbility : AbilityBase
    {
        public DestroyUnitByTypeAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetCardType = ability.TargetCardType;
        }

        public override void Activate()
        {
            base.Activate();
        }

        private void SendAction()
        {
            AbilityProcessingAction?.TriggerActionExternally();
            AbilityProcessingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue: true);

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                }
            );
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            BattlegroundController.DestroyBoardUnit(TargetUnit);

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = AbilityUnitOwner,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                            Target = TargetUnit
                        }
                    }
            });

            AbilityProcessingAction?.TriggerActionExternally();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                SendAction();
                InvokeActionTriggered();
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Action();
        }
    }
}
