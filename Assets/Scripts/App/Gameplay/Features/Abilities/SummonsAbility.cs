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
    public class SummonsAbility : AbilityBase
    {
        public Enumerators.StatType statType;
        public int value = 1;
        private Server _server;
        private GameObject _boardCreaturePrefab,
                        _fightTargetingArrowPrefab;


        public SummonsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.statType = ability.abilityStatType;
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/fireDamageVFX");
            _boardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/BoardCreature");
            _fightTargetingArrowPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/FightTargetingArrow");
            Debug.Log(_boardCreaturePrefab);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();
            if (abilityCallType != Enumerators.AbilityCallType.TURN_START || !cardCaller.isActivePlayer)
                return;

            if (cardCaller.boardZone.cards.Count >= Constants.MAX_BOARD_CREATURES)
                return;

            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(value);

            string cardSetName = string.Empty;
            foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(libraryCard) > -1)
                    cardSetName = cardSet.name;
            }
            //Get server access

            _server = cardCaller.GetServer();

            //PlayerInfo playerInfo = cardCaller.playerInfo;
            //if (cardCaller == null)
            var playerInfo = cardCaller.playerInfo;

            var card = CreateRuntimeCard(libraryCard, playerInfo);

            var creature = CreateBoardCreature(card, cardSetName);

            // if (!cardCaller.boardZone.cards.Contains(card))
            cardCaller.boardZone.AddCard(card);

            playerInfo.namedZones[Constants.ZONE_BOARD].AddCard(creature.card);

            //Add RuntimeCard to hand on server
            _server.gameState.currentPlayer.namedZones[Constants.ZONE_BOARD].cards.Add(card);
            Debug.Log("SUMMON WOOOOOOOW");
        }

        private BoardCreature CreateBoardCreature(RuntimeCard card, string cardSetName)
        {
            var cardObject = GameObject.Instantiate(_boardCreaturePrefab);
            var board = GameObject.Find("PlayerBoard");
            cardObject.tag = cardCaller is DemoHumanPlayer ? Constants.TAG_PLAYER_OWNED : Constants.TAG_OPPONENT_OWNED;
            cardObject.transform.parent = board.transform;
            cardObject.transform.position = new Vector2(1.9f * cardCaller.playerBoardCardsList.Count, 0);
        //    cardObject.transform.Find("TypeIcon").gameObject.SetActive(false);

            var boardCreature = cardObject.GetComponent<BoardCreature>();
            boardCreature.ownerPlayer = cardCaller;
            boardCreature.PopulateWithInfo(card, cardSetName);
            boardCreature.fightTargetingArrowPrefab = _fightTargetingArrowPrefab;

            cardCaller.playerBoardCardsList.Add(boardCreature);

            var localPlayer = NetworkingUtils.GetHumanLocalPlayer() as DemoHumanPlayer;
            localPlayer.RearrangeBottomBoard();
            localPlayer.RearrangeTopBoard();

            return boardCreature;
        }

        private RuntimeCard CreateRuntimeCard(Data.Card libraryCard, PlayerInfo playerInfo)
        {
            var card = new RuntimeCard();
            card.cardId = value;
            //var player = gameState.players.Find(x => x.netId == netId);
            card.instanceId = playerInfo.currentCardInstanceId++;
            card.ownerPlayer = playerInfo;

            var statCopy = new Stat();
            statCopy.statId = 0;
            statCopy.name = "DMG";
            statCopy.originalValue = libraryCard.damage;
            statCopy.baseValue = libraryCard.damage;
            statCopy.minValue = 0;
            statCopy.maxValue = 99;
           
            var statCopy2 = new Stat();
            statCopy2.statId = 1;
            statCopy2.name = "HP";
            statCopy2.originalValue = libraryCard.health;
            statCopy2.baseValue = libraryCard.health;
            statCopy2.minValue = 0;
            statCopy2.maxValue = 99;

            card.stats[0] = statCopy;
            card.stats[1] = statCopy2;
            card.namedStats["DMG"] = statCopy;
            card.namedStats["HP"] = statCopy2;

            cardCaller.EffectSolver.SetDestroyConditions(card);
            cardCaller.EffectSolver.SetTriggers(card);

            return card;
        }
    }
}