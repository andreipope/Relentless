using System;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class HealTargetAbility : AbilityBase
    {
        public int Value { get; }

        public HealTargetAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            object caller = AbilityUnitViewOwner != null ? AbilityUnitViewOwner : (object) BoardSpell;

            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.PLAYER:
                    BattleController.HealPlayerByAbility(caller, AbilityData, TargetPlayer);
                    break;
                case Enumerators.AffectObjectType.CHARACTER:
                    BattleController.HealUnitByAbility(caller, AbilityData, TargetUnitView.Model);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(AffectObjectType), AffectObjectType, null);
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }
    }
}
