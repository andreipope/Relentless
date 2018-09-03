using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class RageAbility : AbilityBase
    {
        public int Value;

        private bool _wasChanged;

        public RageAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        protected override void UnitHpChangedHandler()
        {
            base.UnitHpChangedHandler();

            if (!_wasChanged)
            {
                if (AbilityUnitOwner.CurrentHp < AbilityUnitOwner.MaxCurrentHp)
                {
                    _wasChanged = true;
                    AbilityUnitOwner.BuffedDamage += Value;
                    AbilityUnitOwner.CurrentDamage += Value;
                }
            }
            else
            {
                if (AbilityUnitOwner.CurrentHp >= AbilityUnitOwner.MaxCurrentHp)
                {
                    AbilityUnitOwner.BuffedDamage -= Value;
                    AbilityUnitOwner.CurrentDamage -= Value;
                    _wasChanged = false;
                }
            }
        }
    }
}
