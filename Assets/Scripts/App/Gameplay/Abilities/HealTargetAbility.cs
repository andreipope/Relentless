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

        private List<IBoardObject> _targets;

        private Action _vfxAnimationEndedCallback;

        public HealTargetAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Count = ability.Count;
            SubTrigger = ability.SubTrigger;

            _targets = new List<IBoardObject>();
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
                if (AbilityActivity == Enumerators.AbilityActivity.PASSIVE)
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

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger == Enumerators.AbilityTrigger.DEATH)
                return;

            if (SubTrigger == Enumerators.AbilitySubTrigger.YourOverlord)
            {
                _targets.Add(PlayerCallerOfAbility);

                _vfxAnimationEndedCallback = HealOverlord;
                InvokeActionTriggered(_targets);
            }
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityTrigger == Enumerators.AbilityTrigger.END)
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

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(PlayerCallerOfAbility)
                }
            );

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingOverlord,
                Caller = AbilityUnitOwner,
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
            IBoardObject boardObject = AffectObjectType == Enumerators.AffectObjectType.Player ? (IBoardObject)TargetPlayer : TargetUnit;

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
                Caller = AbilityUnitOwner,
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
                _targets = PredefinedTargets.Select(x => x.IBoardObject).ToList();
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
            foreach (IBoardObject boardObject in _targets)
            {
                switch (boardObject)
                {
                    case CardModel unit:
                        value = unit.MaxCurrentDefense - unit.CurrentDefense;
                        break;
                    case Player player:
                        value = player.MaxCurrentDefense - player.Defense;
                        break;
                }

                if (value > Value)
                    value = Value;

                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
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
                Caller = AbilityUnitOwner,
                TargetEffects = targetEffects
            });
        }

        private void HealTarget(IBoardObject boardObject, int value)
        {
            switch (boardObject)
            {
                case Player player:
                    BattleController.HealPlayerByAbility(AbilityUnitOwner, AbilityData, player, value);
                    break;
                case CardModel unit:
                    BattleController.HealUnitByAbility(AbilityUnitOwner, AbilityData, unit, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(AffectObjectType), AffectObjectType, null);
            }
        }
    }
}
