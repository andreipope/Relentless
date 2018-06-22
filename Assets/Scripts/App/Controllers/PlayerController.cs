using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class PlayerController : IController
    {        1
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
        public virtual List<BoardCreature> playerBoardCardsList { get; set; }

        public RuntimeZone deckZone;
        public RuntimeZone handZone;
        public RuntimeZone boardZone;
        public RuntimeZone graveyardZone;

        public RuntimeZone opponentDeckZone;
        public RuntimeZone opponentHandZone;
        public RuntimeZone opponentBoardZone;
        public RuntimeZone opponentGraveyardZone;

        public int heroDeckId;
        public int opponentDeckId;

        public BoardSkill boardSkill { get; protected set; }

        protected virtual void Awake()
        {
            client = NetworkManager.singleton.client;
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

        public PlayerController()
        {
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }


        protected virtual void RegisterWithServer()
        {
            var msgDefaultDeck = new List<int>();

            //var defaultDeckIndex = isHuman ? PlayerPrefs.GetInt("default_deck") : PlayerPrefs.GetInt("default_ai_deck");

            if (GameManager.Instance.tutorial)
            {
                if (isHuman)
                {
                    msgDefaultDeck.Add(21);
                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(1);
                    msgDefaultDeck.Add(18);
                }
                else
                {
                    msgDefaultDeck.Add(10);
                    msgDefaultDeck.Add(7);
                    msgDefaultDeck.Add(11);
                    msgDefaultDeck.Add(10);
                    msgDefaultDeck.Add(10);
                }
                //int deckId = (GameClient.Get<IUIManager>().GetPage<GameplayPage>() as GameplayPage).CurrentDeckId;
                // int heroId = GameClient.Get<IDataManager>().CachedDecksData.decks[deckId].heroId = 1;
                // int heroId = GameClient.Get<IDataManager>().CachedDecksData.decks[deckId]. = 1;
            }
            else
            {
                if (isHuman)
                {
                    var deckId = GameClient.Get<IGameplayManager>().PlayerDeckId;
                    foreach (var card in GameClient.Get<IDataManager>().CachedDecksData.decks[deckId].cards)
                    {
                        for (var i = 0; i < card.amount; i++)
                        {
                            if (Constants.DEV_MODE)
                            {
                                //card.cardId = 27;
                            }
                            msgDefaultDeck.Add(card.cardId);
                        }
                    }
                }
                else
                {
                    var deckId = GameClient.Get<IGameplayManager>().OpponentDeckId;
                    foreach (var card in GameClient.Get<IDataManager>().CachedOpponentDecksData.decks[deckId].cards)
                    {
                        for (var i = 0; i < card.amount; i++)
                        {
                            if (Constants.DEV_MODE)
                            {
                                //card.cardId = 1;
                            }
                            msgDefaultDeck.Add(card.cardId);
                        }
                    }
                    var deck = GameClient.Get<IDataManager>().CachedOpponentDecksData.decks[deckId];
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
            else if (this is DemoAIPlayer)
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

        public virtual void StopTurn()
        {
            if (!isLocalPlayer)
                return;

            GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.END_TURN);

            isActivePlayer = false;
            var msg = new StopTurnMessage();
            client.Send(NetworkProtocol.StopTurn, msg);
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