using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

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

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
        }

        protected override void UnitHpChangedHandler(int oldValue, int newValue)
        {
            base.UnitHpChangedHandler(oldValue, newValue);

            if (!PvPManager.UseBackendGameLogic)
            {
                if (!_wasChanged)
                {
                    if (AbilityUnitOwner.CurrentHp < AbilityUnitOwner.MaxCurrentHp)
                    {
                        _wasChanged = true;
                        AbilityUnitOwner.BuffedDamage += Value;
                        AbilityUnitOwner.CurrentDamage += Value;
                        InvokeActionTriggered(true);
                    }
                }
                else
                {
                    if (AbilityUnitOwner.CurrentHp >= AbilityUnitOwner.MaxCurrentHp)
                    {
                        AbilityUnitOwner.BuffedDamage -= Value;
                        AbilityUnitOwner.CurrentDamage -= Value;
                        _wasChanged = false;
                        InvokeActionTriggered(false);
                    }
                }
            }
        }
    }
}
