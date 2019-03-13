using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class HealTargetAbility : AbilityBase
    {
        private const int ZedKitId = 72;

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

            if (AbilityTrigger == Enumerators.AbilityTrigger.ENTRY)
            {
                if (AbilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
                {
                   if(SubTrigger == Enumerators.AbilitySubTrigger.YourOverlord)
                   {
                        _targets.Add(PlayerCallerOfAbility);

                        _vfxAnimationEndedCallback = HealOverlord;
                        InvokeActionTriggered(_targets);
                   }
                   else
                   {
                        SelectRandomCountOfAllies();

                        _vfxAnimationEndedCallback = HealRandomCountOfAlliesCompleted;
                        InvokeActionTriggered(_targets);
                    }
                }
             }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                _targets.Add(TargetUnit);

                _vfxAnimationEndedCallback = HealSelectedTarget;
                InvokeActionTriggered(_targets);
            }
        }

        private void HealOverlord()
        {
            HealTarget(PlayerCallerOfAbility, Value);

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(PlayerCallerOfAbility)
                }
            );

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

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(boardObject)
                }
            );

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
        }

        private void SelectRandomCountOfAllies()
        {
            if (PredefinedTargets != null)
            {
                _targets = PredefinedTargets.Select(x => x.BoardObject).ToList();
            }
            else
            {
                if (AbilityData.AbilityTarget.Contains(Enumerators.AbilityTarget.PLAYER_CARD))
                {
                    _targets.AddRange(PlayerCallerOfAbility.BoardCards.Where(x => x.Model != AbilityUnitOwner && x.Model.CurrentDefense < x.Model.MaxCurrentDefense).Select(x => x.Model));
                }

                if (AbilityData.AbilityTarget.Contains(Enumerators.AbilityTarget.PLAYER) && (BoardItem == null || BoardItem.BoardUnitModel.Prototype.MouldId != ZedKitId))
                {
                    _targets.Add(PlayerCallerOfAbility);
                }

                _targets = InternalTools.GetRandomElementsFromList(_targets, Count);
            }
        }

        private void HealRandomCountOfAlliesCompleted()
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            int value = Value;
            foreach (BoardObject boardObject in _targets)
            {
                switch (boardObject)
                {
                    case BoardUnitModel unit:
                        value = unit.MaxCurrentDefense - unit.CurrentDefense;
                        break;
                    case Player player:
                        value = player.MaxCurrentDefense - player.Defense;
                        break;
                }

                if (value > Value)
                    value = Value;

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                    Target = boardObject,
                    HasValue = true,
                    Value = value
                });

                HealTarget(boardObject, value);
            }

            InvokeUseAbilityEvent(
                _targets
                    .Select(x => new ParametrizedAbilityBoardObject(x))
                    .ToList()
            );

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
