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

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Types.Enum.Character);
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }

        public override void Action(object param = null)
        {
            base.Action(param);

            TakeDamageToUnit(TargetUnit, Value);

            List<BoardUnitView> adjacent = BattlegroundController.GetAdjacentUnitsToUnit(TargetUnit);

            foreach (BoardUnitView unitView in adjacent)
            {
                TakeDamageToUnit(unitView.Model, Damage);
            }
        }

        private void TakeDamageToUnit(BoardUnitModel unit, int value)
        {
            BattleController.AttackUnitByAbility(GetCaller(), AbilityData, unit, value);
        }
    }
}
