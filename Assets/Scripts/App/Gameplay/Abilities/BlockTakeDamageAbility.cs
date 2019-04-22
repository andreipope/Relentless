using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class BlockTakeDamageAbility : AbilityBase
    {
        private int Damage { get; }
        private BoardUnitModel _targetedUnit;

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
                ApplyMaximumDamageBuff(_targetedUnit, 999);
            }
        }

        private void ApplyMaximumDamageBuff(CardModel boardUnit, int value)
        {
            boardUnit?.SetMaximumDamageToUnit(value);
        }
    }
}
