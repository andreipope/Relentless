using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class HealTargetAbility : AbilityBase
    {
        public int Value { get; }

        public int Count { get; }

        public Enumerators.AbilitySubTrigger SubTrigger { get; }

        private List<BoardObject> _targets;

        private Action _vfxAnimationEndedCallback;

        public HealTargetAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Count = ability.Count;
            SubTrigger = ability.AbilitySubTrigger;

            _targets = new List<BoardObject>();
        }

        public override void Activate()
        {
            base.Activate();
            
            if (AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);
            }

            if (AbilityCallType == Enumerators.AbilityCallType.ENTRY)
            {
                if (AbilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
                {
                   if(SubTrigger == Enumerators.AbilitySubTrigger.YourOverlord)
                   {
                        _targets.Add(PlayerCallerOfAbility);
                        InvokeActionTriggered(_targets);
                        _vfxAnimationEndedCallback = HealOverlord;
                   }
                   else
                   {
                        HealRandomCountOfAllies();
                        InvokeActionTriggered(_targets);
                        _vfxAnimationEndedCallback = HealRandomCountOfAlliesCompleted;
                    }
                }
             }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
				AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

                _targets.Add(TargetUnit);
                InvokeActionTriggered(_targets);
                _vfxAnimationEndedCallback = HealSelectedTarget;
            }
        }

        private void HealOverlord()
        {
            HealTarget(PlayerCallerOfAbility, Value);

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
            {
                PlayerCallerOfAbility
            }, AbilityData.AbilityType, Protobuf.AffectObjectType.Types.Enum.Player);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingOverlord,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                        Target = PlayerCallerOfAbility,
                        HasValue = true,
                        Value = AbilityData.Value
                    }
                }
            });
        }

        private void HealSelectedTarget()
        {
            BoardObject boardObject = AffectObjectType == Enumerators.AffectObjectType.Player ? (BoardObject)TargetPlayer : TargetUnit;

            Enumerators.ActionType actionType = AffectObjectType == Enumerators.AffectObjectType.Player ?
                                Enumerators.ActionType.CardAffectingOverlord : Enumerators.ActionType.CardAffectingCard;

            HealTarget(boardObject, Value);

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
            {
                boardObject
            }, AbilityData.AbilityType,  (Protobuf.AffectObjectType.Types.Enum) AffectObjectType);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = actionType,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                        Target = boardObject,
                        HasValue = true,
                        Value = AbilityData.Value
                    }
                }
            });
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            _vfxAnimationEndedCallback?.Invoke();

            AbilityProcessingAction?.ForceActionDone();
        }

        private void HealRandomCountOfAllies()
        {
            if (PredefinedTargets != null)
            {
                _targets = PredefinedTargets.Select(x => x.BoardObject).ToList();
            }
            else
            {

                if (AbilityData.AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.PLAYER_CARD))
                {
                    _targets.AddRange(PlayerCallerOfAbility.BoardCards.Select(x => x.Model));

                    if (AbilityUnitOwner != null && _targets.Contains(AbilityUnitOwner))
                        _targets.Remove(AbilityUnitOwner);
                }

                if (AbilityData.AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.PLAYER))
                {
                    _targets.Add(PlayerCallerOfAbility);
                }

                _targets = InternalTools.GetRandomElementsFromList(_targets, Count);
            }
        }

        private void HealRandomCountOfAlliesCompleted()
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            int value = 0;
            foreach (BoardObject boardObject in _targets)
            {
                switch (boardObject)
                {
                    case BoardUnitModel unit:
                        value = unit.MaxCurrentHp - unit.CurrentHp;
                        break;
                    case Player player:
                        value = player.MaxCurrentHp - player.Defense;
                        break;
                }

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                    Target = boardObject,
                    HasValue = true,
                    Value = value
                });

                HealTarget(boardObject, value);
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, _targets, AbilityData.AbilityType, Protobuf.AffectObjectType.Types.Enum.Character);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCardsWithOverlord,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });
        }

        private void HealTarget(BoardObject boardObject, int value)
        {
            switch (boardObject)
            {
                case Player player:
                    BattleController.HealPlayerByAbility(GetCaller(), AbilityData, player, value);
                    break;
                case BoardUnitModel unit:
                    BattleController.HealUnitByAbility(GetCaller(), AbilityData, unit, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(AffectObjectType), AffectObjectType, null);
            }
        }
    }
}
