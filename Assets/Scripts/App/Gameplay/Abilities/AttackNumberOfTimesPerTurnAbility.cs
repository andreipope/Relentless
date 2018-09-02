using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class AttackNumberOfTimesPerTurnAbility : AbilityBase
    {
        public Enumerators.AttackInfoType AttackInfo;

        public int Value = 1;

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
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);

            if (!isAttacker)

                return;

            _numberOfAttacksWas++;

            if (_numberOfAttacksWas < Value)
            {
                AbilityUnitOwner.ForceSetCreaturePlayable();
            }
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();
            _numberOfAttacksWas = 0;
        }

        private void Action()
        {
        }
    }
}
