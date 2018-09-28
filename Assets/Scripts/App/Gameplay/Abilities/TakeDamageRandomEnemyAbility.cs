using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TakeDamageRandomEnemyAbility : AbilityBase
    {
        public int Value { get; }

        public TakeDamageRandomEnemyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
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

        public override void Action(object info = null)
        {
            base.Action(info);

            List<object> allies = new List<object>();

            allies.AddRange(GetOpponentOverlord().BoardCards);
            allies.Add(GetOpponentOverlord());

            allies = InternalTools.GetRandomElementsFromList(allies, 1);

            for (int i = 0; i < allies.Count; i++)
            {
                object ally = allies[i];
                switch (allies[i])
                {
                    case Player allyPlayer:
                        BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, allyPlayer);
                        CreateVfx(allyPlayer.AvatarObject.transform.position, true, 5f, true);
                        break;
                    case BoardUnitView allyUnit:
                        BattleController.AttackUnitByAbility(GetCaller(), AbilityData, allyUnit.Model);
                        CreateVfx(allyUnit.Transform.position, true, 5f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ally), ally, null);
                }
            }
        }
    }
}
