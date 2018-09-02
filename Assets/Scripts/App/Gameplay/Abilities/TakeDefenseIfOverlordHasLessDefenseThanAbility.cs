// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class TakeDefenseIfOverlordHasLessDefenseThanAbility : AbilityBase
    {
        public int value;

        public int health;

        public TakeDefenseIfOverlordHasLessDefenseThanAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            value = ability.value;
            health = ability.health;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            if (abilityCallType != Enumerators.AbilityCallType.ENTRY)

                return;

            Action();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (playerCallerOfAbility.HP <= health)
            {
                abilityUnitOwner.BuffedHP += value;
                abilityUnitOwner.CurrentHP += value;

                // CreateVFX(abilityUnitOwner.transform.position, true, 5f);
            }
        }
    }
}
