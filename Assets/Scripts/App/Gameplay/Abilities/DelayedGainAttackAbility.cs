using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DelayedGainAttackAbility : DelayedAbilityBase
    {
        public int Value;

        public DelayedGainAttackAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            AbilityUnitOwner.CurrentDamage += Value;
            AbilityUnitOwner.BuffedDamage += Value;
        }
    }
}
