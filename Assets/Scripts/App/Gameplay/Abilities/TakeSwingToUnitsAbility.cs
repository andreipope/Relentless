using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class TakeSwingToUnitsAbility : AbilityBase
    {
        public TakeSwingToUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            InvokeActionTriggered();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.AllAllyUnitsInPlay)
            {
                List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

                foreach (BoardUnitView unit in PlayerCallerOfAbility.BoardCards)
                {
                    unit.Model.AddBuffSwing();

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.Swing,
                        Target = unit.Model,
                    });
                }

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = GetCaller(),
                    TargetEffects = TargetEffects
                });
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Action();

            AbilityProcessingAction?.ForceActionDone();
        }
    }
}
