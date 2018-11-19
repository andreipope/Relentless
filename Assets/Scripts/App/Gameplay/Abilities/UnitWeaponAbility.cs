using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class UnitWeaponAbility : AbilityBase
    {
        public int Value;

        public int Health;

        public int Damage;

        public event Action TurnEndedEvent;

        public UnitWeaponAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Damage = ability.Damage;
            Health = ability.Health;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            AbilityUnitOwner.AddEffectOnUnit(Enumerators.EffectOnUnitType.Chainsaw);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            TargetUnit.CurrentDamage += Value;
            TargetUnit.BuffedDamage += Value;

            TargetUnit.CurrentHp += Health;
            TargetUnit.BuffedHp += Health;

            InvokeActionTriggered();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                if (TargetUnit != null)
                {
                    Action();

                    TargetUnit.UnitDied += TargetUnitDiedHandler;

                    AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                    {
                       TargetUnit
                    }, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
                }
            }
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (!GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            TurnEndedEvent?.Invoke();

            ActionEnd();
        }

        private void ActionEnd()
        {
            if (TargetUnit != null)
            {
                BattleController.AttackUnitByAbility(TargetUnit, AbilityData, TargetUnit, Damage);

                CreateVfx(BattlegroundController.GetBoardUnitViewByModel(TargetUnit).Transform.position, true, 5f);

                AbilityUnitOwner.RemoveEffectFromUnit(Enumerators.EffectOnUnitType.Chainsaw);
            }
        }

        private void TargetUnitDiedHandler()
        {
            if (TargetUnit != null)
            {
                TargetUnit.UnitDied -= TargetUnitDiedHandler;
            }

            AbilitiesController.DeactivateAbility(ActivityId);
        }
    }
}
