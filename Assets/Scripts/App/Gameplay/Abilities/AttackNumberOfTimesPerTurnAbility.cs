// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class AttackNumberOfTimesPerTurnAbility : AbilityBase
    {
        public Enumerators.AttackInfoType attackInfo;

        public int value = 1;

        private int _numberOfAttacksWas;

        public AttackNumberOfTimesPerTurnAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            value = ability.value;
            attackInfo = ability.attackInfoType;
        }

        public override void Activate()
        {
            base.Activate();

            abilityUnitOwner.attackInfoType = attackInfo;
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

            if (_numberOfAttacksWas < value)
            {
                abilityUnitOwner.ForceSetCreaturePlayable();
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
