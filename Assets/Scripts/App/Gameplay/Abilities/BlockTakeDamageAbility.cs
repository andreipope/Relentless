using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class BlockTakeDamageAbility : AbilityBase
    {
        private int Damage { get; }

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
                ApplyMaximumDamageBuff(AbilityUnitOwner, Damage);

                InvokeUseAbilityEvent();
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                ApplyMaximumDamageBuff(TargetUnit, Damage);

                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });
            }
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.UntilStartOfNextPlayerTurn)
            {
                ApplyMaximumDamageBuff(AbilityUnitOwner, 999);
            }
        }

        private void ApplyMaximumDamageBuff(CardModel boardUnit, int value)
        {
            boardUnit.SetMaximumDamageToUnit(value);
        }
    }
}
