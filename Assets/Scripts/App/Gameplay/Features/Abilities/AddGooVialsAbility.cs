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


            cardCaller.playerInfo.namedStats[Constants.TAG_MANA].maxValue += value;
            cardCaller.playerInfo.namedStats[Constants.TAG_MANA].baseValue += value;

            cardCaller.playerInfo.namedStats[Constants.TAG_MANA].PermanentUpdateValue();


            /*
            cardCaller.manaStat.maxValue += value;
            cardCaller.manaStat.baseValue += value;

            cardCaller.manaStat.PermanentUpdateValue(); */

            cardCaller.GetServer().gameState.currentPlayer.namedStats[Constants.TAG_MANA].maxValue += value;
            cardCaller.GetServer().gameState.currentPlayer.namedStats[Constants.TAG_MANA].baseValue += value; 
        }
    }
}