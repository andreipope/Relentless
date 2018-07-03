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
    public class StunOrDamageAdjustmentsAbility : AbilityBase
    {
        public Enumerators.StatType statType;
        public int value = 1;


        public StunOrDamageAdjustmentsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.statType = ability.abilityStatType;
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (abilityEffectType)
            {
                case Enumerators.AbilityEffectType.STUN_OR_DAMAGE_FREEZES:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
                    break;
                default:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/fireDamageVFX");
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

            if (_isAbilityResolved)
            {
                Action(targetCreature);
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            var creature = info as BoardUnit;

            CreateVFX(creature.transform.position);

            BoardUnit leftAdjustment = null,
                    rightAdjastment = null;

            int targetIndex = -1;
            for (int i = 0; i < creature.ownerPlayer.BoardCards.Count; i++)
            {
                if (creature.ownerPlayer.BoardCards[i] == creature)
                    targetIndex = i;
            }
            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                    leftAdjustment = creature.ownerPlayer.BoardCards[targetIndex - 1];
                if (targetIndex + 1 < creature.ownerPlayer.BoardCards.Count)
                    rightAdjastment = creature.ownerPlayer.BoardCards[targetIndex + 1];
            }

            if (leftAdjustment != null)
            {
                if (leftAdjustment.IsStun)
                    _battleController.AttackCreatureByAbility(playerCallerOfAbility, abilityData, leftAdjustment);
                else
                    leftAdjustment.Stun(1);
                //CreateVFX(leftAdjustment..transform.position);
            }

            if (rightAdjastment != null)
            {
                if (rightAdjastment.IsStun)
                    _battleController.AttackCreatureByAbility(playerCallerOfAbility, abilityData, rightAdjastment);
                else
                    rightAdjastment.Stun(1);
                //CreateVFX(targetCreature.transform.position);
            }

            if (creature.IsStun)
                _battleController.AttackCreatureByAbility(playerCallerOfAbility, abilityData, creature);
            else
                creature.Stun(1);
        }

        protected override void CreatureOnAttackEventHandler(object info)
        {
            base.CreatureOnAttackEventHandler(info);
        }
    }
}