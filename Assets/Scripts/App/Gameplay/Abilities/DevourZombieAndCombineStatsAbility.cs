// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DevourZombieAndCombineStatsAbility : AbilityBase
    {
        public DevourZombieAndCombineStatsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
          
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
        }

        public override void Update() { }

        public override void Dispose() { }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                Action();
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            int health = targetUnit.initialHP;
            int damage = targetUnit.initialDamage;

            _battlegroundController.DestroyBoardUnit(targetUnit);

            abilityUnitOwner.BuffedHP += health;
            abilityUnitOwner.CurrentHP += health;

            abilityUnitOwner.BuffedDamage += damage;
            abilityUnitOwner.CurrentDamage += damage;
        }
    }
}