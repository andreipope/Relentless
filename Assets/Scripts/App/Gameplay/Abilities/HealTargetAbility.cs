using System;
using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class HealTargetAbility : AbilityBase
    {
        public int Value { get; }

        public HealTargetAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            BoardObject caller = AbilityUnitOwner != null ? (BoardObject)AbilityUnitOwner : BoardSpell;

            object target = null;

            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.Player:
                    BattleController.HealPlayerByAbility(caller, AbilityData, TargetPlayer);

                    AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                    {
                        TargetPlayer
                    }, AbilityData.AbilityType, Protobuf.AffectObjectType.Player);

                    target = TargetPlayer;
                    break;
                case Enumerators.AffectObjectType.Character:
                    BattleController.HealUnitByAbility(caller, AbilityData, TargetUnit);

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
                        ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                        Target = target,
                        HasValue = true,
                        Value = AbilityData.Value
                    }
                }
            });
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }
    }
}
