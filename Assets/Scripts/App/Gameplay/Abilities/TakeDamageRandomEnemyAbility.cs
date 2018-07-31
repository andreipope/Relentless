// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class TakeDamageRandomEnemyAbility : AbilityBase
    {
        public int value = 0;

        public TakeDamageRandomEnemyAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.AT_START)
                return;

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");

            Action();
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
        }

        protected override void UnitOnAttackEventHandler(object info, int damage)
        {
            base.UnitOnAttackEventHandler(info, damage);
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
                if (allies[i] is Player)
                {
                    _battleController.AttackPlayerByAbility(GetCaller(), abilityData, (allies[i] as Player));
                    CreateVFX((allies[i] as Player).AvatarObject.transform.position, true, 5f, true);
                }
                else if (allies[i] is BoardUnit)
                {
                    _battleController.AttackUnitByAbility(GetCaller(), abilityData, (allies[i] as BoardUnit));
                    CreateVFX((allies[i] as BoardUnit).transform.position, true, 5f);
                }
            }
        }
    }
}
