// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DevourZombiesAndCombineStatsAbility : AbilityBase
    {
        public int value;

        public DevourZombiesAndCombineStatsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.AT_START)
                return;

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            if (value == -1)
                DevourAllAllyZombies();
        }

        public override void Update() { }

        public override void Dispose() { }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved && value > 0)
                DevourTargetZombie(targetUnit);
        }

        private void DevourAllAllyZombies()
        {
            var units = playerCallerOfAbility.BoardCards;

            foreach (var unit in units)
                DevourTargetZombie(unit);
        }

        private void DevourTargetZombie(BoardUnit unit)
        {
            if (unit.Equals(abilityUnitOwner))
                return;

            int health = unit.initialHP;
            int damage = unit.initialDamage;

            _battlegroundController.DestroyBoardUnit(unit);

            abilityUnitOwner.BuffedHP += health;
            abilityUnitOwner.CurrentHP += health;

            abilityUnitOwner.BuffedDamage += damage;
            abilityUnitOwner.CurrentDamage += damage;

            CreateVFX(unit.transform.position, true, 5f);
        }
    }
}