// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class FreezeNumberOfRandomAllyAbility : AbilityBase
    {
        public int value = 0;
        public int turns = 1;

        public FreezeNumberOfRandomAllyAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
            turns = ability.turns;      
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");

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

        public override void Action(object info = null)
        {
            base.Action(info);

            List<object> allies = new List<object>();

            allies.AddRange(playerCallerOfAbility.BoardCards);
            allies.Add(playerCallerOfAbility);

            allies = InternalTools.GetRandomElementsFromList(allies, value);

            for (int i = 0; i < allies.Count; i++)
            {
                if (allies[i] is Player)
                {
                    (allies[i] as Player).Stun(Enumerators.StunType.FREEZE, turns);
                    CreateVFX((allies[i] as Player).AvatarObject.transform.position, true, 5f);
                }
                else if (allies[i] is BoardUnit)
                {
                    (allies[i] as BoardUnit).Stun(Enumerators.StunType.FREEZE, turns);
                    CreateVFX((allies[i] as BoardUnit).transform.position, true, 5f);
                }
            }
        }
    }
}
