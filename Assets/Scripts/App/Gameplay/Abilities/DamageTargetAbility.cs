using System;
using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Object = UnityEngine.Object; 

namespace Loom.ZombieBattleground
{
    public class DamageTargetAbility : AbilityBase
    {
        public int Value { get; }

        private IBoardObject _targetObject;

        public DamageTargetAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            TargetUnitSpecialStatus = ability.TargetUnitSpecialStatus;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            DealDamageToUnitOwner();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.END ||
                GameplayManager.CurrentTurnPlayer != PlayerCallerOfAbility)
                return;

            DealDamageToUnitOwner();
        }

        private void DealDamageToUnitOwner()
        {
            if (AbilityTargets.Contains(Enumerators.Target.ITSELF))
            {
                AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

                BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, AbilityUnitOwner);

                InvokeUseAbilityEvent();

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = AbilityUnitOwner,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = AbilityUnitOwner,
                            HasValue = true,
                            Value = -AbilityData.Value
                        }
                    }
                });

                AbilityProcessingAction?.ForceCompleteAction();
            }
        }

        public override void Action(object info = null)
        {
            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue: true);

            base.Action(info);

            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.Player:
                    _targetObject = TargetPlayer;
                    break;
                case Enumerators.AffectObjectType.Character:                   
                    _targetObject = TargetUnit;
                    TargetUnit.HandleDefenseBuffer(Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(AffectObjectType), AffectObjectType, null);
            }

            if(_targetObject != null)
            {
                InvokeUseAbilityEvent(
                        new List<ParametrizedAbilityBoardObject>
                        {
                            new ParametrizedAbilityBoardObject(_targetObject)
                        }
                    );
            }

            InvokeActionTriggered();
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            IBoardObject caller = AbilityUnitOwner;
            Enumerators.ActionType actionType;
            switch (_targetObject)
            {
                case CardModel unit:
                    BattleController.AttackUnitByAbility(caller, AbilityData, unit);
                    actionType = Enumerators.ActionType.CardAffectingCard;
                    break;
                case Player player:
                    BattleController.AttackPlayerByAbility(caller, AbilityData, player);
                    actionType = Enumerators.ActionType.CardAffectingOverlord;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_targetObject), _targetObject, null);
            }

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = actionType,
                Caller = AbilityUnitOwner,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = _targetObject,
                            HasValue = true,
                            Value = -AbilityData.Value
                        }
                    }
            });

            AbilityProcessingAction?.ForceCompleteAction();
        }
    }
}
