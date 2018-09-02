using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
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
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }

        protected override void UnitHPChangedEventHandler()
        {
            base.UnitHPChangedEventHandler();

            // if (abilityCallType != Enumerators.AbilityCallType.GOT_DAMAGE)
            // return;
            if (!_wasChanged)
            {
                if (AbilityUnitOwner.CurrentHp < AbilityUnitOwner.MaxCurrentHp)
                {
                    _wasChanged = true;
                    AbilityUnitOwner.BuffedDamage += Value;
                    AbilityUnitOwner.CurrentDamage += Value;
                }
            } else
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
