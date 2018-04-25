﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class StunOrDamageAdjustmentsAbility : AbilityBase
    {
        public Enumerators.StatType statType;
        public int value = 1;
        private Server _server;


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

            var creature = info as BoardCreature;

            CreateVFX(creature.transform.position);

            BoardCreature leftAdjustment = null,
                    rightAdjastment = null;

            int targetIndex = -1;
            for (int i = 0; i < cardCaller.opponentBoardCardsList.Count; i++)
            {
                if (cardCaller.opponentBoardCardsList[i] == creature)
                    targetIndex = i;
            }
            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                    leftAdjustment = cardCaller.opponentBoardCardsList[targetIndex - 1];
                if (targetIndex + 1 < cardCaller.opponentBoardCardsList.Count)
                    rightAdjastment = cardCaller.opponentBoardCardsList[targetIndex + 1];
            }

            if (leftAdjustment != null)
            {
                if (leftAdjustment.IsStun)
                    cardCaller.FightCreatureBySkill(value, leftAdjustment.card);
                else
                    leftAdjustment.Stun(1);
                //CreateVFX(leftAdjustment..transform.position);
            }

            if (rightAdjastment != null)
            {
                if (rightAdjastment.IsStun)
                    cardCaller.FightCreatureBySkill(value, rightAdjastment.card);
                else
                    rightAdjastment.Stun(1);
                //CreateVFX(targetCreature.transform.position);
            }

            if (creature.IsStun)
                cardCaller.FightCreatureBySkill(value, creature.card);
            else
                creature.Stun(1);
        }

        protected override void CreatureOnAttackEventHandler(object info)
        {
            base.CreatureOnAttackEventHandler(info);
        }
    }
}