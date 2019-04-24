using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class DelayedLoseHeavyGainAttackAbility : DelayedAbilityBase
    {
        public int Value;

        public DelayedLoseHeavyGainAttackAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            AbilityUnitOwner.BuffedDamage += Value;
            AbilityUnitOwner.AddToCurrentDamageHistory(Value, Enumerators.ReasonForValueChange.AbilityBuff);

            AbilityUnitOwner.SetAsWalkerUnit();
        }
    }
}
