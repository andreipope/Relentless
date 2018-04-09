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
        private GameObject _boardCreaturePrefab;


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
            Debug.Log(_boardCreaturePrefab);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();
            if (abilityCallType != Enumerators.AbilityCallType.TURN_START)
                return;

            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(value);
            string cardSetName = string.Empty;
            foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(libraryCard) > -1)
                    cardSetName = cardSet.name;
            }
            Debug.Log(_boardCreaturePrefab);

            var boardCreature = GameObject.Instantiate(_boardCreaturePrefab);
            var board = GameObject.Find("PlayerBoard");
            //boardCreature.tag = "PlayerOwned";


            var card = new RuntimeCard();
            card.cardId = value;
            //var player = gameState.players.Find(x => x.netId == netId);
            //card.instanceId = player.currentCardInstanceId++;
            //card.ownerPlayer = player;

            var statCopy = new Stat();
            statCopy.statId = 0;
            statCopy.name = "DMG";
            statCopy.originalValue = libraryCard.damage;
            statCopy.baseValue = libraryCard.damage;
            statCopy.minValue = 0;
            statCopy.maxValue = 99;
            card.stats[0] = statCopy;
            card.namedStats["DMG"] = statCopy;

            statCopy = new Stat();
            statCopy.statId = 1;
            statCopy.name = "HP";
            statCopy.originalValue = libraryCard.health;
            statCopy.baseValue = libraryCard.health;
            statCopy.minValue = 0;
            statCopy.maxValue = 99;
            card.stats[1] = statCopy;
            card.namedStats["HP"] = statCopy;

            boardCreature.transform.parent = board.transform;
            boardCreature.transform.position = new Vector2(1.9f * cardCaller.boardZone.cards.Count, 0);
            boardCreature.GetComponent<BoardCreature>().ownerPlayer = cardCaller;
            boardCreature.GetComponent<BoardCreature>().PopulateWithInfo(card, cardSetName);

            cardCaller.playerBoardCardsList.Add(boardCreature.GetComponent<BoardCreature>());
            cardCaller.playerInfo.namedZones[Constants.ZONE_BOARD].AddCard(card);
        }
    }
}