using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class AttackOverlordAbility : AbilityBase
    {
        public int Value { get; }

        public List<Enumerators.AbilityTargetType> TargetTypes { get; }

        public AttackOverlordAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetTypes = ability.AbilityTargetTypes;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");

            Action();
        }

        public override void Action(object param = null)
        {
            base.Action(param);

            foreach (Enumerators.AbilityTargetType target in TargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT:
                        GetOpponentOverlord().Hp -= Value;
                        CreateVfx(GetOpponentOverlord().AvatarObject.transform.position, true, 5f, true);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        PlayerCallerOfAbility.Hp -= Value;
                        CreateVfx(PlayerCallerOfAbility.AvatarObject.transform.position, true, 5f, true);
                        break;
                    default: continue;
                }
            }
        }
    }
}
