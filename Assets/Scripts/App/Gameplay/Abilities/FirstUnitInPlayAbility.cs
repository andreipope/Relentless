// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class FirstUnitInPlayAbility : AbilityBase
    {
        public int value;

        public FirstUnitInPlayAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.AT_START)
                return;

            Action();
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

        public override void Action(object info = null)
        {
            base.Action(info);

            if (playerCallerOfAbility.BoardCards.Count == 0 ||
                (playerCallerOfAbility.BoardCards.Count == 1 && playerCallerOfAbility.BoardCards[0].Equals(abilityUnitOwner)))
            {

                abilityUnitOwner.BuffedHP += value;
                abilityUnitOwner.CurrentHP += value;

                abilityUnitOwner.BuffedDamage += value;
                abilityUnitOwner.CurrentDamage += value;
            }
        }
    }
}
