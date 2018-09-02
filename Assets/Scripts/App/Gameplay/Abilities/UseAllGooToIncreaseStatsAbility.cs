using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class UseAllGooToIncreaseStatsAbility : AbilityBase
    {
        public int Value;

        public UseAllGooToIncreaseStatsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.Entry)
                return;

            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (PlayerCallerOfAbility.Goo == 0)
                return;

            int increaseOn = 0;

            increaseOn = PlayerCallerOfAbility.Goo * Value;
            AbilityUnitOwner.BuffedHp += increaseOn;
            AbilityUnitOwner.CurrentHp += increaseOn;

            increaseOn = PlayerCallerOfAbility.Goo * Value;
            AbilityUnitOwner.BuffedDamage += increaseOn;
            AbilityUnitOwner.CurrentDamage += increaseOn;

            PlayerCallerOfAbility.Goo = 0;
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }
    }
}
