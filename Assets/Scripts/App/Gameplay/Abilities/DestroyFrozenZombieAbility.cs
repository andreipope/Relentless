using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DestroyFrozenZombieAbility : AbilityBase
    {
        public DestroyFrozenZombieAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetUnitStatusType = ability.TargetUnitStatusType;
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null);

                InvokeActionTriggered();
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            TargetUnit.LastAttackingSetType = Enumerators.SetType.NONE;
            BattlegroundController.DestroyBoardUnit(TargetUnit);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                            Target = TargetUnit
                        }
                    }
            });

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
            {
               TargetUnit
            }, AbilityData.AbilityType, Protobuf.AffectObjectType.Types.Enum.Character);

            AbilityProcessingAction?.ForceActionDone();

        }
    }
}
