using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AttackNumberOfTimesPerTurnAbility : AbilityBase
    {
        public Enumerators.AttackInfoType AttackInfo { get; }

        public int Value { get; }

        private int _numberOfAttacksWas;

        public AttackNumberOfTimesPerTurnAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            AttackInfo = ability.AttackInfoType;
        }

        public override void Activate()
        {
            base.Activate();

            AbilityUnitOwner.AttackInfoType = AttackInfo;

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);

            if (!isAttacker)
                return;

            _numberOfAttacksWas++;

            if (_numberOfAttacksWas < Value)
            {
                AbilityUnitOwner.ForceSetCreaturePlayable();
            }
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();
            _numberOfAttacksWas = 0;
        }
    }
}
