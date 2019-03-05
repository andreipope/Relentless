using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TakeDefenseIfOverlordHasLessDefenseThanAbility : AbilityBase
    {
        public int Value;

        public int Health;

        public TakeDefenseIfOverlordHasLessDefenseThanAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Health = ability.Health;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();

            InvokeUseAbilityEvent();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (PlayerCallerOfAbility.Defense <= Health)
            {
                AbilityUnitOwner.BuffedHp += Value;
                AbilityUnitOwner.CurrentHp += Value;
            }
        }
    }
}
