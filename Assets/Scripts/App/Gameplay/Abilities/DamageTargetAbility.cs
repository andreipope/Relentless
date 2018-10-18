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

        public DamageTargetAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            TargetUnitStatusType = ability.TargetUnitStatusType;
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                InvokeActionTriggered();
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            object caller = AbilityUnitOwner != null ? AbilityUnitOwner : (object)BoardSpell;

            object target = null;

            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.Player:
                    BattleController.AttackPlayerByAbility(caller, AbilityData, TargetPlayer);
                    AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                    {
                        TargetPlayer
                    }, AbilityData.AbilityType, Protobuf.AffectObjectType.Player);

                    target = TargetPlayer;
                    break;
                case Enumerators.AffectObjectType.Character:
                    BattleController.AttackUnitByAbility(caller, AbilityData, TargetUnit);
                    AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                    {
                        TargetUnit
                    }, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);

                    target = TargetUnit;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(AffectObjectType), AffectObjectType, null);
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = target,
                            HasValue = true,
                            Value = -AbilityData.Value
                        }
                    }
            });
        }
    }
}
