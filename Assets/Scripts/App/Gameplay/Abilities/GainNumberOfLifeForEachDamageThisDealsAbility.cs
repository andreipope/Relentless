// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class GainNumberOfLifeForEachDamageThisDealsAbility : AbilityBase
    {
        public int value = 0;

        public GainNumberOfLifeForEachDamageThisDealsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
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

            if (abilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            Action(damage);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            int damageDeal = (int)info;

            abilityUnitOwner.BuffedHP += (value * damageDeal);
            abilityUnitOwner.CurrentHP += (value * damageDeal);

            CreateVFX(abilityUnitOwner.transform.position, true);
        }
    }
}
