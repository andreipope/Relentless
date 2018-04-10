﻿using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class ReturnToHandAbility : AbilityBase
    {
        public int value = 1;

        private Server _server;

        public ReturnToHandAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");
        }

        public override void Update() { }

        public override void Dispose() { }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                Action();
            }
        }
        public override void Action(object info = null)
        {
            base.Action(info);

            //if (cardCaller.playerInfo.netId == cardCaller.netId)
            //{
            

            // targetCreature.ownerPlayer

            PlayerInfo playerInfo = cardCaller.playerInfo;
            if (targetCreature.ownerPlayer == null)
                playerInfo = cardCaller.opponentInfo;

            Debug.Log(targetCreature.ownerPlayer);
            Debug.Log(playerInfo.isHuman);
            Debug.Log(playerInfo.nickname);
            Debug.Log(playerInfo.id);


            var card = new RuntimeCard();
            card.cardId = targetCreature.card.cardId;
            card.instanceId = playerInfo.currentCardInstanceId++;
            card.ownerPlayer = playerInfo;
            card.stats[0] = targetCreature.card.stats[0];
            card.stats[1] = targetCreature.card.stats[1];
            card.namedStats["DMG"] = targetCreature.card.namedStats["DMG"];
            card.namedStats["HP"] = targetCreature.card.namedStats["HP"];
            
            card.namedStats["DMG"].baseValue = card.namedStats["DMG"].originalValue;
            card.namedStats["HP"].baseValue = card.namedStats["HP"].originalValue;

            //cardCaller.EffectSolver.MoveCard(card.ownerPlayer.netId, card, "Board", "Graveyard");
            //cardCaller.playerInfo.namedZones["Board"].RemoveCard(card);
            //MoveCard(card.ownerPlayer.netId, card, "Board", "Graveyard");

            var netCard = new NetCard();
            netCard.cardId = card.cardId;
            netCard.instanceId = card.instanceId;
            netCard.stats = new NetStat[card.stats.Count];
            var idx = 0;
            foreach (var entry in card.stats)
            {
                netCard.stats[idx++] = NetworkingUtils.GetNetStat(entry.Value);
            }
            netCard.keywords = new NetKeyword[card.keywords.Count];
            idx = 0;
            foreach (var entry in card.keywords)
            {
                netCard.keywords[idx++] = NetworkingUtils.GetNetKeyword(entry);
            }
            netCard.connectedAbilities = card.connectedAbilities.ToArray();
            
            cardCaller.CreateAndPutToHandRuntimeCard(netCard, playerInfo);
            
            //Debug.Log(playerInfo.namedZones[Constants.ZONE_BOARD].cards.Count);
            //TODO CHEEEEECK!!!!
                playerInfo.namedZones[Constants.ZONE_BOARD].RemoveCard(targetCreature.card);
            //Debug.Log(playerInfo.namedZones[Constants.ZONE_BOARD].cards.Count);

            //GameObject.Destroy(targetCreature.gameObject);        */
           /* }
            else
            {
                cardCaller.playerInfo.namedZones[Constants.ZONE_BOARD].RemoveCard(targetCreature.card);
                GameObject.Destroy(targetCreature.gameObject);

                Debug.Log("return to Opponent hand");
            }*/

            CreateVFX(targetCreature.transform.position);
        }

        private void GetServer()
        {
            if (_server == null)
            {
                var server = GameObject.Find("Server");
                if (server != null)
                {
                    _server = server.GetComponent<Server>();
                }
            }
        }
    }
}