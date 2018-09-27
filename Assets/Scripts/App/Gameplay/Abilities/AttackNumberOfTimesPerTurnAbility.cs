using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

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

            AbilityUnitViewOwner.Model.AttackInfoType = AttackInfo;
        }

        protected override void UnitAttackedHandler(object info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);

            if (!isAttacker)
                return;

            _numberOfAttacksWas++;

            if (_numberOfAttacksWas < Value)
            {
                AbilityUnitViewOwner.Model.ForceSetCreaturePlayable();
            }
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();
            _numberOfAttacksWas = 0;
        }
    }
}
