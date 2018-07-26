// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class RageAbility : AbilityBase
    {
        private bool _wasChanged = false;

        public int value = 0;

        public RageAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
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

        protected override void UnitOnAttackEventHandler(object info, int damage)
        {
            base.UnitOnAttackEventHandler(info, damage);
        }

        protected override void UnitHPChangedEventHandler(int oldHP, int newHP)
        {
            base.UnitHPChangedEventHandler(oldHP, newHP);

            if (abilityCallType != Enumerators.AbilityCallType.GOT_DAMAGE)
                return;

            if (!_wasChanged)
            {
                if (abilityUnitOwner.CurrentHP < abilityUnitOwner.MaxCurrentHP)
                {
                    _wasChanged = true;
                    abilityUnitOwner.BuffedDamage += value;
                    abilityUnitOwner.CurrentDamage += value;
                }
            }
            else
            {
                if (abilityUnitOwner.CurrentHP >= abilityUnitOwner.MaxCurrentHP)
                {
                    abilityUnitOwner.BuffedDamage -= value;
                    abilityUnitOwner.CurrentDamage -= value;
                    _wasChanged = false;
                }
            }
        }
    }
}
