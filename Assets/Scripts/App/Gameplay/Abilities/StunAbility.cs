// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class StunAbility : AbilityBase
    {
        public Enumerators.StatType statType;
        public int value = 1;


        public StunAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.statType = ability.abilityStatType;
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (abilityEffectType)
            {
                case Enumerators.AbilityEffectType.STUN_FREEZES:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
                    break;
                default:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
                    break;
            }
            
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

        protected override void UnitOnAttackEventHandler(object info)
        {
            base.UnitOnAttackEventHandler(info);
            if (abilityCallType != Enumerators.AbilityCallType.AT_ATTACK)
                return;
            if(info is BoardUnit)
            {
                var creature = info as BoardUnit;
                creature.Stun(Enumerators.StunType.FREEZE, value);
				CreateVFX(creature.transform.position);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.STUN_CREATURE_BY_ABILITY, new object[]
                {
                    abilityUnitOwner,
                    creature
                }));
            }
        }
    }
}