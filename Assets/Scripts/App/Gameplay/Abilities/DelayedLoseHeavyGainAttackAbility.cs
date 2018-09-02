// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DelayedLoseHeavyGainAttackAbility : DelayedAbilityBase
    {
        public int value;

        public DelayedLoseHeavyGainAttackAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            abilityUnitOwner.CurrentDamage += value;
            abilityUnitOwner.BuffedDamage += value;

            abilityUnitOwner.SetAsWalkerUnit();
        }
    }
}
