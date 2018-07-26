// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DelayedGainAttackAbility : DelayedAbilityBase
    {
        public int value = 0;

        public DelayedGainAttackAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            abilityUnitOwner.CurrentDamage += value;
            abilityUnitOwner.BuffedDamage += value;
        }
    }
}
