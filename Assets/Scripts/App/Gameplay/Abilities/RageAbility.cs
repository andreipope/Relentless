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
                if (AbilityUnitViewOwner.Model.CurrentHp < AbilityUnitViewOwner.Model.MaxCurrentHp)
                {
                    _wasChanged = true;
                    AbilityUnitViewOwner.Model.BuffedDamage += Value;
                    AbilityUnitViewOwner.Model.CurrentDamage += Value;
                }
            }
            else
            {
                if (AbilityUnitViewOwner.Model.CurrentHp >= AbilityUnitViewOwner.Model.MaxCurrentHp)
                {
                    AbilityUnitViewOwner.Model.BuffedDamage -= Value;
                    AbilityUnitViewOwner.Model.CurrentDamage -= Value;
                    _wasChanged = false;
                }
            }
        }
    }
}
