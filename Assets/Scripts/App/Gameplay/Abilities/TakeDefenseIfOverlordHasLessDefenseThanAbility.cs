using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
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

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (PlayerCallerOfAbility.Hp <= Health)
            {
                AbilityUnitOwner.BuffedHp += Value;
                AbilityUnitOwner.CurrentHp += Value;

                // CreateVFX(abilityUnitOwner.transform.position, true, 5f);
            }
        }
    }
}
