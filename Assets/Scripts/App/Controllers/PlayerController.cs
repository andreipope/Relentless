using GrandDevs.CZB.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GrandDevs.CZB
{
    public class PlayerController : IController
    {
        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;
        private AbilitiesController _abilitiesController;
        private CardsController _cardsController;
        private BattlegroundController _battlegroundController;
  
        public Player PlayerInfo { get; protected set; }

        public BoardWeapon CurrentBoardWeapon { get; protected set; }
        public BoardSkill boardSkill { get; protected set; }


        public bool AlreadyAttackedInThisTurn { get; set; }
        public bool IsPlayerStunned { get; set; }
        public bool IsCardSelected { get; set; }
        public bool IsActive { get; set; }

        public SpellCardView currentSpellCard;
        public GameObject currentBoardCreature;
        public BoardCreature currentCreature;


        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            _gameplayManager.OnGameStartedEvent += OnGameStartedEventHandler;
            _gameplayManager.OnGameEndedEvent += OnGameEndedEventHandler;
            _gameplayManager.OnTurnStartedEvent += OnTurnStartedEventHandler;
            _gameplayManager.OnTurnEndedEvent += OnTurnEndedEventHandler;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            if (!_gameplayManager.GameStarted)
                return;

            if (GameClient.Get<ITutorialManager>().IsTutorial && (GameClient.Get<ITutorialManager>().CurrentStep != 8 &&
                                                             GameClient.Get<ITutorialManager>().CurrentStep != 17 &&
                                                             GameClient.Get<ITutorialManager>().CurrentStep != 19 &&
                                                             GameClient.Get<ITutorialManager>().CurrentStep != 27))
                return;

            HandleInput();

        }
          

        public void CallOnEndTurnEvent()
        {
            PlayerInfo.CallOnEndTurnEvent();
        }

        public void CallOnStartTurnEvent()
        {
            PlayerInfo.CallOnStartTurnEvent();
        }

        public void InitializePlayer()
        {
            PlayerInfo = new Player(GameObject.Find("Player"), false);

            _gameplayManager.PlayersInGame.Add(PlayerInfo);

            GameClient.Get<IPlayerManager>().PlayerInfo = PlayerInfo;

            var playerDeck = new List<int>();

            if (_gameplayManager.IsTutorial)
            {
                playerDeck.Add(21);
                playerDeck.Add(1);
                playerDeck.Add(1);
                playerDeck.Add(1);
                playerDeck.Add(18);
            }
            else
            {
                var deckId = _gameplayManager.PlayerDeckId;
                foreach (var card in _dataManager.CachedDecksData.decks[deckId].cards)
                {
                    for (var i = 0; i < card.amount; i++)
                    {
                        if (Constants.DEV_MODE)
                        {
                            //card.cardId = 27; 
                        }

                        playerDeck.Add(card.cardId);
                    }
                }
            }

            PlayerInfo.SetDeck(playerDeck);

            for (int i = 0; i < PlayerInfo.CardsInDeck.Count; i++)
            {
                if (i >= Constants.DEFAULT_CARDS_IN_HAND_AT_START_GAME)
                    break;

                _cardsController.AddCardToHand(PlayerInfo, PlayerInfo.CardsInDeck[i]);
            }

            foreach (var card in PlayerInfo.CardsInHand)
                _cardsController.AddCardToHand(card);

            _battlegroundController.RearrangeHand();
        }

       
        public virtual void OnGameStartedEventHandler()
        {
          //  LoadPlayerStates(msg.player, msg.opponent);
        }


        public virtual void OnGameEndedEventHandler()
        {
           
        }

        private void HandleInput()
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                if (IsActive && currentSpellCard == null)
                {
                    var hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                    var hitCards = new List<GameObject>();
                    foreach (var hit in hits)
                    {
                        if (hit.collider != null &&
                            hit.collider.gameObject != null &&
                            hit.collider.gameObject.GetComponent<CardView>() != null &&
                            !hit.collider.gameObject.GetComponent<CardView>().isPreview &&
                            hit.collider.gameObject.GetComponent<CardView>().CanBePlayed(PlayerInfo))
                        {
                            hitCards.Add(hit.collider.gameObject);
                        }
                    }
                    if (hitCards.Count > 0)
                    {
                        _battlegroundController.DestroyCardPreview();
                        hitCards = hitCards.OrderByDescending(x => x.transform.position.z).ToList();
                        var topmostCardView = hitCards[hitCards.Count - 1].GetComponent<CardView>();
                        var topmostHandCard = topmostCardView.GetComponent<HandCard>();
                        if (topmostHandCard != null)
                        {
                            topmostCardView.GetComponent<HandCard>().OnSelected();
                            if (GameClient.Get<ITutorialManager>().IsTutorial)
                            {
                                GameClient.Get<ITutorialManager>().DeactivateSelectTarget();
                            }
                        }
                    }
                }
            }
            else if (!IsCardSelected)
            {
                var hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                var hitCards = new List<GameObject>();
                var hitHandCard = false;
                var hitBoardCard = false;
                foreach (var hit in hits)
                {
                    if (hit.collider != null &&
                        hit.collider.gameObject != null &&
                        hit.collider.gameObject.GetComponent<CardView>() != null)
                    {
                        hitCards.Add(hit.collider.gameObject);
                        hitHandCard = true;
                    }
                }
                if (!hitHandCard)
                {
                    foreach (var hit in hits)
                    {
                        if (hit.collider != null && hit.collider.name.Contains("BoardCreature"))
                        {
                            hitCards.Add(hit.collider.gameObject);
                            hitBoardCard = true;
                        }
                    }
                }

                if (hitHandCard)
                {
                    if (hitCards.Count > 0)
                    {
                        hitCards = hitCards.OrderBy(x => x.GetComponent<SortingGroup>().sortingOrder).ToList();
                        var topmostCardView = hitCards[hitCards.Count - 1].GetComponent<CardView>();
                        if (!topmostCardView.isPreview)
                        {
                            if (!_battlegroundController.isPreviewActive || topmostCardView.WorkingCard.instanceId != _battlegroundController.currentPreviewedCardId)
                            {
                                _battlegroundController.DestroyCardPreview();
                                _battlegroundController.CreateCardPreview(topmostCardView.WorkingCard, topmostCardView.transform.position, IsActive);
                            }
                        }
                    }
                }
                else if (hitBoardCard)
                {
                    if (hitCards.Count > 0)
                    {
                        hitCards = hitCards.OrderBy(x => x.GetComponent<SortingGroup>().sortingOrder).ToList();
                        var selectedBoardCreature = hitCards[hitCards.Count - 1].GetComponent<BoardCreature>(); /// CHECK OBJECT 
                        if (!_battlegroundController.isPreviewActive || selectedBoardCreature.Card.instanceId != _battlegroundController.currentPreviewedCardId)
                        {
                            _battlegroundController.DestroyCardPreview();
                            _battlegroundController.CreateCardPreview(selectedBoardCreature.Card, selectedBoardCreature.transform.position, false);
                        }
                    }
                }
                else
                {
                    _battlegroundController.DestroyCardPreview();
                }
            }

        }


        /*  
       public virtual void OnStartTurn()
       {

           CleanupTurnLocalState();


           LoadPlayerStates(msg.player, msg.opponent, msg.isRecipientTheActivePlayer);
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

                 // move to ai controller
               //var createdHandCard = controlPlayer.AddCardToOpponentHand();
               //createdHandCard.transform.position = cardPosition;
               //controlPlayer.RearrangeOpponentHand(true, false);

   }
}
*/
        public void OnTurnEndedEventHandler()
        {
            //foreach (var entry in gameState.currentPlayer.stats)
            //{
            //    entry.Value.OnEndTurn();
            //}

            //foreach (var zone in gameState.currentPlayer.zones)
            //{
            //    foreach (var card in zone.Value.cards)
            //    {
            //        foreach (var stat in card.stats)
            //        {
            //            stat.Value.OnEndTurn();
            //        }
            //    }
            //}
        }

        public void OnTurnStartedEventHandler()
        {

        }

        public void UpdateHandCardsHighlight()
        {
            if (boardSkill != null && IsActive)
            {
                if (PlayerInfo.Mana >= boardSkill.manaCost)
                    boardSkill.SetHighlightingEnabled(true);
                else
                    boardSkill.SetHighlightingEnabled(false);
            }

            foreach (var card in _battlegroundController.playerHandCards)
            {
                if (card.CanBePlayed(PlayerInfo) && card.CanBeBuyed(PlayerInfo))
                {
                    card.SetHighlightingEnabled(true);
                }
                else
                {
                    card.SetHighlightingEnabled(false);
                }
            }
        }


        public void PlayCreatureCard(WorkingCard card, List<int> targetInfo = null)
        {
            var libraryCard = _dataManager.CachedCardsLibraryData.GetCard(card.cardId);

            if (!Constants.DEV_MODE)
                PlayerInfo.Mana -= libraryCard.cost;

            PlayerInfo.RemoveCardFromHand(card);
            PlayerInfo.AddCardToBoard(card);

            //var msg = new MoveCardMessage();
            //msg.playerNetId = netId;
            //msg.cardInstanceId = card.instanceId;
            //msg.originZoneId = playerInfo.namedZones["Hand"].zoneId;
            //msg.destinationZoneId = playerInfo.namedZones["Board"].zoneId;
            //if (targetInfo != null)
            //{
            //    msg.targetInfo = targetInfo.ToArray();
            //}
            //client.Send(NetworkProtocol.MoveCard, msg);
        }

        public void PlaySpellCard(WorkingCard card, List<int> targetInfo = null)
        {
            var libraryCard = _dataManager.CachedCardsLibraryData.GetCard(card.cardId);

            if (!Constants.DEV_MODE)
                PlayerInfo.Mana -= libraryCard.cost;


            PlayerInfo.RemoveCardFromHand(card);
            PlayerInfo.AddCardToBoard(card);

            //var msg = new MoveCardMessage();
            //msg.playerNetId = netId;
            //msg.cardInstanceId = card.instanceId;
            //msg.originZoneId = playerInfo.namedZones["Hand"].zoneId;
            //msg.destinationZoneId = playerInfo.namedZones["Board"].zoneId;
            //if (targetInfo != null)
            //{
            //    msg.targetInfo = targetInfo.ToArray();
            //}
            //client.Send(NetworkProtocol.MoveCard, msg);
        }


        /*       public virtual void OnCardMoved(CardMovedMessage msg)
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


               public void FightPlayer(WorkingCard attackingCard)
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

               public void FightCreature(WorkingCard attackingCard, WorkingCard attackedCard)
               {
                   GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_CARD);
                   EffectSolver.FightCreature(netId, attackingCard, attackedCard);

                   var msg = new FightCreatureMessage();
                   msg.attackingPlayerNetId = netId;
                   msg.attackingCardInstanceId = attackingCard.instanceId;
                   msg.attackedCardInstanceId = attackedCard.instanceId;
                   client.Send(NetworkProtocol.FightCreature, msg);
               }
               public void FightCreatureBySkill(int attack, WorkingCard attackedCard)
               {
                   EffectSolver.FightCreatureBySkill(netId, attackedCard, attack);

                   var msg = new FightCreatureBySkillMessage();
                   msg.attackingPlayerNetId = netId;
                   msg.attackedCardInstanceId = attackedCard.instanceId;
                   msg.attack = attack;

                   client.Send(NetworkProtocol.FightCreatureBySkill, msg);
               }

               public void HealCreatureBySkill(int value, WorkingCard card)
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

              */
    }
}
 