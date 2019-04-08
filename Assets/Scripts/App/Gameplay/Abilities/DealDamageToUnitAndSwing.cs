using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DealDamageToUnitAndSwing : AbilityBase
    {
        public int Value { get; }

        public int Damage { get; }

        public DealDamageToUnitAndSwing(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Damage = ability.Damage;
        }

        public override void Activate()
        {
            base.Activate();
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

            Action();
        }

        public override void Action(object param = null)
        {
            base.Action(param);

            TakeDamageToUnit(TargetUnit, Value);

            List<BoardUnitModel> adjacent = BattlegroundController.GetAdjacentUnitsToUnit(TargetUnit);

            foreach (BoardUnitModel unit in adjacent)
            {
                TakeDamageToUnit(unit, Damage);
            }

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                }
            );
        }

        private void TakeDamageToUnit(BoardUnitModel unit, int value)
        {
            BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, unit, value);
        }
    }
}
