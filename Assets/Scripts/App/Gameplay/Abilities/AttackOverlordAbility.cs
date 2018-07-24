// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class AttackOverlordAbility : AbilityBase
    {
        public int value = 1;
        public List<Enumerators.AbilityTargetType> targetTypes;

        public AttackOverlordAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.targetTypes = ability.abilityTargetTypes;
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.AT_START)
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

        public override void Action(object param = null)
        {
            base.Action(param);

            foreach(var target in targetTypes)
            {
                switch(target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT:
                        GetOpponentOverlord().HP -= value;
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        playerCallerOfAbility.HP -= value;
                        break;
                    default: continue;
                }
            }
        }
    }
}