using GrandDevs.CZB.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class PlayerController : IController
    {
        public event Action OnEndTurnEvent;
        public event Action OnStartTurnEvent;


        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;
        private AbilitiesController _abilitiesController;

        public Player PlayerInfo { get; protected set; }

        public BoardWeapon CurrentBoardWeapon { get; protected set; }
        public BoardSkill boardSkill { get; protected set; }


        public bool AlreadyAttackedInThisTurn { get; set; }
        public bool IsPlayerStunned { get; set; }
        public bool IsCardSelected { get; set; }
        public bool IsActive { get; set; }


        public PlayerController()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();

            GameClient.Get<IPlayerManager>().PlayerInfo = PlayerInfo;


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
        }

        public void CallOnEndTurnEvent()
        {
            OnEndTurnEvent?.Invoke();
        }

        public void CallOnStartTurnEvent()
        {
            OnStartTurnEvent?.Invoke();
        }

        public void InitializePlayer()
        {
            var playerDeck = new List<int>();

            if (_gameplayManager.IsTutorial)
            {
                playerDeck.Add(21);
                playerDeck.Add(1);
                playerDeck.Add(1);
                playerDeck.Add(1);
                playerDeck.Add(18);

                // move to ai controller ------------------------------------------------------
                /*   msgDefaultDeck.Add(10);
                   msgDefaultDeck.Add(7);
                   msgDefaultDeck.Add(11);
                   msgDefaultDeck.Add(10);
                   msgDefaultDeck.Add(10); */
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

                // move to ai controller ------------------------------------------------------
                /*
                   var deckId = GameClient.Get<IGameplayManager>().OpponentDeckId;
                   foreach (var card in GameClient.Get<IDataManager>().CachedOpponentDecksData.decks[deckId].cards)
                   {
                       for (var i = 0; i < card.amount; i++)
                       {
                           if (Constants.DEV_MODE)
                           {
                               //card.cardId = 1;
                           }
                           playerDeck.Add(card.cardId);
                       }
                   }
                   */
            }

            PlayerInfo.CardsInDeck = playerDeck;
        }

        public virtual void OnGameStartedEventHandler()
        {
            LoadPlayerStates(msg.player, msg.opponent);
        }


        public virtual void OnGameEndedEventHandler()
        {
           
        }

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
                /* // move to ai controller
                var createdHandCard = controlPlayer.AddCardToOpponentHand();
                createdHandCard.transform.position = cardPosition;
                controlPlayer.RearrangeOpponentHand(true, false); */
            }
        }

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
            var libraryCard = _dataManager.CachedCardsLibraryData.GetCard(card.cardId);

            if (!Constants.DEV_MODE)
                PlayerInfo.Mana -= libraryCard.cost;

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

        public void PlaySpellCard(RuntimeCard card, List<int> targetInfo = null)
        {
            var libraryCard = _dataManager.CachedCardsLibraryData.GetCard(card.cardId);

            if (!Constants.DEV_MODE)
                PlayerInfo.Mana -= libraryCard.cost;

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

        public void PlayCard(CardView card, HandCard handCard)
        {
            if (card.CanBePlayed(this))
            {
                gameUI.endTurnButton.SetEnabled(false);

                GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.MOVE_CARD);

                var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.card.cardId);

                string cardSetName = string.Empty;
                foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
                {
                    if (cardSet.cards.IndexOf(libraryCard) > -1)
                        cardSetName = cardSet.name;
                }

                card.transform.DORotate(Vector3.zero, .1f);
                card.GetComponent<HandCard>().enabled = false;

                GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);
                // GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_PLAY, Constants.ZOMBIES_SOUND_VOLUME, false, true);

                if ((Enumerators.CardKind)libraryCard.cardKind == Enumerators.CardKind.CREATURE)
                {
                    int indexOfCard = 0;
                    float newCreatureCardPosition = card.transform.position.x;

                    // set correct position on board depends from card view position
                    for (int i = 0; i < playerBoardCards.Count; i++)
                    {
                        if (newCreatureCardPosition > playerBoardCards[i].transform.position.x)
                            indexOfCard = i + 1;
                        else break;
                    }

                    var boardCreature = Instantiate(boardCreaturePrefab);

                    var board = GameObject.Find("PlayerBoard");
                    boardCreature.tag = "PlayerOwned";
                    boardCreature.transform.parent = board.transform;
                    boardCreature.transform.position = new Vector2(1.9f * playerBoardCards.Count, 0);
                    boardCreature.GetComponent<BoardCreature>().ownerPlayer = this;
                    boardCreature.GetComponent<BoardCreature>().PopulateWithInfo(card.card, cardSetName);

                    playerHandCards.Remove(card);
                    RearrangeHand();
                    playerBoardCards.Insert(indexOfCard, boardCreature.GetComponent<BoardCreature>());

                    GameClient.Get<ITimerManager>().AddTimer((creat) =>
                    {
                        GraveyardCardsCount++;
                    }, null, 1f);

                    //Destroy(card.gameObject);
                    card.removeCardParticle.Play();


                    currentCreature = boardCreature.GetComponent<BoardCreature>();


                    Sequence animationSequence = DOTween.Sequence();
                    animationSequence.Append(card.transform.DOScale(new Vector3(.27f, .27f, .27f), 1f));
                    animationSequence.OnComplete(() =>
                    {
                        RemoveCard(new object[] { card });
                        _timerManager.AddTimer(PlayArrivalAnimationDelay, new object[] { boardCreature.GetComponent<BoardCreature>() }, 0.1f, false);
                    });

                    //GameClient.Get<ITimerManager>().AddTimer(RemoveCard, new object[] {card}, 0.5f, false);
                    //_timerManager.AddTimer(PlayArrivalAnimationDelay, new object[] { currentCreature }, 0.7f, false);

                    RearrangeBottomBoard(() =>
                    {
                        CallAbility(libraryCard, card, card.card, Enumerators.CardKind.CREATURE, currentCreature, CallCardPlay, true);
                    });

                    //Debug.Log("<color=green> Now type: " + libraryCard.cardType + "</color>" + boardCreature.transform.position + "  " + currentCreature.transform.position);
                    //PlayArrivalAnimation(boardCreature, libraryCard.cardType);

                }
                else if ((Enumerators.CardKind)libraryCard.cardKind == Enumerators.CardKind.SPELL)
                {
                    //var spellsPivot = GameObject.Find("PlayerSpellsPivot");
                    //var sequence = DOTween.Sequence();
                    //sequence.Append(card.transform.DOMove(spellsPivot.transform.position, 0.5f));
                    //sequence.Insert(0, card.transform.DORotate(Vector3.zero, 0.2f));
                    //sequence.Play().OnComplete(() =>
                    //{ 
                    card.GetComponent<SortingGroup>().sortingLayerName = "BoardCards";
                    card.GetComponent<SortingGroup>().sortingOrder = 1000;

                    var boardSpell = card.gameObject.AddComponent<BoardSpell>();

                    Debug.Log(card.name);

                    CallAbility(libraryCard, card, card.card, Enumerators.CardKind.SPELL, boardSpell, CallSpellCardPlay, true, handCard: handCard);
                    //});
                }
            }
            else
            {
                card.GetComponent<HandCard>().ResetToInitialPosition();
            }
        }
    }
}
 