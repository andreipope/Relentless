using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class EnemyThatAttacksBecomeFrozenAbility : AbilityBase
    {
        public int Value { get; } = 1;

        public EnemyThatAttacksBecomeFrozenAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        protected override void UnitDamagedHandler(object from)
        {
            base.UnitDamagedHandler(from);

            if (AbilityCallType != Enumerators.AbilityCallType.AT_DEFENCE)
                return;

            ((BoardUnitView) from)?.Model.Stun(Enumerators.StunType.FREEZE, Value);
        }
    }
}
