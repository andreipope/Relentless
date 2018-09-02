using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DestroyTargetUnitAfterAttackAbility : AbilityBase
    {
        public DestroyTargetUnitAfterAttackAbility(Enumerators.CardKind cardKind, AbilityData ability)
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

        public override void Action(object info = null)
        {
            base.Action(info);

            BoardUnit unit = info as BoardUnit;

            BattlegroundController.DestroyBoardUnit(unit);
        }

        protected override void UnitOnAttackEventHandler(object from, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(from, damage, isAttacker);

            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            Action(from);
        }
    }
}
