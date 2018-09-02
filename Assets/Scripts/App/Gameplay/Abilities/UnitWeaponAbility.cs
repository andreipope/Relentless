using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class UnitWeaponAbility : AbilityBase
    {
        public int Value;

        public int Damage;

        public UnitWeaponAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Damage = ability.Damage;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (TargetUnit != null)
            {
                TargetUnit.CurrentDamage += Value;
                TargetUnit.BuffedDamage += Value;

                CreateVfx(TargetUnit.Transform.position, true, 5f);
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (IsAbilityResolved)
            {
                Action();

                if (TargetUnit != null)
                {
                    TargetUnit.UnitOnDieEvent += TargetUnitOnDieEventHandler;
                }
            }
        }

        protected override void OnEndTurnEventHandler()
        {
            base.OnEndTurnEventHandler();

            if (!GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            ActionEnd();
        }

        private void ActionEnd()
        {
            if (TargetUnit != null)
            {
                BattleController.AttackUnitByAbility(TargetUnit, AbilityData, TargetUnit, Damage);

                CreateVfx(TargetUnit.Transform.position, true, 5f);
            }
        }

        private void TargetUnitOnDieEventHandler()
        {
            if (TargetUnit != null)
            {
                TargetUnit.UnitOnDieEvent -= TargetUnitOnDieEventHandler;
            }

            AbilitiesController.DeactivateAbility(ActivityId);
        }
    }
}
