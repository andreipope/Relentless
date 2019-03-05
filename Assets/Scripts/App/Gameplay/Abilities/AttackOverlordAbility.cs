using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AttackOverlordAbility : AbilityBase
    {
        public int Value { get; }

        public List<Enumerators.AbilityTarget> TargetTypes { get; }

        public AttackOverlordAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetTypes = ability.AbilityTarget;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            InvokeActionTriggered();
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Player targetObject = null; 

            foreach (Enumerators.AbilityTarget target in TargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTarget.OPPONENT:
                        targetObject = GetOpponentOverlord();
                        break;
                    case Enumerators.AbilityTarget.PLAYER:
                        targetObject = PlayerCallerOfAbility;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }

                BattleController.AttackPlayerByAbility(AbilityUnitOwner, AbilityData, targetObject);
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingOverlord,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = targetObject,
                            HasValue = true,
                            Value = -AbilityData.Value
                        }
                    }
            });

            AbilityProcessingAction?.ForceActionDone();
        }
    }
}
