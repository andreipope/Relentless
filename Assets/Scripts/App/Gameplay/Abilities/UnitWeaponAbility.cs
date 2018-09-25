using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
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

            if (TargetUnitView != null)
            {
                TargetUnitView.Model.CurrentDamage += Value;
                TargetUnitView.Model.BuffedDamage += Value;

                CreateVfx(TargetUnitView.Transform.position, true, 5f);
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action();

                if (TargetUnitView != null)
                {
                    TargetUnitView.Model.UnitDied += TargetUnitDiedHandler;
                }
            }
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (!GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            ActionEnd();
        }

        private void ActionEnd()
        {
            if (TargetUnitView != null)
            {
                BattleController.AttackUnitByAbility(TargetUnitView, AbilityData, TargetUnitView.Model, Damage);

                CreateVfx(TargetUnitView.Transform.position, true, 5f);
            }
        }

        private void TargetUnitDiedHandler()
        {
            if (TargetUnitView != null)
            {
                TargetUnitView.Model.UnitDied -= TargetUnitDiedHandler;
            }

            AbilitiesController.DeactivateAbility(ActivityId);
        }
    }
}
