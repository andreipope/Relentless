// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using GrandDevs.CZB;
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;
using GrandDevs.CZB.Common;
using System.Linq;

namespace CCGKit
{
    /// <summary>
    /// This type represents a game player and, as a multiplayer-aware entity, it is derived from
    /// NetworkBehaviour.
    /// </summary>
    public class Player : NetworkBehaviour
    {
        /// <summary>
        /// True if this player is the current active player in the game; false otherwise. 'Active' meaning
        /// the current game turn is his turn.
        /// </summary>
        public bool isActivePlayer;

        /// <summary>
        /// True if this player is controlled by a human; false otherwise (AI).
        /// </summary>
        public bool isHuman;

        /// <summary>
        /// Cached network client.
        /// </summary>
        protected NetworkClient client;

        protected GameState gameState = new GameState();
        protected PlayerInfo playerInfo = new PlayerInfo();
        protected PlayerInfo opponentInfo = new PlayerInfo();

        /// <summary>
        /// True if the game has started; false otherwise.
        /// </summary>
        protected bool gameStarted;

        /// <summary>
        /// Index of this player in the game.
        /// </summary>
        protected int playerIndex;

        /// <summary>
        /// This game's turn duration (in seconds).
        /// </summary>
        protected int turnDuration;

        protected EffectSolver effectSolver;

        public int CurrentTurn;


        public EffectSolver EffectSolver
        {
            get
            {
                return effectSolver;
            }
        }

        

        protected virtual void Awake()
        {
            client = NetworkManager.singleton.client;
        }

        protected virtual void Start()
        {
            //plugin doesn't use id's at all... very strange
            playerInfo.id = 0;
            opponentInfo.id = 1;

            GameClient.Get<IPlayerManager>().playerInfo = playerInfo;
            GameClient.Get<IPlayerManager>().opponentInfo = opponentInfo;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            RegisterWithServer();
            var networkClient = GameObject.Find("DemoNetworkClient").GetComponent<GameNetworkClient>();
            networkClient.AddLocalPlayer(this);

            var gameConfig = GameManager.Instance.config;
            foreach (var stat in gameConfig.playerStats)
            {
                var statCopy = new Stat();
                statCopy.statId = stat.id;
                statCopy.name = stat.name;
                statCopy.originalValue = stat.originalValue;
                statCopy.baseValue = stat.baseValue;
                statCopy.minValue = stat.minValue;
                statCopy.maxValue = stat.maxValue;
                playerInfo.stats[stat.id] = statCopy;
                playerInfo.namedStats[stat.name] = statCopy;
            }
            foreach (var stat in gameConfig.playerStats)
            {
                var statCopy = new Stat();
                statCopy.statId = stat.id;
                statCopy.name = stat.name;
                statCopy.originalValue = stat.originalValue;
                statCopy.baseValue = stat.baseValue;
                statCopy.minValue = stat.minValue;
                statCopy.maxValue = stat.maxValue;
                opponentInfo.stats[stat.id] = statCopy;
                opponentInfo.namedStats[stat.name] = statCopy;
            }

            foreach (var zone in gameConfig.gameZones)
            {
                var zoneCopy = new RuntimeZone();
                zoneCopy.zoneId = zone.id;
                zoneCopy.name = zone.name;
                if (zone.hasMaxSize)
                {
                    zoneCopy.maxCards = zone.maxSize;
                }
                else
                {
                    zoneCopy.maxCards = int.MaxValue;
                }
                playerInfo.zones[zone.id] = zoneCopy;
                playerInfo.namedZones[zone.name] = zoneCopy;
            }

            foreach (var zone in gameConfig.gameZones)
            {
                var zoneCopy = new RuntimeZone();
                zoneCopy.zoneId = zone.id;
                zoneCopy.name = zone.name;
                if (zone.hasMaxSize)
                {
                    zoneCopy.maxCards = zone.maxSize;
                }
                else
                {
                    zoneCopy.maxCards = int.MaxValue;
                }
                opponentInfo.zones[zone.id] = zoneCopy;
                opponentInfo.namedZones[zone.name] = zoneCopy;
            }

            gameState.players.Add(playerInfo);
            gameState.players.Add(opponentInfo);
        }

        protected virtual void RegisterWithServer()
        {
            var msgDefaultDeck = new List<int>();

            //var defaultDeckIndex = isHuman ? PlayerPrefs.GetInt("default_deck") : PlayerPrefs.GetInt("default_ai_deck");

            if (GameManager.Instance.tutorial)
            {
                if (isHuman)
                {
                    msgDefaultDeck.Add(28);
                    msgDefaultDeck.Add(29);
                    msgDefaultDeck.Add(29);
                    msgDefaultDeck.Add(29);
                    msgDefaultDeck.Add(29);
                }
                else
                {
                    msgDefaultDeck.Add(31);
                    msgDefaultDeck.Add(30);
                    msgDefaultDeck.Add(32);
                    msgDefaultDeck.Add(30);
                    msgDefaultDeck.Add(30);
                }
                //int deckId = (GameClient.Get<IUIManager>().GetPage<GameplayPage>() as GameplayPage).CurrentDeckId;
               // int heroId = GameClient.Get<IDataManager>().CachedDecksData.decks[deckId].heroId = 1;
               // int heroId = GameClient.Get<IDataManager>().CachedDecksData.decks[deckId]. = 1;
            }
            else
            {
                if (isHuman)
                {
                    int deckId = (GameClient.Get<IUIManager>().GetPage<GameplayPage>() as GameplayPage).CurrentDeckId;
                    foreach (var card in GameClient.Get<IDataManager>().CachedDecksData.decks[deckId].cards)
                    {
                        for (var i = 0; i < card.amount; i++)
                        {
                            msgDefaultDeck.Add(card.cardId);
                        }
                    }
                }
                else
                {
                    msgDefaultDeck.Add(0);
                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(0);
                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(0);
                    msgDefaultDeck.Add(1);

                    /*

                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(12);
                    msgDefaultDeck.Add(12);
                    msgDefaultDeck.Add(12);
                    msgDefaultDeck.Add(12);
                    msgDefaultDeck.Add(8);
                    msgDefaultDeck.Add(8);
                    msgDefaultDeck.Add(8);
                    msgDefaultDeck.Add(8);
                    msgDefaultDeck.Add(20);
                    msgDefaultDeck.Add(20);
                    msgDefaultDeck.Add(20);
                    msgDefaultDeck.Add(20);
                    msgDefaultDeck.Add(16);
                    msgDefaultDeck.Add(16);
                    msgDefaultDeck.Add(16);
                    msgDefaultDeck.Add(16);
                    msgDefaultDeck.Add(24);
                    msgDefaultDeck.Add(0);
                    msgDefaultDeck.Add(0);
                    msgDefaultDeck.Add(0);
                    msgDefaultDeck.Add(11);
                    msgDefaultDeck.Add(11);
                    msgDefaultDeck.Add(11);
                    msgDefaultDeck.Add(11);
                    msgDefaultDeck.Add(0);
                    msgDefaultDeck.Add(0);
                    */

                }
            }

            // Register the player to the game and send the server his information.
            var msg = new RegisterPlayerMessage();
            msg.netId = netId;
            if (isHuman)
            {
                var playerName = PlayerPrefs.GetString("player_name");
                msg.name = string.IsNullOrEmpty(playerName) ? "Unnamed Wizard" : playerName;
            }
            else
            {
                msg.name = "Turing Machine";
            }
            msg.isHuman = isHuman;
            msg.deck = msgDefaultDeck.ToArray();
            client.Send(NetworkProtocol.RegisterPlayer, msg);
        }

        public virtual void OnStartGame(StartGameMessage msg)
        {
            gameStarted = true;
            playerIndex = msg.playerIndex;
            turnDuration = msg.turnDuration;

            effectSolver = new EffectSolver(gameState, msg.rngSeed);
            effectSolver.SetTriggers(playerInfo);
            effectSolver.SetTriggers(opponentInfo);
            LoadPlayerStates(msg.player, msg.opponent);
        }

        public virtual void OnEndGame(EndGameMessage msg)
        {
        }

        public virtual void OnStartTurn(StartTurnMessage msg)
        {
            if (msg.isRecipientTheActivePlayer)
            {
                isActivePlayer = true;
                CleanupTurnLocalState();

                gameState.currentPlayer = playerInfo;
                gameState.currentOpponent = opponentInfo;
            }
            else
            {
                gameState.currentPlayer = opponentInfo;
                gameState.currentOpponent = playerInfo;
            }

            effectSolver.OnTurnStarted();
            LoadPlayerStates(msg.player, msg.opponent);
            CurrentTurn = msg.turn;
        }

        public void LoadPlayerStates(NetPlayerInfo playerState, NetPlayerInfo opponentState)
        {
            var players = new Dictionary<NetPlayerInfo, PlayerInfo>();
            players.Add(playerState, playerInfo);
            players.Add(opponentState, opponentInfo);
            foreach (var player in players)
            {
                player.Value.netId = player.Key.netId;

                foreach (var stat in player.Key.stats)
                {
                    var playerStat = player.Value.stats[stat.statId];
                    var oldValue = playerStat.effectiveValue;
                    playerStat.originalValue = stat.originalValue;
                    playerStat.baseValue = stat.baseValue;
                    playerStat.minValue = stat.minValue;
                    playerStat.maxValue = stat.maxValue;
                    playerStat.modifiers = new List<Modifier>();
                    foreach (var netModifier in stat.modifiers)
                    {
                        var modifier = new Modifier(netModifier.value, netModifier.duration);
                        playerStat.modifiers.Add(modifier);
                    }
                    if (playerStat.onValueChanged != null)
                    {
                        playerStat.onValueChanged(oldValue, playerStat.effectiveValue);
                    }
                }

                foreach (var zone in player.Key.staticZones)
                {
                    // Remove obsolete entries.
                    var obsoleteCards = new List<RuntimeCard>(player.Value.zones[zone.zoneId].cards.Count);
                    foreach (var card in player.Value.zones[zone.zoneId].cards)
                    {
                        if (System.Array.FindIndex(zone.cards, x => x.instanceId == card.instanceId) == -1)
                        {
                            obsoleteCards.Add(card);
                        }
                    }

                    foreach (var card in obsoleteCards)
                    {
                        player.Value.zones[zone.zoneId].RemoveCard(card);
                    }

                    // Add new entries.
                    foreach (var card in zone.cards)
                    {
                        var runtimeCard = player.Value.zones[zone.zoneId].cards.Find(x => x.instanceId == card.instanceId);

                        if (runtimeCard == null)
                        {
                            runtimeCard = CreateRuntimeCard();
                            runtimeCard.cardId = card.cardId;
                            runtimeCard.instanceId = card.instanceId;
                            runtimeCard.ownerPlayer = player.Value;
                            player.Value.zones[zone.zoneId].AddCard(runtimeCard);
                        }
                    }

                    player.Value.zones[zone.zoneId].numCards = zone.numCards;
                }

                foreach (var zone in player.Key.dynamicZones)
                {
                    // Remove obsolete entries.
                    var obsoleteCards = new List<RuntimeCard>(player.Value.zones[zone.zoneId].cards.Count);
                    foreach (var card in player.Value.zones[zone.zoneId].cards)
                    {
                        if (System.Array.FindIndex(zone.cards, x => x.instanceId == card.instanceId) == -1)
                        {
                            obsoleteCards.Add(card);
                        }
                    }
                    foreach (var card in obsoleteCards)
                    {
                        player.Value.zones[zone.zoneId].RemoveCard(card);
                    }

                    foreach (var card in zone.cards)
                    {
                        var runtimeCard = player.Value.zones[zone.zoneId].cards.Find(x => x.instanceId == card.instanceId);
                        if (runtimeCard != null)
                        {
                            foreach (var stat in card.stats)
                            {
                                runtimeCard.stats[stat.statId].originalValue = stat.originalValue;
                                runtimeCard.stats[stat.statId].baseValue = stat.baseValue;
                                runtimeCard.stats[stat.statId].minValue = stat.minValue;
                                runtimeCard.stats[stat.statId].maxValue = stat.maxValue;
                                runtimeCard.stats[stat.statId].modifiers = new List<Modifier>();
                                foreach (var netModifier in stat.modifiers)
                                {
                                    var modifier = new Modifier(netModifier.value, netModifier.duration);
                                    runtimeCard.stats[stat.statId].modifiers.Add(modifier);
                                }
                            }
                            runtimeCard.type = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId).type;
                            runtimeCard.connectedAbilities.Clear();
                            foreach (var abilityId in card.connectedAbilities)
                            {
                                runtimeCard.ConnectAbility(abilityId);
                            }
                        }
                        else
                        {
                            runtimeCard = CreateRuntimeCard();
                            runtimeCard.cardId = card.cardId;
                            runtimeCard.instanceId = card.instanceId;
                            runtimeCard.ownerPlayer = player.Value;
                            foreach (var stat in card.stats)
                            {
                                var runtimeStat = NetworkingUtils.GetRuntimeStat(stat);
                                runtimeCard.stats[stat.statId] = runtimeStat;

								var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);

                                var statName = "DMG";
                                if (stat.statId == 1)
                                    statName = "HP";
                                runtimeCard.namedStats[statName] = runtimeStat;
                            }
                            runtimeCard.type = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId).type;

                            foreach (var abilityId in card.connectedAbilities)
                            {
                                runtimeCard.ConnectAbility(abilityId);
                            }

                            player.Value.zones[zone.zoneId].AddCard(runtimeCard);
                            effectSolver.SetDestroyConditions(runtimeCard);
                            effectSolver.SetTriggers(runtimeCard);
                        }
                    }
                    player.Value.zones[zone.zoneId].numCards = zone.numCards;
                }
            }
        }

        public virtual void OnEndTurn(EndTurnMessage msg)
        {
            if (msg.isRecipientTheActivePlayer)
            {
                isActivePlayer = false;
                CleanupTurnLocalState();
            }

            effectSolver.OnTurnEnded();

            foreach (var entry in gameState.currentPlayer.stats)
            {
                entry.Value.OnEndTurn();
            }

            foreach (var zone in gameState.currentPlayer.zones)
            {
                foreach (var card in zone.Value.cards)
                {
                    foreach (var stat in card.stats)
                    {
                        stat.Value.OnEndTurn();
                    }
                }
            }
        }

        protected virtual void CleanupTurnLocalState()
        {
        }

        public virtual void StopTurn()
        {
            if (!isLocalPlayer)
                return;

            GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.END_TURN);

            isActivePlayer = false;
            var msg = new StopTurnMessage();
            client.Send(NetworkProtocol.StopTurn, msg);
        }

        public virtual void OnReceiveChatText(NetworkInstanceId senderNetId, string text)
        {
        }

        protected virtual RuntimeCard CreateRuntimeCard()
        {
            return new RuntimeCard();
        }

        public virtual void OnCardMoved(CardMovedMessage msg)
        {
            var runtimeCard = CreateRuntimeCard();
            runtimeCard.cardId = msg.card.cardId;
            runtimeCard.instanceId = msg.card.instanceId;
            runtimeCard.ownerPlayer = playerInfo.netId == msg.playerNetId ? playerInfo : opponentInfo;
            //runtimeCard.abilities = AbilitiesController.AbilityUintArrayTypeToList(msg.card.abilities);
            runtimeCard.connectedAbilities = msg.card.connectedAbilities.ToList();

            foreach (var stat in msg.card.stats)
            {
                var runtimeStat = NetworkingUtils.GetRuntimeStat(stat);
                runtimeCard.stats[stat.statId] = runtimeStat;
                var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(msg.card.cardId);
                var statName = "DMG";
                if (stat.statId == 1)
                    statName = "HP";
                runtimeCard.namedStats[statName] = runtimeStat;
            }
            opponentInfo.zones[msg.originZoneId].RemoveCard(runtimeCard);
            opponentInfo.zones[msg.destinationZoneId].AddCard(runtimeCard);
        }

        public virtual void OnPlayerAttacked(PlayerAttackedMessage msg)
        {
        }

        public virtual void OnCreatureAttacked(CreatureAttackedMessage msg)
        {
        }

        public void CreateActiveAbility(int zoneId, int cardInstanceId, int abilityIndex)
        {
            var card = playerInfo.zones[zoneId].cards.Find(x => x.instanceId == cardInstanceId);
            if (card != null)
            {
                var libraryCard = GameManager.Instance.config.GetCard(card.cardId);
                var activatedAbilities = libraryCard.abilities.FindAll(x => x is ActivatedAbility);
                if (activatedAbilities.Count > 0 && abilityIndex < activatedAbilities.Count)
                {
                    var activatedAbility = activatedAbilities[abilityIndex] as ActivatedAbility;
                    var cost = activatedAbility.costs[0];
                    if (cost != null)
                    {
                        var payResourceCost = cost as PayResourceCost;
                        var statCost = payResourceCost.value;
                        if (playerInfo.stats[payResourceCost.statId].effectiveValue >= statCost)
                        {
                            playerInfo.stats[payResourceCost.statId].baseValue -= statCost;
                            effectSolver.CreateActiveAbility(playerInfo, card, 0);
                            var msg = new CreateActiveAbilityMessage();
                            msg.playerNetId = playerInfo.netId;
                            msg.zoneId = zoneId;
                            msg.cardInstanceId = cardInstanceId;
                            msg.abilityIndex = abilityIndex;
                            client.Send(NetworkProtocol.CreateActiveAbility, msg);
                        }
                    }
                }
            }
        }

        public virtual void OnCreateActiveAbility(CreateActiveAbilityMessage msg)
        {
            var card = opponentInfo.zones[msg.zoneId].cards.Find(x => x.instanceId == msg.cardInstanceId);
            if (card != null)
            {
                var libraryCard = GameManager.Instance.config.GetCard(card.cardId);
                var cost = libraryCard.costs.Find(x => x is PayResourceCost);
                if (cost != null)
                {
                    var payResourceCost = cost as PayResourceCost;
                    var statCost = payResourceCost.value;
                    if (opponentInfo.stats[payResourceCost.statId].effectiveValue >= statCost)
                    {
                        opponentInfo.stats[payResourceCost.statId].baseValue -= statCost;
                        effectSolver.CreateActiveAbility(opponentInfo, card, 0);
                    }
                }
            }
        }

        public void PlayCreatureCard(RuntimeCard card, List<int> targetInfo = null)
        {
            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);
            playerInfo.namedStats["Mana"].baseValue -= libraryCard.cost;

            var msg = new MoveCardMessage();
            msg.playerNetId = netId;
            msg.cardInstanceId = card.instanceId;
            msg.originZoneId = playerInfo.namedZones["Hand"].zoneId;
            msg.destinationZoneId = playerInfo.namedZones["Board"].zoneId;
            if (targetInfo != null)
            {
                msg.targetInfo = targetInfo.ToArray();
            }
            client.Send(NetworkProtocol.MoveCard, msg);
        }

        public void PlaySpellCard(RuntimeCard card, List<int> targetInfo = null)
        {
            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);
            playerInfo.namedStats["Mana"].baseValue -= libraryCard.cost;

            var msg = new MoveCardMessage();
            msg.playerNetId = netId;
            msg.cardInstanceId = card.instanceId;
            msg.originZoneId = playerInfo.namedZones["Hand"].zoneId;
            msg.destinationZoneId = playerInfo.namedZones["Board"].zoneId;
            if (targetInfo != null)
            {
                msg.targetInfo = targetInfo.ToArray();
            }
            client.Send(NetworkProtocol.MoveCard, msg);
        }

        public void FightPlayer(int cardInstanceId)
        {
            GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_HERO);
            effectSolver.FightPlayer(netId, cardInstanceId);
            Debug.Log("FightPlayer");
            var msg = new FightPlayerMessage();
            msg.attackingPlayerNetId = netId;
            msg.cardInstanceId = cardInstanceId;
            client.Send(NetworkProtocol.FightPlayer, msg);
        }


		public void FightPlayerBySkill(int attack, bool isOpponent = true)
		{
            effectSolver.FightPlayerBySkill(netId, attack, isOpponent);

            var msg = new FightPlayerBySkillMessage();
			msg.attackingPlayerNetId = netId;
			msg.attack = attack;
			msg.isOpponent = isOpponent;
            client.Send(NetworkProtocol.FightPlayerBySkill, msg);
		}


        public void HealPlayerBySkill(int value, bool isOpponent = true)
        {
            effectSolver.HealPlayerBySkill(netId, value, isOpponent);

            var msg = new HealPlayerBySkillMessage();
            msg.callerPlayerNetId = netId;
            msg.value = value;
            msg.isOpponent = isOpponent;
            client.Send(NetworkProtocol.HealPlayerBySkill, msg);
        }

        public void FightCreature(RuntimeCard attackingCard, RuntimeCard attackedCard)
        {
            GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_CARD);
            effectSolver.FightCreature(netId, attackingCard, attackedCard);

            var msg = new FightCreatureMessage();
            msg.attackingPlayerNetId = netId;
            msg.attackingCardInstanceId = attackingCard.instanceId;
            msg.attackedCardInstanceId = attackedCard.instanceId;
            client.Send(NetworkProtocol.FightCreature, msg);
        }
		public void FightCreatureBySkill(int attack, RuntimeCard attackedCard)
		{
            effectSolver.FightCreatureBySkill(netId, attackedCard, attack);

            var msg = new FightCreatureBySkillMessage();
            msg.attackingPlayerNetId = netId;
            msg.attackedCardInstanceId = attackedCard.instanceId;
            msg.attack = attack;
            client.Send(NetworkProtocol.FightCreatureBySkill, msg);
        }

        public void HealCreatureBySkill(int value, RuntimeCard card)
        {
            effectSolver.HealCreatureBySkill(netId, card, value);

            var msg = new HealCreatureBySkillMessage();
            msg.callerPlayerNetId = netId;
            msg.cardInstanceId = card.instanceId;
            msg.value = value;
            client.Send(NetworkProtocol.HealCreatureBySkill, msg);
        }
    }
}
