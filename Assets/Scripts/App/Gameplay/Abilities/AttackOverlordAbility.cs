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

        public List<Enumerators.AbilityTargetType> TargetTypes { get; }

        public AttackOverlordAbility() : base()
        {

        }

        public AttackOverlordAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetTypes = ability.AbilityTargetTypes;
            Value = ability.Value;
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

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Player targetObject = null; 

            foreach (Enumerators.AbilityTargetType target in TargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT:
                        targetObject = GetOpponentOverlord();
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        targetObject = PlayerCallerOfAbility;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }

                if(!PvPManager.UseBackendGameLogic)
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

        public void ActivateAbility(AttackOverlordOutcome outcome)
        {
            BoardObject boardObject = BattlegroundController.GetBoardObjectByInstanceId(outcome.PlayerInstanceId);
            if (boardObject is Player targetOverlord)
            {
                BattleController.AttackPlayer(targetOverlord, outcome.Damage);
                targetOverlord.Defense = outcome.NewDefence;
            }
            else
            {
                throw new Exception("Attack overlord should apply only on Player Overlord");
            }
        }
    }

    public class AttackOverlordOutcome
    {
        public InstanceId PlayerInstanceId;
        public int Damage;
        public int NewDefence;
    }
}
