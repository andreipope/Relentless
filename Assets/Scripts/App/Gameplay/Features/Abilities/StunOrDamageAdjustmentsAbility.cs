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

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/fireDamageVFX");
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

        protected override void CreatureOnAttackEventHandler(object info)
        {
            base.CreatureOnAttackEventHandler(info);
            if (abilityCallType != Enumerators.AbilityCallType.AT_ATTACK)
                return;

			if (info is BoardCreature)
			{
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
						cardCaller.FightCreatureBySkill(3, leftAdjustment.card);
					else
                        leftAdjustment.Stun(value);
					//CreateVFX(leftAdjustment..transform.position);
				}

				if (rightAdjastment != null)
				{
					if (rightAdjastment.IsStun)
						cardCaller.FightCreatureBySkill(3, rightAdjastment.card);
					else
                        rightAdjastment.Stun(value);
					//CreateVFX(targetCreature.transform.position);
				}

				if (creature.IsStun)
					cardCaller.FightCreatureBySkill(3, creature.card);
				else
					creature.Stun(value);
			}
        }
    }
}