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
        public event Action OnEndTurnEvent;
        public event Action OnStartTurnEvent;

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
        public PlayerInfo playerInfo = new PlayerInfo();
        public PlayerInfo opponentInfo = new PlayerInfo();

        /// <summary>
        /// True if the game has started; false otherwise.
        /// </summary>
        public bool gameStarted;

        public bool gameEnded;


        /// <summary>
        /// Index of this player in the game.
        /// </summary>
        public int playerIndex;

        /// <summary>
        /// This game's turn duration (in seconds).
        /// </summary>
        public int turnDuration;

        protected EffectSolver effectSolver;

        public int CurrentTurn;

        private AbilitiesController abilitiesController;
        private Server _server;


        public EffectSolver EffectSolver
        {
            get
            {
                return effectSolver;
            }
            set
            {
                effectSolver = value;
            }
        }


        public BoardWeapon CurrentBoardWeapon { get; protected set; }
        public bool AlreadyAttackedInThisTurn { get; set; }
        public bool isPlayerStunned { get; set; }

        public virtual List<BoardCreature> opponentBoardCardsList { get; set; }
        public virtual List<BoardCreature> playerBoardCardsList { get; set;  }

        public RuntimeZone deckZone;
        public RuntimeZone handZone;
        public RuntimeZone boardZone;
        public RuntimeZone graveyardZone;

        public RuntimeZone opponentDeckZone;
        public RuntimeZone opponentHandZone;
        public RuntimeZone opponentBoardZone;
        public RuntimeZone opponentGraveyardZone;

        public int deckId;

        public BoardSkill boardSkill { get; protected set; }

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
            abilitiesController = GameClient.Get<IGameplayManager>().GetController<AbilitiesController>();
        }


        public void CallOnEndTurnEvent()
        {
            OnEndTurnEvent?.Invoke();
        }

        public void CallOnStartTurnEvent()
        {
            OnStartTurnEvent?.Invoke();
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
                    msgDefaultDeck.Add(18);
                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(16);
                }
                else
                {
                    msgDefaultDeck.Add(8);
                    msgDefaultDeck.Add(5);
                    msgDefaultDeck.Add(9);
                    msgDefaultDeck.Add(8);
                    msgDefaultDeck.Add(8); 
                }
                //int deckId = (GameClient.Get<IUIManager>().GetPage<GameplayPage>() as GameplayPage).CurrentDeckId;
               // int heroId = GameClient.Get<IDataManager>().CachedDecksData.decks[deckId].heroId = 1;
               // int heroId = GameClient.Get<IDataManager>().CachedDecksData.decks[deckId]. = 1;
            }
            else
            {
                if (isHuman)
                {
                    deckId = (GameClient.Get<IUIManager>().GetPage<GameplayPage>() as GameplayPage).CurrentDeckId;
                    foreach (var card in GameClient.Get<IDataManager>().CachedDecksData.decks[deckId].cards)
                    {
                        for (var i = 0; i < card.amount; i++)
                        {
                            if (Constants.DEV_MODE)
                            {
                                card.cardId = 1;
                            }
                            msgDefaultDeck.Add(card.cardId);
                        }
                    }
                }
                else
                {
                    deckId = UnityEngine.Random.Range(0, GameClient.Get<IDataManager>().CachedOpponentDecksData.decks.Count);
                    foreach (var card in GameClient.Get<IDataManager>().CachedOpponentDecksData.decks[deckId].cards)
                    {
                        for (var i = 0; i < card.amount; i++)
                        {
                            if (Constants.DEV_MODE)
                            {
                                //card.cardId = 2;
                            }
                            msgDefaultDeck.Add(card.cardId);
                        }
                    }
                    var deck = GameClient.Get<IDataManager>().CachedOpponentDecksData.decks[deckId];
                    GameClient.Get<IGameplayManager>().OpponentHeroId = deck.heroId;
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
            gameEnded = true;
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
            EffectSolver.OnTurnStarted();
            LoadPlayerStates(msg.player, msg.opponent, msg.isRecipientTheActivePlayer);
            CurrentTurn = msg.turn;
        }

        public Server GetServer()
        {
            if (_server == null)
            {
                var server = GameObject.Find("Server");

                if (server)
                    _server = server.GetComponent<Server>();
            }
            return _server;
        }

        public void ModificateStatMaxValue(string stat, int value, int max)
        {
            var server = GetServer();

            playerInfo.namedStats[stat].maxValue = Mathf.Clamp(playerInfo.namedStats[stat].maxValue + value, 0, max);
            playerInfo.namedStats[stat].baseValue = playerInfo.namedStats[stat].maxValue;
            playerInfo.namedStats[stat].PermanentUpdateValue();

           // server.gameState.currentPlayer.namedStats[stat].maxValue += value;

            server.gameState.currentPlayer.namedStats[stat].maxValue = Mathf.Clamp(server.gameState.currentPlayer.namedStats[stat].maxValue + value, 0, max);
            server.gameState.currentPlayer.namedStats[stat].baseValue = server.gameState.currentPlayer.namedStats[stat].maxValue;


            if (!(this is DemoHumanPlayer))
            {
              //  Debug.LogError(value + " value " + stat);

                var humanPlayer = NetworkingUtils.GetHumanLocalPlayer();

                humanPlayer.opponentInfo.namedStats[stat].maxValue = playerInfo.namedStats[stat].maxValue;
                humanPlayer.opponentInfo.namedStats[stat].baseValue = playerInfo.namedStats[stat].maxValue;
                humanPlayer.opponentInfo.namedStats[stat].PermanentUpdateValue();
            }
        }
    
        public void LoadPlayerStates(NetPlayerInfo playerState, NetPlayerInfo opponentState, bool isNewTurn = false)
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

                if (GameManager.Instance.tutorial)
                    ModificateStatMaxValue(Constants.TAG_MANA, 8, 10);
                else if (playerState.id == player.Key.id && isNewTurn)
                {
                    ModificateStatMaxValue(Constants.TAG_MANA, 1, 10);

                  //  Debug.LogError("playerState " + playerState.id);
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
                            runtimeCard.type = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId).cardType;
                            runtimeCard.connectedAbilities.Clear();
                            foreach (var abilityId in card.connectedAbilities)
                            {
                                runtimeCard.ConnectAbility(abilityId);
                            }
                        }
                        else
                        {
                            if (zone.zoneId == 1)
                            {
                                CreateAndPutToHandRuntimeCard(card, player.Value);
                            }
                        }
                    }
                    player.Value.zones[zone.zoneId].numCards = zone.numCards;
                }
            }
        }


        public RuntimeCard InitializeRuntimeCard(NetCard card, PlayerInfo player)
        {
            var runtimeCard = CreateRuntimeCard();
            runtimeCard.cardId = card.cardId;
            runtimeCard.instanceId = card.instanceId;
            runtimeCard.ownerPlayer = player;
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
            runtimeCard.type = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId).cardType;

            foreach (var abilityId in card.connectedAbilities)
            {
                runtimeCard.ConnectAbility(abilityId);
            }
            return runtimeCard;
        }


        public void CreateAndPutToHandRuntimeCard(NetCard card, PlayerInfo player)
        {
            var runtimeCard = InitializeRuntimeCard(card, player);

            player.namedZones[Constants.ZONE_HAND].AddCard(runtimeCard);
            EffectSolver.SetDestroyConditions(runtimeCard);
            EffectSolver.SetTriggers(runtimeCard);
        }

        public virtual void ReturnToHandRuntimeCard(NetCard card, PlayerInfo player, Vector3 cardPosition)
        {
            var runtimeCard = InitializeRuntimeCard(card, player);
            player.namedZones[Constants.ZONE_HAND].AddCardSilent(runtimeCard);

            DemoHumanPlayer controlPlayer = this is DemoHumanPlayer ? this as DemoHumanPlayer : (NetworkingUtils.GetHumanLocalPlayer() as DemoHumanPlayer);

            if (this is DemoHumanPlayer)
            {
                var createdHandCard = controlPlayer.AddCardToHand(runtimeCard);
                createdHandCard.transform.position = cardPosition;
                createdHandCard.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f); // size of the cards in hand
                controlPlayer.RearrangeHand(true);
            }
            else if(this is DemoAIPlayer)
            {
                var createdHandCard = controlPlayer.AddCardToOpponentHand();
                createdHandCard.transform.position = cardPosition;               
                controlPlayer.RearrangeOpponentHand(true, false);
            }

            EffectSolver.SetDestroyConditions(runtimeCard);
            EffectSolver.SetTriggers(runtimeCard);
        }

        public virtual void OnEndTurn(EndTurnMessage msg)
        {
            if (msg.isRecipientTheActivePlayer)
            {
                isActivePlayer = false;
                CleanupTurnLocalState();
            }

            EffectSolver.OnTurnEnded();

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
                            EffectSolver.CreateActiveAbility(playerInfo, card, 0);
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
                        EffectSolver.CreateActiveAbility(opponentInfo, card, 0);
                    }
                }
            }
        }

        public void PlayCreatureCard(RuntimeCard card, List<int> targetInfo = null)
        {
            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);

            if (!Constants.DEV_MODE || (this is DemoAIPlayer))
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

            if (!Constants.DEV_MODE || (this is DemoAIPlayer))
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

        public void FightPlayer(RuntimeCard attackingCard)
        {
            GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_HERO);
            EffectSolver.FightPlayer(netId, attackingCard.instanceId);

            var msg = new FightPlayerMessage();
            msg.attackingPlayerNetId = netId;
            msg.cardInstanceId = attackingCard.instanceId;
            client.Send(NetworkProtocol.FightPlayer, msg);
        }


		public void FightPlayerBySkill(int attack, bool isOpponent = true)
		{
            EffectSolver.FightPlayerBySkill(netId, attack, isOpponent);

            var msg = new FightPlayerBySkillMessage();
			msg.attackingPlayerNetId = netId;
			msg.attack = attack;
			msg.isOpponent = isOpponent;
            client.Send(NetworkProtocol.FightPlayerBySkill, msg);
		}


        public void HealPlayerBySkill(int value, bool isOpponent = true, bool isLimited = true)
        {
            EffectSolver.HealPlayerBySkill(netId, value, isOpponent, isLimited);

            var msg = new HealPlayerBySkillMessage();
            msg.callerPlayerNetId = netId;
            msg.value = value;
            msg.isOpponent = isOpponent;
            msg.isLimited = isLimited;
            client.Send(NetworkProtocol.HealPlayerBySkill, msg);
        }

        public void FightCreature(RuntimeCard attackingCard, RuntimeCard attackedCard)
        {
            GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_CARD);
            EffectSolver.FightCreature(netId, attackingCard, attackedCard);

			var msg = new FightCreatureMessage();
            msg.attackingPlayerNetId = netId;
            msg.attackingCardInstanceId = attackingCard.instanceId;
            msg.attackedCardInstanceId = attackedCard.instanceId;
            client.Send(NetworkProtocol.FightCreature, msg);
        }
		public void FightCreatureBySkill(int attack, RuntimeCard attackedCard)
		{
            EffectSolver.FightCreatureBySkill(netId, attackedCard, attack);

            var msg = new FightCreatureBySkillMessage();
            msg.attackingPlayerNetId = netId;
            msg.attackedCardInstanceId = attackedCard.instanceId;
            msg.attack = attack;

            client.Send(NetworkProtocol.FightCreatureBySkill, msg);
        }

        public void HealCreatureBySkill(int value, RuntimeCard card)
        {
            EffectSolver.HealCreatureBySkill(netId, card, value);

            var msg = new HealCreatureBySkillMessage();
            msg.callerPlayerNetId = netId;
            msg.cardInstanceId = card.instanceId;
            msg.value = value;
            client.Send(NetworkProtocol.HealCreatureBySkill, msg);
        }

        public void TryToAttackViaWeapon(int value)
        {
            EffectSolver.TryToAttackViaWeapon(netId, value);

            var msg = new TryToAttackViaWeaponMessage();
            msg.callerPlayerNetId = netId;
            msg.value = value;
            client.Send(NetworkProtocol.TryToAttackViaWeapon, msg);
        }

        public void DoBoardSkill(int effectType, int from, int to, int toType)
        {
            EffectSolver.PlaySkillEffect(netId, effectType, from, to, toType);

            var msg = new PlayEffectMessage();
            msg.callerPlayerNetId = netId;
            msg.effectType = effectType;
            msg.from = from;
            msg.to = to;
            msg.toType = toType;
            client.Send(NetworkProtocol.PlayEffect, msg);
        }

        public virtual void AddWeapon(GrandDevs.CZB.Data.Card card)
        {
           
        }
        public virtual void DestroyWeapon()
        {
        }
    }
}