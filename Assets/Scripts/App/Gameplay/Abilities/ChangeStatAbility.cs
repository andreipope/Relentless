// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class ChangeStatAbility : AbilityBase
    {
        public Enumerators.SetType setType;
        public Enumerators.StatType statType;
        public int value = 1;


        public ChangeStatAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
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
            if (_isAbilityResolved)
            {
            }
        }

        protected override void CreatureOnAttackEventHandler(object info)
        {
            base.CreatureOnAttackEventHandler(info);
            if (abilityCallType != Enumerators.AbilityCallType.AT_ATTACK)
                return;
            
			string statName = statType == Enumerators.StatType.HEALTH ? "HP" : "DMG";

            switch (statType)
            {
                case Enumerators.StatType.HEALTH:
                    boardCreature.HP += value;
                    break;
                case Enumerators.StatType.DAMAGE:
                    boardCreature.Damage += value;
                    break;
                default:
                    break;
            }
             //var newValue = boardCreature.card.namedStats[statName].baseValue + value;
             //if (newValue < 0)
             //	newValue = 0;

            //var netCard = _server.gameState.currentPlayer.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == boardCreature.card.instanceId);

            //boardCreature.card.namedStats[statName].baseValue = newValue;

            try
            {
              //  netCard.namedStats[statName].baseValue = newValue;
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
            }

			//CreateVFX(boardCreature.transform.position);
        }
    }
}