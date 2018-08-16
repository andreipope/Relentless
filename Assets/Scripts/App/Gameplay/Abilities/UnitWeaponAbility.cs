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

                if (targetUnit != null)
                    targetUnit.UnitOnDieEvent += TargetUnitOnDieEventHandler;
            }
        }

        protected override void OnEndTurnEventHandler()
        {
            base.OnEndTurnEventHandler();

            if (!_gameplayManager.CurrentTurnPlayer.Equals(playerCallerOfAbility))
                return;

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
            if (targetUnit != null)
            {
                _battleController.AttackUnitByAbility(targetUnit, abilityData, targetUnit, damage);

                CreateVFX(targetUnit.transform.position, true, 5f);
            }
        }

        private void TargetUnitOnDieEventHandler()
        {
            if (targetUnit != null)
                targetUnit.UnitOnDieEvent -= TargetUnitOnDieEventHandler;

            _abilitiesController.DeactivateAbility(activityId);
        }
    }
}
