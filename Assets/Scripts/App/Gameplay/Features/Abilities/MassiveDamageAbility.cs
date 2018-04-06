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

            Action();
            //_vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");
            

               // foreach (var card in cardCaller.boardZone.cards)
                 //   cardCaller.FightCreatureBySkill(value, card);
        }

        public override void Update() { }

        public override void Dispose() { }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }
        public override void Action()
        {
            base.Action();

            foreach (var target in abilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                        BoardCreature[] cards = new BoardCreature[cardCaller.opponentBoardCardsList.Count];
                        cardCaller.opponentBoardCardsList.CopyTo(cards);
                        foreach (var cardOpponent in cards)
                        {
                            cardCaller.FightCreatureBySkill(value, cardOpponent.card);
                            CreateVFX(cardOpponent.transform.position);
                        }
                        Array.Clear(cards, 0, cards.Length);
                        cards = null;
                        break;
                    case Enumerators.AbilityTargetType.OPPONENT:
                        cardCaller.FightPlayerBySkill(value);
                        //CreateVFX(targetCreature.transform.position);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        //cardCaller.FightPlayerBySkill(value, false);
                        //CreateVFX(targetCreature.transform.position);
                        break;
                    default: break;
                }
            }
        }
    }
}