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
            Debug.LogWarning("CREATING ABILITY HEAL ");
            Value = ability.Value;
            Count = ability.Count;
            SubTrigger = ability.SubTrigger;

            _targets = new List<BoardObject>();
        }

        public override void Activate()
        {
            Debug.LogWarning("ACTIVATING ABILITY");
            base.Activate();

            if (AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);
            }

            if (AbilityTrigger == Enumerators.AbilityTrigger.DEATH || AbilityTrigger == Enumerators.AbilityTrigger.END)
            {
                InvokeUseAbilityEvent();
            }

            if (AbilityTrigger == Enumerators.AbilityTrigger.ENTRY)
            {
                if (AbilityActivity == Enumerators.AbilityActivity.PASSIVE)
                {
                    if(SubTrigger == Enumerators.AbilitySubTrigger.YourOverlord)
                    {
                       InvokeUseAbilityEvent();
                        _targets.Add(PlayerCallerOfAbility);

                        _vfxAnimationEndedCallback = HealOverlord;
                        InvokeActionTriggered(_targets);
                    }
                    else if (SubTrigger == Enumerators.AbilitySubTrigger.AllOtherAllyUnitsInPlay)
                    {
                        _targets.AddRange(PlayerCallerOfAbility.PlayerCardsController.
                            CardsOnBoard.Where(unit => unit != AbilityUnitOwner && !unit.IsDead &&
                                                unit.CurrentDefense > 0 && unit.IsUnitActive));
                        _vfxAnimationEndedCallback = HealRandomCountOfAlliesCompleted;
                        InvokeActionTriggered(_targets);
                    }
                    else if (SubTrigger == Enumerators.AbilitySubTrigger.AllAllyUnitsInPlay)
                    {
                        _targets.AddRange(PlayerCallerOfAbility.PlayerCardsController.
                            CardsOnBoard.Where(unit => unit != AbilityUnitOwner && !unit.IsDead &&
                                                unit.CurrentDefense > 0 && unit.IsUnitActive));
                        _vfxAnimationEndedCallback = HealRandomCountOfAlliesCompleted;
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

        protected override void UnitDiedHandler()
        {
            Debug.LogWarning("GETTING HERE");
            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
            {
                base.UnitDiedHandler();
                return;
            }

            if (SubTrigger == Enumerators.AbilitySubTrigger.YourOverlord)
            {
                Debug.LogWarning("GETTING HERE 1");
                _targets.Add(PlayerCallerOfAbility);
                _vfxAnimationEndedCallback = HealOverlord;
                InvokeActionTriggered(_targets);
            } 
            else
            {
                Debug.LogWarning("GETTING HERE 2");
                base.UnitDiedHandler();
            }
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.END)
                return;

            if (SubTrigger == Enumerators.AbilitySubTrigger.AllAllyUnitsInPlay)
            {
                _targets.Clear();

                _targets.AddRange(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard);

                _vfxAnimationEndedCallback = HealRandomCountOfAlliesCompleted;
                InvokeActionTriggered(_targets);
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

            int value = Value;
            if (CheckValueForRestoring(boardObject, ref value))
            {
                HealTarget(boardObject, value);

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
                            Value = value
                        }
                    }
                });
            }

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(boardObject)
                }
            );
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();
            
            if (AbilityTrigger == Enumerators.AbilityTrigger.DEATH)
            {
                base.UnitDiedHandler();
            }

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
                if (AbilityData.Targets.Contains(Enumerators.Target.PLAYER_CARD))
                {
                    _targets.AddRange(PlayerCallerOfAbility.CardsOnBoard.Where(x => x != AbilityUnitOwner && x.CurrentDefense < x.MaxCurrentDefense));
                }

                if (AbilityData.Targets.Contains(Enumerators.Target.PLAYER))
                {
                    _targets.Add(PlayerCallerOfAbility);
                }

                _targets = InternalTools.GetRandomElementsFromList(_targets, Count);
            }
        }

        private void HealRandomCountOfAlliesCompleted()
        {
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

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

                if (CheckValueForRestoring(boardObject, ref value))
                {
                    targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldBuff,
                        Target = boardObject,
                        HasValue = true,
                        Value = value
                    });

                    HealTarget(boardObject, value);
                }
            }

            if (AbilityTrigger != Enumerators.AbilityTrigger.END)
            {
                InvokeUseAbilityEvent(
                    _targets
                        .Select(x => new ParametrizedAbilityBoardObject(x))
                        .ToList()
                );
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCardsWithOverlord,
                Caller = GetCaller(),
                TargetEffects = targetEffects
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

        private bool CheckValueForRestoring(BoardObject boardObject, ref int value)
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

            return value != 0;
        }
    }
}
