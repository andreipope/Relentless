// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



﻿using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class ReturnToHandAbility : AbilityBase
    {
        public int value = 1;

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

            //Player creatureOwner = targetCreature.ownerPlayer;
            ////RuntimeCard returningCard = targetCreature.Card;
            //Vector3 creaturePosition = targetCreature.transform.position;

            //// Debug.LogError("<color=white>------return card of " + creatureOwner.GetType() + "; human " + creatureOwner.isHuman + "; to hand-------</color>");
            //// Debug.LogError("<color=white>------returning card " + returningCard.instanceId + " to hand-------</color>");

            //// STEP 1 - REMOVE CREATURE FROM BOARD
            //if (creatureOwner.BoardCards.Contains(targetCreature)) // hack
            //    creatureOwner.BoardCards.Remove(targetCreature);

            //// STEP 2 - DESTROY CREATURE ON THE BOARD OR ANIMATE
            //CreateVFX(creaturePosition);
            //MonoBehaviour.Destroy(targetCreature.gameObject);

            // STEP 3 - REMOVE RUNTIME CARD FROM BOARD
            //creatureOwner.playerInfo.namedZones[Constants.ZONE_BOARD].RemoveCard(returningCard);
            //creatureOwner.boardZone.RemoveCard(returningCard);

            //var serverCurrentPlayer = creatureOwner.Equals(playerCallerOfAbility) ? creatureOwner.GetServer().gameState.currentPlayer : creatureOwner.GetServer().gameState.currentOpponent;

            // STEP 4 - REMOVE CARD FROM SERVER BOARD
            //var boardRuntimeCard = serverCurrentPlayer.namedZones[Constants.ZONE_BOARD].cards.Find(x => x.instanceId == returningCard.instanceId);
            //serverCurrentPlayer.namedZones[Constants.ZONE_BOARD].cards.Remove(boardRuntimeCard);

            // STEP 5 - CREATE AND ADD TO SERVER NEW RUNTIME CARD FOR HAND
            //var card = CreateRuntimeCard(creatureOwner.playerInfo, returningCard.instanceId);
            //serverCurrentPlayer.namedZones[Constants.ZONE_HAND].cards.Add(card);

            //// STEP 6 - CREATE NET CARD AND SIMULATE ANIMATION OF RETURNING CARD TO HAND
            //var netCard = CreateNetCard(card);
            //creatureOwner.ReturnToHandRuntimeCard(netCard, creatureOwner.playerInfo, creaturePosition);

            ////STEP 7 - REARRANGE CREATURES ON THE BOARD
            //GameClient.Get<IGameplayManager>().RearrangeHands();
        }

        //private RuntimeCard CreateRuntimeCard(PlayerInfo playerInfo, int instanceId)
        //{
        //    var card = new RuntimeCard();
        //    card.cardId = targetCreature.card.cardId;
        //    card.instanceId = instanceId;// playerInfo.currentCardInstanceId++;
        //    card.ownerPlayer = playerInfo;
        //    card.stats[0] = targetCreature.card.stats[0];
        //    card.stats[1] = targetCreature.card.stats[1];
        //    card.namedStats[Constants.STAT_DAMAGE] = targetCreature.card.namedStats[Constants.STAT_DAMAGE].Clone();
        //    card.namedStats[Constants.STAT_HP] = targetCreature.card.namedStats[Constants.STAT_HP].Clone();
        //    card.namedStats[Constants.STAT_DAMAGE].baseValue = card.namedStats[Constants.STAT_DAMAGE].originalValue;
        //    card.namedStats[Constants.STAT_HP].baseValue = card.namedStats[Constants.STAT_HP].originalValue;
        //    return card;
        //}

        //private NetCard CreateNetCard(RuntimeCard card)
        //{
        //    var netCard = new NetCard();
        //    netCard.cardId = card.cardId;
        //    netCard.instanceId = card.instanceId;
        //    netCard.stats = new NetStat[card.stats.Count];

        //    var idx = 0;

        //    foreach (var entry in card.stats)
        //        netCard.stats[idx++] = NetworkingUtils.GetNetStat(entry.Value);

        //    netCard.keywords = new NetKeyword[card.keywords.Count];

        //    idx = 0;

        //    foreach (var entry in card.keywords)
        //        netCard.keywords[idx++] = NetworkingUtils.GetNetKeyword(entry);

        //    netCard.connectedAbilities = card.connectedAbilities.ToArray();

        //    return netCard;
        //}

        //private Player GetOwnerOfCreature(BoardCreature creature)
        //{
        //    if (playerCallerOfAbility.playerBoardCardsList.Contains(creature))
        //        return playerCallerOfAbility;
        //    else
        //    {
        //        if (playerCallerOfAbility is DemoHumanPlayer)
        //            return DemoAIPlayer.Instance;
        //        else
        //            return NetworkingUtils.GetHumanLocalPlayer();
        //    }
        //}
    }
}