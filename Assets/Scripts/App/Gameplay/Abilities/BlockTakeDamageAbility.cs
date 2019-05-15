using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class BlockTakeDamageAbility : AbilityBase
    {
        private int Damage { get; }
        private BoardUnitModel _targetedUnit;

        private Action _animationEndedAction;

        private int _previousMaximumDamageBuff;

        public BlockTakeDamageAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            Damage = ability.Damage;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            if (AbilityData.Targets.Contains(Enumerators.Target.ITSELF))
            {
                _targetedUnit = AbilityUnitOwner;

                ApplyMaximumDamageBuff(_targetedUnit, Damage);

                InvokeActionTriggered(_targetedUnit);

                InvokeUseAbilityEvent();
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                _targetedUnit = TargetUnit;

                ApplyMaximumDamageBuff(_targetedUnit, Damage);

                InvokeActionTriggered(_targetedUnit);

                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(_targetedUnit)
                });
            }
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (!GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.UntilStartOfNextPlayerTurn)
            {
                ApplyMaximumDamageBuff(_targetedUnit, _previousMaximumDamageBuff);
                Deactivate();
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();
            _targetedUnit.IsPlayable = true;
        }

        private void ApplyMaximumDamageBuff(BoardUnitModel boardUnit, int value)
        {
            if (boardUnit != null)
            {
                _previousMaximumDamageBuff = boardUnit.MaximumDamageFromAnySource;
                boardUnit.SetMaximumDamageToUnit(value);

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = GetCaller(),
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        {
                            new PastActionsPopup.TargetEffectParam()
                            {
                                ActionEffectType = Enumerators.ActionEffectType.None,
                                Target = boardUnit
                            }
                        }
                });
            }
        }
    }
}
