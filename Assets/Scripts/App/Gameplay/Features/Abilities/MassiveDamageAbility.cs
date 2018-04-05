using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class MassiveDamageAbility : AbilityBase
    {
        public int value = 1;

        public MassiveDamageAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            //_vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");
        }

        public override void Update() { }

        public override void Dispose() { }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                foreach(var cardOpponent in cardCaller.opponentBoardZone.cards)
                    cardCaller.FightCreatureBySkill(value, cardOpponent);

                foreach (var card in cardCaller.boardZone.cards)
                    cardCaller.FightCreatureBySkill(value, card);
            }
        }
    }
}