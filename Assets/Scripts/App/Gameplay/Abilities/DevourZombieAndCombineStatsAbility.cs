// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class DevourZombiesAndCombineStatsAbility : AbilityBase
    {
        public int value;

        public DevourZombiesAndCombineStatsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.ENTRY)
            
return;

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            if (value == -1)
            {
                DevourAllAllyZombies();
            }
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved && (value > 0))
            {
                DevourTargetZombie(targetUnit);
            }
        }

        private void DevourAllAllyZombies()
        {
            List<BoardUnit> units = playerCallerOfAbility.BoardCards;

            foreach (BoardUnit unit in units)
            {
                DevourTargetZombie(unit);
            }
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
