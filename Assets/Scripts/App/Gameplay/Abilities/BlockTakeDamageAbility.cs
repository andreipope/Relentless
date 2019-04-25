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

                _animationEndedAction = () =>
                {
                    ApplyMaximumDamageBuff(_targetedUnit, Damage);
                };

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

                _animationEndedAction = () =>
                {
                    ApplyMaximumDamageBuff(_targetedUnit, Damage);
                };

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
                _animationEndedAction = () =>
                {
                    ApplyMaximumDamageBuff(_targetedUnit, 999);
                };

                InvokeActionTriggered(_targetedUnit);
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            _animationEndedAction?.Invoke();
            _animationEndedAction = null;
        }

        private void ApplyMaximumDamageBuff(BoardUnitModel boardUnit, int value)
        {
            Debug.LogError(1111);
            boardUnit?.SetMaximumDamageToUnit(value);
        }
    }
}
