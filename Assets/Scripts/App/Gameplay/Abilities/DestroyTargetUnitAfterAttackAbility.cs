// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DestroyTargetUnitAfterAttackAbility : AbilityBase
    {
        public DestroyTargetUnitAfterAttackAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
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

        protected override void UnitOnAttackEventHandler(object from, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(from, damage, isAttacker);

            if (abilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            Action(from);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            var unit = info as BoardUnit;

            _battlegroundController.DestroyBoardUnit(unit);
        }
    }
}
