using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class EnemyThatAttacksBecomeFrozenAbility : AbilityBase
    {
        public int Value { get; } = 1;

        public EnemyThatAttacksBecomeFrozenAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        protected override void UnitGotDamageEventHandler(object from)
        {
            base.UnitGotDamageEventHandler(from);

            if (AbilityCallType != Enumerators.AbilityCallType.AT_DEFENCE)
                return;

            (from as BoardUnit)?.Stun(Enumerators.StunType.FREEZE, Value);
        }
    }
}
