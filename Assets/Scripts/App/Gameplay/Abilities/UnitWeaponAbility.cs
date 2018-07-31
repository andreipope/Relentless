// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class UnitWeaponAbility : AbilityBase
    {
        public int value = 0;
        public int damage = 0;

        public UnitWeaponAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
            damage = ability.damage;
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

            if(_isAbilityResolved)
            {
                Action();
            }
        }

        protected override void UnitOnAttackEventHandler(object info, int damage)
        {
            base.UnitOnAttackEventHandler(info, damage);
        }

        protected override void OnEndTurnEventHandler()
        {
            base.OnEndTurnEventHandler();

            ActionEnd();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if(targetUnit != null)
            {
                targetUnit.CurrentDamage += value;
                targetUnit.BuffedDamage += value;

                CreateVFX(targetUnit.transform.position, true, 5f);
            }
        }

        private void ActionEnd()
        {
            _battleController.AttackUnitByAbility(abilityUnitOwner, abilityData, abilityUnitOwner, damage);
            CreateVFX(targetUnit.transform.position, true, 5f);
        }
    }
}
