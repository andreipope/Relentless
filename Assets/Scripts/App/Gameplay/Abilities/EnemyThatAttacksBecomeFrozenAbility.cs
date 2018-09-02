using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class EnemyThatAttacksBecomeFrozenAbility : AbilityBase
    {
        public int Value = 1;

        public EnemyThatAttacksBecomeFrozenAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }

        protected override void UnitGotDamageEventHandler(object from)
        {
            base.UnitGotDamageEventHandler(from);

            if (AbilityCallType != Enumerators.AbilityCallType.AtDefence)
                return;

            (@from as BoardUnit)?.Stun(Enumerators.StunType.Freeze, Value);
        }
    }
}
