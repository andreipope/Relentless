// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Gameplay;
using LoomNetwork.CZB.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LoomNetwork.CZB
{
    public class CardsController : IController
    {
        public event Action<Player> UpdateCardsStatusEvent;

        private IGameplayManager _gameplayManager;
        private ITimerManager _timerManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IDataManager _dataManager;
        private ISoundManager _soundManager;
        private ITutorialManager _tutorialManager;
        private IUIManager _uiManager;

        private BattlegroundController _battlegroundController;
        private VFXController _vfxController;
        private AbilitiesController _abilitiesController;
        private ActionsQueueController _actionsQueueController;
        private AnimationsController _animationsController;
        private RanksController _ranksController;

        private GameObject _playerBoard;
        private GameObject _opponentBoard;


        private int _cardInstanceId = 0;

        public GameObject creatureCardViewPrefab,
                           opponentCardPrefab,
                           spellCardViewPrefab;

        public bool CardDistribution { get; set; }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _vfxController = _gameplayManager.GetController<VFXController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _animationsController = _gameplayManager.GetController<AnimationsController>();
            _ranksController = _gameplayManager.GetController<RanksController>();


            creatureCardViewPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            spellCardViewPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/SpellCard");
            opponentCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/OpponentCard");

            _gameplayManager.OnGameStartedEvent += OnGameStartedEventHandler;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public int GetNewCardInstanceId()
        {
            return _cardInstanceId++;
        }

        private void OnGameStartedEventHandler()
        {
            _cardInstanceId = 0;

            _playerBoard = GameObject.Find("PlayerBoard");
            _opponentBoard = GameObject.Find("OpponentBoard");
        }


        public void StartCardDistribution()
        {
            CardDistribution = true;

            GameClient.Get<ICameraManager>().FadeIn(0.5f);

            if (!_gameplayManager.IsTutorial)
                _timerManager.AddTimer(DirectlyEndCardDistribution, null, Constants.CARD_DISTRIBUTION_TIME);
            else
                EndCardDistribution();
        }

        public void EndCardDistribution()
        {
            if (!CardDistribution)
                return;

            GameClient.Get<ICameraManager>().FadeOut(immediately: true);

            _timerManager.StopTimer(DirectlyEndCardDistribution);

            // for local player

            List<BoardCard> cards = new List<BoardCard>();
            cards.AddRange(_gameplayManager.CurrentPlayer.CardsPreparingToHand.FindAll(x => x.cardShouldBeDistributed));
            foreach (var card in cards)
                card.ReturnCardToDeck();

            foreach (var card in _gameplayManager.CurrentPlayer.CardsPreparingToHand)
            {
                _gameplayManager.CurrentPlayer.RemoveCardFromDeck(card.WorkingCard);
                _gameplayManager.CurrentPlayer.CardsInHand.Add(card.WorkingCard);
                _battlegroundController.playerHandCards.Add(card);

                _timerManager.AddTimer((x) =>
                {
                    card.HandBoardCard.enabled = true;
                }, null, 2f);
            }

            if (_gameplayManager.CurrentPlayer.CardsPreparingToHand.Count > 0)
            {
                _battlegroundController.UpdatePositionOfCardsInPlayerHand(true);
                _gameplayManager.CurrentPlayer.CardsPreparingToHand.Clear();
            }

            CardDistribution = false;

            if (!_gameplayManager.IsTutorial)
            {
                _gameplayManager.CurrentPlayer.CardsInDeck = _gameplayManager.CurrentPlayer.ShuffleCardsList(_gameplayManager.CurrentPlayer.CardsInDeck);

                if (_gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.CurrentPlayer))
                {
                    AddCardToHand(_gameplayManager.CurrentPlayer, _gameplayManager.CurrentPlayer.CardsInDeck[0]);
                    AddCardToHand(_gameplayManager.OpponentPlayer, _gameplayManager.OpponentPlayer.CardsInDeck[0]);
                    AddCardToHand(_gameplayManager.OpponentPlayer, _gameplayManager.OpponentPlayer.CardsInDeck[0]);
                }
                else
                {
                    AddCardToHand(_gameplayManager.OpponentPlayer, _gameplayManager.OpponentPlayer.CardsInDeck[0]);
                    AddCardToHand(_gameplayManager.CurrentPlayer, _gameplayManager.CurrentPlayer.CardsInDeck[0]);
                    AddCardToHand(_gameplayManager.CurrentPlayer, _gameplayManager.CurrentPlayer.CardsInDeck[0]);
                }
                _timerManager.AddTimer((x) =>
                {
                    _battlegroundController.StartGameplayTurns();
                }, null, 2f);
            }
            else
                _battlegroundController.StartGameplayTurns();
        }

        private void DirectlyEndCardDistribution(object[] param)
        {
            EndCardDistribution();
        }

        public void AddCardToDistributionState(Player player, WorkingCard card)
        {
            var boardCard = CreateBoardCard(card);
            player.CardsPreparingToHand.Add(boardCard);
            boardCard.HandBoardCard.enabled = false;
            boardCard.MoveCardFromDeckToCenter();
        }

        public void UpdatePositionOfCardsForDistribution(Player player)
        {
            int count = player.CardsPreparingToHand.Count;

            var handWidth = 0.0f;
            var spacing = -3f;

            handWidth += spacing * count - 1;

            var pivot = new Vector3(-1.5f, 0, 0);

            Vector3 moveToPosition = Vector3.zero;

            for (var i = 0; i < count; i++)
            {
                moveToPosition = new Vector3(pivot.x - handWidth / 2f, 0, 0);
                player.CardsPreparingToHand[i].transform.DOMove(moveToPosition, 1f, false);

                pivot.x += handWidth / count;
            }
        }

        public void ReturnCardToDeck(BoardCard card, Action callback)
        {
            card.WorkingCard.owner.CardsPreparingToHand.Remove(card);
            MonoBehaviour.Destroy(card.gameObject);

            callback?.Invoke();

            // UpdatePositionOfCardsForDistribution(card.WorkingCard.owner);
        }

        public void AddCardToHand(Player player, WorkingCard card = null)
        {
            if (card == null)
                card = player.CardsInDeck[0];

            player.RemoveCardFromDeck(card);
            player.AddCardToHand(card);
        }

        public GameObject AddCardToHand(WorkingCard card, bool silent = false)
        {
            var boardCard = CreateBoardCard(card);

            if (_battlegroundController.currentTurn == 0)
            {
                boardCard.SetDefaultAnimation(boardCard.WorkingCard.owner.CardsInHand.Count);
                //if (playerHandCards.Count == 4)
                //    RearrangeHand();
            }

            _battlegroundController.playerHandCards.Add(boardCard);

            if (silent)
            {
                boardCard.HandBoardCard.enabled = false;

                _timerManager.AddTimer((x) =>
                {
                    boardCard.HandBoardCard.enabled = true;
                    boardCard.HandBoardCard.CheckStatusOfHighlight();

                }, null, 2f);
            }
            else
            {
                boardCard.HandBoardCard.CheckStatusOfHighlight();
            }
            // UpdateCardsStatusEvent?.Invoke(card.owner);

            return boardCard.gameObject;
        }

        private BoardCard CreateBoardCard(WorkingCard card)
        {
            string cardSetName = GetSetOfCard(card.libraryCard);
            GameObject go = null;
            BoardCard boardCard = null;
            if (card.libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                go = MonoBehaviour.Instantiate(creatureCardViewPrefab);
                boardCard = new UnitBoardCard(go);
            }
            else if (card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                go = MonoBehaviour.Instantiate(spellCardViewPrefab);
                boardCard = new SpellBoardCard(go);
            }

            boardCard.Init(card);
            boardCard.CurrentTurn = _battlegroundController.currentTurn;

            var handCard = new HandBoardCard(go, boardCard);
            handCard.ownerPlayer = card.owner;
            handCard.boardZone = _playerBoard;
            boardCard.HandBoardCard = handCard;
            handCard.CheckStatusOfHighlight();
            boardCard.transform.localScale = Vector3.one * .3f;

            return boardCard;
        }

        public GameObject AddCardToOpponentHand(WorkingCard card, bool silent = false)
        {
            var opponent = _gameplayManager.OpponentPlayer;
            var go = MonoBehaviour.Instantiate(opponentCardPrefab);
            go.GetComponent<SortingGroup>().sortingOrder = opponent.CardsInHand.Count;

            _battlegroundController.opponentHandCards.Add(go);

            return go;
        }


        public void RemoveCard(object[] param)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CARD_BATTLEGROUND_TO_TRASH, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

            BoardCard card = param[0] as BoardCard;
            //BoardCreature currentCreature = null;
            //if (param.Length > 1)
            //    currentCreature = param[1] as BoardCreature;

            var go = card.gameObject;

            //if (!go.transform.Find("BackgroundBack").gameObject.activeSelf)
            //    return;

            var sortingGroup = card.gameObject.GetComponent<SortingGroup>();



            Sequence animationSequence3 = DOTween.Sequence();
            //animationSequence3.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 90, 90), .2f));
            animationSequence3.Append(go.transform.DORotate(new Vector3(0, 90, 90), .3f));
            //go.transform.DOScale(new Vector3(.19f, .19f, .19f), .2f);
            go.transform.DOScale(new Vector3(.195f, .195f, .195f), .2f);
            animationSequence3.OnComplete(() =>
            {

                go.transform.Find("Back").gameObject.SetActive(true);
                Sequence animationSequence4 = DOTween.Sequence();
                //animationSequence4.Append(go.transform.DORotate(new Vector3(40f, 180, 90f), .3f));
                animationSequence4.Append(go.transform.DORotate(new Vector3(0, 180, 0f), .45f));
                //animationSequence4.AppendInterval(2f);

                //Changing layers to all child objects to set them Behind the Graveyard Card
                sortingGroup.sortingLayerName = Constants.LAYER_FOREGROUND;
                sortingGroup.sortingOrder = 7;

                sortingGroup.gameObject.layer = 0;

                for (int i = 0; i < sortingGroup.transform.childCount; i++)
                {
                    Transform child = sortingGroup.transform.GetChild(i);

                    if (child.name != "Back")
                    {
                        child.gameObject.SetActive(false);
                    }
                    else
                    {
                        child.gameObject.layer = 0;
                    }
                }
            });

            Sequence animationSequence2 = DOTween.Sequence();
            //animationSequence2.Append(go.transform.DOMove(new Vector3(-4.1f, -1, 0), .3f));
            animationSequence2.Append(go.transform.DOMove(new Vector3(-6.57f, -1, 0), 0.7f));


            animationSequence2.OnComplete(() =>
            {


                for (int i = 0; i < sortingGroup.transform.childCount; i++)
                {
                    Transform child = sortingGroup.transform.GetChild(i);

                    if (child.name == "Back")
                    {
                        child.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    }
                }


                Sequence animationSequence5 = DOTween.Sequence();
                animationSequence5.Append(go.transform.DOMove(new Vector3(-6.57f, -4.352f, 0), .5f));
                animationSequence5.OnComplete(() =>
                {
                    MonoBehaviour.Destroy(go);
                });
            });
        }

        public void RemoveOpponentCard(object[] param)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CARD_BATTLEGROUND_TO_TRASH, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

            GameObject go = param[0] as GameObject;
            BoardUnit currentCreature = null;
            if (param.Length > 1)
                currentCreature = param[1] as BoardUnit;

            //if (!go.transform.Find("BackgroundBack").gameObject.activeSelf)
            //    return;
            var sortingGroup = go.GetComponent<SortingGroup>();

            Sequence animationSequence3 = DOTween.Sequence();
            //animationSequence3.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0, 90), .2f));
            animationSequence3.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0, -30f), .4f));
            go.transform.DOScale(new Vector3(1, 1, 1), .2f);
            animationSequence3.OnComplete(() =>
            {

                //    if (go.transform.Find("BackgroundBack") != null)
                //        go.transform.Find("BackgroundBack").gameObject.SetActive(true);
                //    //Sequence animationSequence4 = DOTween.Sequence();
                //    //animationSequence4.Append(go.transform.DORotate(new Vector3(40f, 180, 90f), .3f));
                //    //animationSequence4.AppendInterval(2f);
            });

            Sequence animationSequence2 = DOTween.Sequence();
            //animationSequence2.Append(go.transform.DOMove(new Vector3(-4.85f, 6.3f, 0), .3f));
            animationSequence2.Append(go.transform.DOMove(new Vector3(6.535f, 14f, 0), .6f));

            animationSequence2.OnComplete(() =>
            {
                go.layer = 0;
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    go.transform.GetChild(i).gameObject.layer = 0;
                }

                //sortingGroup.sortingLayerName = "Default";
                sortingGroup.sortingOrder = 7; // Foreground layer

                Sequence animationSequence4 = DOTween.Sequence();
                animationSequence4.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0f, 0f), .2f));

                Sequence animationSequence5 = DOTween.Sequence();
                animationSequence5.Append(go.transform.DOMove(new Vector3(6.535f, 6.306f, 0), .5f));
                animationSequence5.OnComplete(() =>
                {
                    MonoBehaviour.Destroy(go);
                });
            });
        }

        public void PlayPlayerCard(Player player, BoardCard card, HandBoardCard handCard)
        {
            if (card.CanBePlayed(card.WorkingCard.owner))
            {
                if (!Constants.DEV_MODE)
                    player.Mana -= card.libraryCard.cost;

                //  _actionsQueueController.AddNewActionInToQueue((parameter, actionComplete) =>
                // {
                // _uiManager.GetPage<GameplayPage>().SetEndTurnButtonStatus(false);

                _tutorialManager.ReportAction(Enumerators.TutorialReportAction.MOVE_CARD);

                var libraryCard = card.WorkingCard.libraryCard;

                string cardSetName = GetSetOfCard(libraryCard);

                card.transform.DORotate(Vector3.zero, .1f);
                card.HandBoardCard.enabled = false;

                _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);
                // GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CARDS, libraryCard.name.ToLower() + "_" + Constants.CARD_SOUND_PLAY, Constants.ZOMBIES_SOUND_VOLUME, false, true);

                if (libraryCard.cardKind == Enumerators.CardKind.CREATURE)
                {
                    int indexOfCard = 0;
                    float newCreatureCardPosition = card.transform.position.x;

                    // set correct position on board depends from card view position
                    for (int i = 0; i < player.BoardCards.Count; i++)
                    {
                        if (newCreatureCardPosition > player.BoardCards[i].transform.position.x)
                            indexOfCard = i + 1;
                        else break;
                    }

                    var boardUnit = new BoardUnit(_playerBoard.transform);
                    boardUnit.transform.tag = Constants.TAG_PLAYER_OWNED;
                    boardUnit.transform.parent = _playerBoard.transform;
                    boardUnit.transform.position = new Vector2(1.9f * player.BoardCards.Count, 0);
                    boardUnit.ownerPlayer = card.WorkingCard.owner;
                    boardUnit.SetObjectInfo(card.WorkingCard, cardSetName);

                    _battlegroundController.playerHandCards.Remove(card);
                    _battlegroundController.playerBoardCards.Add(boardUnit);
                    player.AddCardToBoard(card.WorkingCard);
                    player.RemoveCardFromHand(card.WorkingCard);

                    player.BoardCards.Insert(indexOfCard, boardUnit);

                    //_ranksController.UpdateRanksBuffs(player);

                    _timerManager.AddTimer((creat) =>
                    {
                        card.WorkingCard.owner.GraveyardCardsCount++;
                    }, null, 1f);

                    //Destroy(card.gameObject);
                    card.removeCardParticle.Play();

                    _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.PLAY_UNIT_CARD, new object[]
                    {
                        boardUnit.ownerPlayer,
                        boardUnit
                    }));


                    UpdateCardsStatusEvent?.Invoke(player);

                    Sequence animationSequence = DOTween.Sequence();
                    animationSequence.Append(card.transform.DOScale(new Vector3(.27f, .27f, .27f), 1f));
                    animationSequence.OnComplete(() =>
                    {
                        RemoveCard(new object[] { card });
                        _timerManager.AddTimer(_animationsController.PlayArrivalAnimationDelay, new object[] { boardUnit }, 0.1f, false);
                    });

                    //GameClient.Get<ITimerManager>().AddTimer(RemoveCard, new object[] {card}, 0.5f, false);
                    //_timerManager.AddTimer(PlayArrivalAnimationDelay, new object[] { currentCreature }, 0.7f, false);

                    _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(() =>
                    {
                        _abilitiesController.CallAbility(libraryCard, card, card.WorkingCard, Enumerators.CardKind.CREATURE, boardUnit, CallCardPlay, true, null);
                    });

                    //actionComplete?.Invoke();

                    //Debug.Log("<color=green> Now type: " + libraryCard.cardType + "</color>" + boardCreature.transform.position + "  " + currentCreature.transform.position);
                    //PlayArrivalAnimation(boardCreature, libraryCard.cardType);

                }
                else if (libraryCard.cardKind == Enumerators.CardKind.SPELL)
                {
                    //var spellsPivot = GameObject.Find("PlayerSpellsPivot");
                    //var sequence = DOTween.Sequence();
                    //sequence.Append(card.transform.DOMove(spellsPivot.transform.position, 0.5f));
                    //sequence.Insert(0, card.transform.DORotate(Vector3.zero, 0.2f));
                    //sequence.Play().OnComplete(() =>
                    //{ 


                    player.RemoveCardFromHand(card.WorkingCard);
                    _battlegroundController.playerHandCards.Remove(card);
                    _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                    card.gameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LAYER_BOARD_CARDS;
                    card.gameObject.GetComponent<SortingGroup>().sortingOrder = 1000;

                    var boardSpell = new BoardSpell(card.gameObject, card.WorkingCard);

                    _abilitiesController.CallAbility(libraryCard, card, card.WorkingCard, Enumerators.CardKind.SPELL, boardSpell, CallSpellCardPlay, true, null, handCard: handCard);
                    //});

                    //    actionComplete?.Invoke();
                }
                // }
                // );
            }
            else
            {
                card.HandBoardCard.ResetToInitialPosition();
            }
        }

        public void PlayOpponentCard(Player player, WorkingCard card, object target, Action<WorkingCard, object> completePlayCardCallback)
        {
            var randomCard = _battlegroundController.opponentHandCards[UnityEngine.Random.Range(0, _battlegroundController.opponentHandCards.Count)];

            _battlegroundController.opponentHandCards.Remove(randomCard);

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.MOVE_CARD);

            _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

            randomCard.transform.DOMove(Vector3.up * 2.5f, 0.6f).OnComplete(() =>
            {
                //GameClient.Get<ITimerManager>().AddTimer(DestroyRandomCard, new object[] { randomCard }, 1f, false);
                //randomCard.GetComponent<Animator>().SetTrigger("RemoveCard");
                randomCard.transform.Find("RemoveCardParticle").GetComponent<ParticleSystem>().Play();

                randomCard.transform.DOScale(Vector3.one * 1.2f, 0.6f).OnComplete(() =>
                {
                    RemoveOpponentCard(new object[] { randomCard });

                    _timerManager.AddTimer((x) => { completePlayCardCallback?.Invoke(card, target); }, null, 0.1f);
                    _ranksController.UpdateRanksBuffs(player);
                    _timerManager.AddTimer((x) =>
                    {
                        player.GraveyardCardsCount++;
                    }, null, 1f);
                });
            });

            randomCard.transform.DORotate(Vector3.zero, 0.5f);

            _battlegroundController.UpdatePositionOfCardsInOpponentHand(true);
        }

        public void DrawCardInfo(WorkingCard card)
        {
            string cardSetName = GetSetOfCard(card.libraryCard); ;

            GameObject go = null;
            BoardCard boardCard = null;
            if (card.libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                go = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard"));
                boardCard = new UnitBoardCard(go);
            }
            else if (card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                go = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/SpellCard"));
                boardCard = new SpellBoardCard(go);

            }
            boardCard.Init(card);
            go.transform.position = new Vector3(-6, 0, 0);
            go.transform.localScale = Vector3.one * .3f;
            boardCard.SetHighlightingEnabled(false);

            MonoBehaviour.Destroy(go, 2f);
        }

        private void CallCardPlay(BoardCard card)
        {

        }

        private void CallSpellCardPlay(BoardCard card)
        {

        }

        public void ReturnToHandBoardUnit(WorkingCard workingCard, Player player, Vector3 cardPosition)
        {
            var cardObject = player.AddCardToHand(workingCard, true);
            cardObject.transform.position = cardPosition;

            if (player.IsLocalPlayer)
                cardObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f); // size of the cards in hand         
        }

        public void LowGooCostOfCardInHand(Player player, WorkingCard card = null)
        {
            if (card == null && player.CardsInHand.Count > 0)
                card = player.CardsInHand[UnityEngine.Random.Range(0, player.CardsInHand.Count)];

            if (card == null)
                return;

            if (player.IsLocalPlayer)
            {
                var boardCard = _battlegroundController.playerHandCards.Find(x => x.WorkingCard.Equals(card));

                boardCard.SetCardCost(Mathf.Clamp(boardCard.manaCost - 1, 0, boardCard.manaCost));
            }
            else
            {
                card.libraryCard.cost = Mathf.Clamp(card.libraryCard.cost - 1, 0, card.libraryCard.cost);
            }
        }

        public string GetSetOfCard(Card card)
        {
            var set = _dataManager.CachedCardsLibraryData.sets.Find(x => x.cards.Find(y => y.id == card.id) != null);

            if (set != null)
                return set.name;
            return string.Empty;
        }
    }
}