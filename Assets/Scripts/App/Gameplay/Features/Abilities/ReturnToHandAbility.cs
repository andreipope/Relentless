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


            Debug.Log("Return To Hand");
            //if (cardCaller.playerInfo.netId == cardCaller.netId)
      
            PlayerInfo playerInfo = GetOwnerOfCreature(targetCreature);

            if (targetCreature.ownerPlayer == null)
                playerInfo = cardCaller.opponentInfo;

            //Get server access
            GetServer();

            //create RuntimeCard
            var card = CreateRuntimeCard(playerInfo);

            //Add RuntimeCard to hand on server
            if (playerInfo == cardCaller.playerInfo)
                _server.gameState.currentPlayer.namedZones[Constants.ZONE_HAND].cards.Add(card);
            else
                _server.gameState.currentOpponent.namedZones[Constants.ZONE_HAND].cards.Add(card);

            //Create Visual process of creating new card at the hand (simulation turn back)
            var netCard = CreateNetCard(card);

            //Put netCard to hand
            cardCaller.CreateAndPutToHandRuntimeCard(netCard, playerInfo);

            //MAYBE use that on future
            /*playerInfo.namedZones[Constants.ZONE_HAND].AddCard(card);
            cardCaller.EffectSolver.SetDestroyConditions(card);
            cardCaller.EffectSolver.SetTriggers(card);*/

            //Remove RuntimeCard on server
            if (playerInfo == cardCaller.playerInfo)
            {
                var boardRuntimeCard = _server.gameState.currentPlayer.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == targetCreature.card.instanceId);
                _server.gameState.currentPlayer.namedZones[Constants.ZONE_BOARD].cards.Remove(boardRuntimeCard);

                if (cardCaller.playerBoardCardsList.Contains(targetCreature))
                    cardCaller.playerBoardCardsList.Remove(targetCreature);
            }
            else
            {
                var boardRuntimeCard = _server.gameState.currentOpponent.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == targetCreature.card.instanceId);
                _server.gameState.currentOpponent.namedZones[Constants.ZONE_BOARD].cards.Remove(boardRuntimeCard);

                if (cardCaller.opponentBoardCardsList.Contains(targetCreature))
                    cardCaller.opponentBoardCardsList.Remove(targetCreature);
            }

            

            //Remove RuntimeCard from hand
            playerInfo.namedZones[Constants.ZONE_BOARD].RemoveCard(targetCreature.card);

            GameObject.Destroy(targetCreature.gameObject);
            CreateVFX(targetCreature.transform.position);


            (NetworkingUtils.GetHumanLocalPlayer() as DemoHumanPlayer).RearrangeBottomBoard();
            (NetworkingUtils.GetHumanLocalPlayer() as DemoHumanPlayer).RearrangeTopBoard();
        }

        private RuntimeCard CreateRuntimeCard(PlayerInfo playerInfo)
        {
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
            return card;
        }

        private NetCard CreateNetCard(RuntimeCard card)
        {
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
            return netCard;
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

        public PlayerInfo GetOwnerOfCreature(BoardCreature creature)
        {
            if (cardCaller.playerBoardCardsList.Contains(creature))
                return cardCaller.playerInfo;

            return cardCaller.opponentInfo;
        }
    }
}