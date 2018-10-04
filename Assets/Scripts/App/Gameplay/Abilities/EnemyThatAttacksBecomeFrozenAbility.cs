using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class EnemyThatAttacksBecomeFrozenAbility : AbilityBase
    {
        public int Value { get; } = 1;

        public EnemyThatAttacksBecomeFrozenAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        protected override void UnitDamagedHandler(BoardObject from)
        {
            base.UnitDamagedHandler(from);

            if (AbilityCallType != Enumerators.AbilityCallType.AT_DEFENCE)
                return;

            ((BoardUnitModel)from)?.Stun(Enumerators.StunType.FREEZE, Value);

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                {
                    from
                }, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }
    }
}
