﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrandDevs.CZB.Common;
using UnityEngine;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class AddGooVialsAbility : AbilityBase
    {
        public int value = 1;

        public AddGooVialsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            Debug.Log("Activate");
            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");
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

            if (_isAbilityResolved)
            {

            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            string stat = Constants.TAG_MANA;

            playerCallerOfAbility.playerInfo.namedStats[stat].maxValue = Mathf.Clamp(playerCallerOfAbility.playerInfo.namedStats[stat].maxValue + value, 0, 10);
            playerCallerOfAbility.playerInfo.namedStats[stat].baseValue = playerCallerOfAbility.playerInfo.namedStats[stat].maxValue;
            playerCallerOfAbility.playerInfo.namedStats[stat].PermanentUpdateValue();

            playerCallerOfAbility.GetServer().gameState.currentPlayer.namedStats[stat].maxValue = Mathf.Clamp(playerCallerOfAbility.GetServer().gameState.currentPlayer.namedStats[stat].maxValue + value, 0, 10);
            playerCallerOfAbility.GetServer().gameState.currentPlayer.namedStats[stat].baseValue = playerCallerOfAbility.GetServer().gameState.currentPlayer.namedStats[stat].maxValue;
        }
    }
}