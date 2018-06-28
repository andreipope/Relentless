﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrandDevs.CZB.Common;
using UnityEngine;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class SummonsAbility : AbilityBase
    {
        public Enumerators.StatType statType;
        public int value = 1;
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
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();
          //  if (abilityCallType != Enumerators.AbilityCallType.TURN_START || !playerCallerOfAbility.isActivePlayer)
          //      return;

           // if (playerCallerOfAbility.boardZone.cards.Count >= Constants.MAX_BOARD_CREATURES)
           //     return;

            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(value);

            string cardSetName = string.Empty;
            foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(libraryCard) > -1)
                    cardSetName = cardSet.name;
            }
            //Get server access

            //PlayerInfo playerInfo = cardCaller.playerInfo;
            //if (cardCaller == null)
            //var playerInfo = playerCallerOfAbility.playerInfo;

            //var card = CreateRuntimeCard(libraryCard, playerInfo);

            //var creature = CreateBoardCreature(card, cardSetName);

            //// if (!cardCaller.boardZone.cards.Contains(card))
            //playerCallerOfAbility.boardZone.AddCard(card);

            //playerInfo.namedZones[Constants.ZONE_BOARD].AddCard(creature.card);

            ////Add RuntimeCard to hand on server
            //_server.gameState.currentPlayer.namedZones[Constants.ZONE_BOARD].cards.Add(card);
        }

        //private BoardCreature CreateBoardCreature(RuntimeCard card, string cardSetName)
        //{
        //    var cardObject = GameObject.Instantiate(_boardCreaturePrefab);

        //    GameObject board = playerCallerOfAbility is DemoHumanPlayer ? GameObject.Find("PlayerBoard") : GameObject.Find("OpponentBoard");

        //    cardObject.tag = playerCallerOfAbility is DemoHumanPlayer ? Constants.TAG_PLAYER_OWNED : Constants.TAG_OPPONENT_OWNED;
        //    cardObject.transform.parent = board.transform;
        //    cardObject.transform.position = new Vector2(1.9f * playerCallerOfAbility.playerBoardCardsList.Count, 0);
        //    //    cardObject.transform.Find("TypeIcon").gameObject.SetActive(false);

        //    var boardCreature = cardObject.GetComponent<BoardCreature>();
        //    boardCreature.ownerPlayer = playerCallerOfAbility;
        //    boardCreature.PopulateWithInfo(card, cardSetName);
        //    boardCreature.PlayArrivalAnimation();
        //    boardCreature._fightTargetingArrowPrefab = _fightTargetingArrowPrefab;

        //    playerCallerOfAbility.playerBoardCardsList.Add(boardCreature);

        //    var localPlayer = NetworkingUtils.GetHumanLocalPlayer() as DemoHumanPlayer;
        //    localPlayer.RearrangeBottomBoard();

        //    if (!(playerCallerOfAbility is DemoHumanPlayer))
        //    {
        //        localPlayer.RearrangeTopBoard();
          
        //        //localPlayer.opponentBoardZone.AddCard(card);// boardCreature.GetComponent<BoardCreature>());
        //        //localPlayer.opponentBoardCardsList.Add(boardCreature.GetComponent<BoardCreature>());
        //        //localPlayer.GetServer().gameState.currentOpponent.namedZones[Constants.ZONE_BOARD].AddCard(card);
        //    }

        //    return boardCreature;
        //}

        //private RuntimeCard CreateRuntimeCard(Data.Card libraryCard, PlayerInfo playerInfo)
        //{
        //    var card = new RuntimeCard();
        //    card.cardId = value;
        //    //var player = gameState.players.Find(x => x.netId == netId);
        //    card.instanceId = playerInfo.currentCardInstanceId++;
        //    card.ownerPlayer = playerInfo;

        //    var statCopy = new Stat();
        //    statCopy.statId = 0;
        //    statCopy.name = Constants.STAT_DAMAGE;
        //    statCopy.originalValue = libraryCard.damage;
        //    statCopy.baseValue = libraryCard.damage;
        //    statCopy.minValue = 0;
        //    statCopy.maxValue = 99;
           
        //    var statCopy2 = new Stat();
        //    statCopy2.statId = 1;
        //    statCopy2.name = Constants.STAT_HP;
        //    statCopy2.originalValue = libraryCard.health;
        //    statCopy2.baseValue = libraryCard.health;
        //    statCopy2.minValue = 0;
        //    statCopy2.maxValue = 99;

        //    card.stats[0] = statCopy;
        //    card.stats[1] = statCopy2;
        //    card.namedStats[Constants.STAT_DAMAGE] = statCopy;
        //    card.namedStats[Constants.STAT_HP] = statCopy2;

        //    playerCallerOfAbility.EffectSolver.SetDestroyConditions(card);
        //    playerCallerOfAbility.EffectSolver.SetTriggers(card);

        //    return card;
        //}
    }
}