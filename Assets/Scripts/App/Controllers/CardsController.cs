using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.View;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class CardsController : IController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(CardsController));

        public event Action<AbilityData.ChoosableAbility> CardForAbilityChoosed;

        public GameObject CreatureCardViewPrefab, OpponentCardPrefab, ItemCardViewPrefab;

        private IGameplayManager _gameplayManager;

        private ITimerManager _timerManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private ISoundManager _soundManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private IMatchManager _matchManager;

        private IPvPManager _pvpManager;

        private BattlegroundController _battlegroundController;

        private VfxController _vfxController;

        private AbilitiesController _abilitiesController;

        private ActionsQueueController _actionsQueueController;

        private AnimationsController _animationsController;

        private RanksController _ranksController;

        private BoardController _boardController;

        public GameObject PlayerBoard { get; private set; }

        public GameObject OpponentBoard { get; private set; }

        private int _cardInstanceId;

        private int _indexOfCard;

        public event Action<Player> UpdateCardsStatusEvent;

        public bool CardDistribution { get; set; }

        private bool _isHoveringCardOfBoard;

        public List<BoardUnitModel> MulliganCards;

        private List<ChoosableCardForAbility> _currentListOfChoosableCards;

        public bool HasChoosableCardsForAbilities { get { return _currentListOfChoosableCards.Count > 0; } }

        private Transform _parentOfSelectableCards;

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _matchManager = GameClient.Get<IMatchManager>();
            _pvpManager = GameClient.Get<IPvPManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _vfxController = _gameplayManager.GetController<VfxController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _animationsController = _gameplayManager.GetController<AnimationsController>();
            _ranksController = _gameplayManager.GetController<RanksController>();
            _boardController = _gameplayManager.GetController<BoardController>();

            CreatureCardViewPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard");
            ItemCardViewPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard");
            OpponentCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/OpponentCard");

            _gameplayManager.GameStarted += GameStartedHandler;
            _gameplayManager.GameEnded += GameEndedHandler;

            _indexOfCard = -1;
        }

        public void Dispose()
        {
        }

        public void ResetAll()
        {
            ResetChoosalbeCardsList();
        }

        public void Update()
        {
        }

        public InstanceId GetNewCardInstanceId()
        {
            _cardInstanceId++;
            return new InstanceId(_cardInstanceId);
        }

        public InstanceId GetCardInstanceId()
        {
            return new InstanceId(_cardInstanceId);
        }

        public void SetNewCardInstanceId(int id)
        {
            _cardInstanceId = id;
        }

        public void StartCardDistribution()
        {
            CardDistribution = true;

            if (Constants.MulliganEnabled && !_pvpManager.DebugCheats.SkipMulligan || GameClient.Get<IMatchManager>().MatchType != Enumerators.MatchType.PVP)
            {
                GameClient.Get<ICameraManager>().FadeIn(0.8f, 0, false);

                if (_gameplayManager.IsTutorial && !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().MulliganScreenShouldAppear)
                {
                    EndCardDistribution();
                }
                else
                {
                    _uiManager.DrawPopup<MulliganPopup>();
                }
            }
            else
            {
                _uiManager.GetPopup<WaitingForPlayerPopup>().Show("Waiting for the opponent...");
            }
        }

        public void EndCardDistribution()
        {
            GameplayPage gameplayPage = _uiManager.GetPage<GameplayPage>();
            if (gameplayPage.Self)
            {
                gameplayPage.SettingsAndBackButtonVisibility(true);
            }

            if (!CardDistribution)
                return;

            _gameplayManager.IsPreparingEnded = true;

            GameClient.Get<ICameraManager>().FadeOut(immediately: true);

            _timerManager.StopTimer(DirectlyEndCardDistribution);

            // for local player
            foreach (BoardUnitModel card in _gameplayManager.CurrentPlayer.CardsPreparingToHand)
            {
                _gameplayManager.CurrentPlayer.PlayerCardsController.AddCardFromDeckToHand(card);
            }
            _gameplayManager.CurrentPlayer.PlayerCardsController.SetCardsPreparingToHand(Array.Empty<BoardUnitModel>());

            CardDistribution = false;

            _gameplayManager.CurrentPlayer.PlayerCardsController.InvokeHandChanged();

            if (GameClient.Get<IMatchManager>().MatchType != Enumerators.MatchType.PVP && (!_gameplayManager.IsTutorial ||
                (_gameplayManager.IsTutorial &&
                _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.DisabledInitialization)))
            {
                _gameplayManager.CurrentPlayer.PlayerCardsController.ShuffleCardsInDeck();
            }

            _battlegroundController.StartGameplayTurns();

            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP &&
                GameClient.Get<BackendFacade>().IsConnected)
            {
                IEnumerable<Protobuf.CardInstance> cards = _pvpManager.InitialGameState.PlayerStates
                 .SelectMany(state =>
                     state.MulliganCards
                         .Concat(state.CardsInDeck)
                         .Concat(state.CardsInHand)
                         .Concat(state.CardsInPlay)
                         .Concat(state.CardsInGraveyard));

                int highestInstanceId = 0;

                if (cards.Count() > 0)
                {
                    cards.Max(card => card.InstanceId.Id);
                    highestInstanceId = cards.Count();
                }
                else
                {
                    Log.Warn($"[Out of sync] Sequence contains no elements in player state. connection status: {GameClient.Get<BackendFacade>().IsConnected}");
                }

                SetNewCardInstanceId(highestInstanceId);
            }
        }

        public void CardsDistribution(IReadOnlyList<BoardUnitModel> mulliganCards)
        {
            Player player = _gameplayManager.CurrentPlayer;
            List<BoardUnitModel> randomCards = new List<BoardUnitModel>();

            int count = 0;
            while (randomCards.Count < mulliganCards.Count)
            {
                if (!player.CardsPreparingToHand.Contains(player.CardsInDeck[count]) && !mulliganCards.Contains(player.CardsInDeck[count]))
                {
                    randomCards.Add(player.CardsInDeck[count]);
                }
                count++;
            }

            UniquePositionedList<BoardUnitModel> finalCards =
                player.CardsPreparingToHand
                    .Except(mulliganCards)
                    .Concat(randomCards)
                    .ToUniquePositionedList();
            player.PlayerCardsController.SetCardsPreparingToHand(finalCards);

            EndCardDistribution();
        }

        public void RemoveCard(BoardCardView card)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CARD_BATTLEGROUND_TO_TRASH, Constants.CardsMoveSoundVolume);

            GameObject go = card.GameObject;

            SortingGroup sortingGroup = card.GameObject.GetComponent<SortingGroup>();

            Sequence animationSequence3 = DOTween.Sequence();
            animationSequence3.Append(go.transform.DORotate(new Vector3(0, 90, 90), .3f));

            go.transform.DOScale(new Vector3(.195f, .195f, .195f), .2f);
            animationSequence3.OnComplete(
                () =>
                {
                    go.transform.Find("Back").gameObject.SetActive(true);
                    Sequence animationSequence4 = DOTween.Sequence();
                    animationSequence4.Append(go.transform.DORotate(new Vector3(0, 180, 0f), .45f));

                    // Changing layers to all child objects to set them Behind the Graveyard Card
                    sortingGroup.sortingLayerID = SRSortingLayers.Foreground;
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
            animationSequence2.Append(go.transform.DOMove(new Vector3(-7.74f, -1, 0), 0.7f));

            animationSequence2.OnComplete(
                () =>
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
                    animationSequence5.Append(go.transform.DOMove(new Vector3(-7.74f, -4.352f, 0), .5f));
                    animationSequence5.OnComplete(
                        () =>
                        {
                            Object.Destroy(go);
                        });
                });
        }

        public void RemoveOpponentCard(OpponentHandCard opponentHandCard)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CARD_BATTLEGROUND_TO_TRASH, Constants.CardsMoveSoundVolume);

            SortingGroup sortingGroup = opponentHandCard.GameObject.GetComponent<SortingGroup>();

            Sequence animationSequence3 = DOTween.Sequence();

            animationSequence3.Append(opponentHandCard.Transform.DORotate(new Vector3(opponentHandCard.Transform.eulerAngles.x, 0, -30f), .4f));
            opponentHandCard.Transform.DOScale(new Vector3(1, 1, 1), .2f);

            Sequence animationSequence2 = DOTween.Sequence();
            animationSequence2.Append(opponentHandCard.Transform.DOMove(new Vector3(7.7f, 14f, 0), .6f));

            animationSequence2.OnComplete(
                () =>
                {
                    opponentHandCard.GameObject.layer = SRLayers.Default;
                    for (int i = 0; i < opponentHandCard.Transform.childCount; i++)
                    {
                        opponentHandCard.Transform.GetChild(i).gameObject.layer = SRLayers.Default;
                    }

                    sortingGroup.sortingOrder = 7; // Foreground layer

                    Sequence animationSequence4 = DOTween.Sequence();
                    animationSequence4.Append(opponentHandCard.Transform.DORotate(new Vector3(opponentHandCard.Transform.eulerAngles.x, 0f, 0f), .2f));

                    Sequence animationSequence5 = DOTween.Sequence();
                    animationSequence5.Append(opponentHandCard.Transform.DOMove(new Vector3(7.7f, 6.306f, 0), .5f));
                    animationSequence5.OnComplete(
                        () =>
                        {
                            opponentHandCard.Dispose();
                        });
                });
        }

        public void HoverPlayerCardOnBattleground(Player player, BoardCardView card)
        {
            IReadOnlyCard prototype = card.Model.Card.Prototype;
            if (prototype.Kind == Enumerators.CardKind.CREATURE &&
                _gameplayManager.CurrentPlayer.CardsOnBoard.Count < _gameplayManager.CurrentPlayer.MaxCardsInPlay)
            {
                int newIndexOfCard = 0;
                float newCreatureCardPosition = card.Transform.position.x;
                IReadOnlyList<BoardUnitView> cardsOnBoardViews = _battlegroundController.GetBoardUnitViewsFromModels(player.CardsOnBoard);

                // set correct position on board depends from card view position
                for (int i = 0; i < player.CardsOnBoard.Count; i++)
                {
                    if (newCreatureCardPosition > cardsOnBoardViews[i].Transform.position.x)
                    {
                        newIndexOfCard = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (player.CardsOnBoard.Count > 0 && _indexOfCard != newIndexOfCard)
                {
                    _indexOfCard = newIndexOfCard;

                    IReadOnlyList<BoardUnitView> playerCards =
                        _battlegroundController.GetBoardUnitViewsFromModels(_gameplayManager.CurrentPlayer.CardsOnBoard);
                    List<BoardUnitView> toArrangeList = new List<BoardUnitView>();

                    for (int i = 0; i < playerCards.Count; i++)
                    {
                        toArrangeList.Add(playerCards[i]);
                    }

                    toArrangeList.Insert(_indexOfCard, null);
                    _boardController.UpdateBoard(toArrangeList, true, null, skipIndex: _indexOfCard);
                }
            }
        }

        public void ReturnCardToHand(BoardUnitModel boardUnitModel)
        {
            Player unitOwner = boardUnitModel.OwnerPlayer;
            BoardUnitView boardUnitView = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(boardUnitModel);

            boardUnitModel.Card.InstanceCard.Cost = boardUnitModel.Card.Prototype.Cost;
            boardUnitModel.DisableBuffsOnValueHistory(boardUnitModel.CurrentCostHistory);

            Vector3 unitPosition = boardUnitView.Transform.position;

            _battlegroundController.DeactivateAllAbilitiesOnUnit(boardUnitModel);

            boardUnitModel.InvokeUnitPrepairingToDie();

            InternalTools.DoActionDelayed(() =>
            {
                boardUnitModel.Die(true);
                boardUnitView.DisposeGameObject();

                unitOwner.PlayerCardsController.RemoveCardFromBoard(boardUnitModel);

                boardUnitModel.ResetToInitial();

                unitOwner.PlayerCardsController.ReturnToHandBoardUnit(boardUnitModel, unitPosition);               

                _gameplayManager.RearrangeHands();
            },
                2f);
        }

        public void ResetPlayerCardsOnBattlegroundPosition()
        {
            if (_indexOfCard != -1)
            {
                _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.CurrentPlayer, null);

                _indexOfCard = -1;
            }
        }

        public void PlayPlayerCard(Player player,
                                   BoardCardView card,
                                   HandBoardCard handCard,
                                   Action<PlayCardOnBoard> OnPlayPlayerCard,
                                   BoardObject target = null,
                                   bool skipEntryAbilities = false)
        {
            if (card.Model.CanBePlayed(card.Model.Card.Owner))
            {
                card.Transform.DORotate(Vector3.zero, .1f);
                card.HandBoardCard.Enabled = false;
                if (!_gameplayManager.AvoidGooCost)
                {
                    card.Model.Card.Owner.CurrentGoo -= card.Model.CurrentCost;
                }

                _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND,
                    Constants.CardsMoveSoundVolume);

                GameplayQueueAction<object> callAbilityAction = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsage, blockQueue: true);
                GameplayQueueAction<object> rankBuffAction = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.RankBuff);

                switch (card.Model.Card.Prototype.Kind)
                {
                    case Enumerators.CardKind.CREATURE:
                        {
                            card.FuturePositionOnBoard = 0;
                            float newCreatureCardPosition = card.Transform.position.x;

                            IReadOnlyList<BoardUnitView> cardsOnBoardViews = _battlegroundController.GetBoardUnitViewsFromModels(player.CardsOnBoard);

                            // set correct position on board depends from card view position
                            for (int i = 0; i < player.CardsOnBoard.Count; i++)
                            {
                                if (newCreatureCardPosition > cardsOnBoardViews[i].Transform.position.x)
                                {
                                    card.FuturePositionOnBoard = i + 1;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            BoardUnitView boardUnitView = new BoardUnitView(card.Model, PlayerBoard.transform);
                            boardUnitView.Transform.tag = SRTags.PlayerOwned;
                            boardUnitView.Transform.parent = PlayerBoard.transform;
                            boardUnitView.Transform.position = new Vector2(1.9f * player.CardsOnBoard.Count, 0);
                            boardUnitView.Model.Card.Owner = card.Model.Card.Owner;
                            boardUnitView.Model.Card.TutorialObjectId = card.Model.Card.TutorialObjectId;

                            player.PlayerCardsController.RemoveCardFromHand(card.Model, true);
                            _battlegroundController.RegisterBoardUnitView(player, boardUnitView, InternalTools.GetSafePositionToInsert(card.FuturePositionOnBoard, player.CardsOnBoard));
                            //player.BoardCards.Insert(InternalTools.GetSafePositionToInsert(card.FuturePositionOnBoard, player.BoardCards), boardUnitView);
                            player.PlayerCardsController.AddCardToBoard(card.Model, (ItemPosition)card.FuturePositionOnBoard);
                            _battlegroundController.PlayerHandCards.Remove(card);
                            _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                            InternalTools.DoActionDelayed(
                                     () =>
                                     {
                                         card.Model.Card.Owner.GraveyardCardsCount++;
                                     }, 1f);

                            card.RemoveCardParticle.Play();

                            _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitView.Model, false, _gameplayManager.CanDoDragActions);

                            if(Constants.RankSystemEnabled)
                                _ranksController.UpdateRanksByElements(boardUnitView.Model.OwnerPlayer.CardsOnBoard, boardUnitView.Model, rankBuffAction);

                            _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.CurrentPlayer,
                                () =>
                                {
                                    card.HandBoardCard.GameObject.SetActive(false);

                                    _abilitiesController.CallAbility(card, card.Model,
                                        Enumerators.CardKind.CREATURE, boardUnitView.Model, CallCardPlay, true, (status) =>
                                        {
                                            UpdateCardsStatusEvent?.Invoke(player);

                                            if (status)
                                            {
                                                player.ThrowPlayCardEvent(card.Model, card.FuturePositionOnBoard);
                                                OnPlayPlayerCard?.Invoke(new PlayCardOnBoard(boardUnitView, card.Model.CurrentCost));
                                            }
                                            else
                                            {
                                                rankBuffAction.Action = null;
                                                rankBuffAction.ForceActionDone();

                                                boardUnitView.DisposeGameObject();
                                                boardUnitView.Model.Die(true, isDead: false);

                                                _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.CurrentPlayer, null);
                                            }

                                        }, callAbilityAction, target, handCard, skipEntryAbilities);

                                    _actionsQueueController.ForceContinueAction(callAbilityAction);
                                });
                            boardUnitView.PlayArrivalAnimation(playUniqueAnimation: true);
                            break;
                        }
                    case Enumerators.CardKind.ITEM:
                        {
                            player.PlayerCardsController.RemoveCardFromHand(card.Model, true);
                            _battlegroundController.PlayerHandCards.Remove(card);
                            _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                            card.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.BoardCards;
                            card.GameObject.GetComponent<SortingGroup>().sortingOrder = 1000;

                            BoardItem boardItem = new BoardItem(card.GameObject, card.Model);

                            card.RemoveCardParticle.Play();

                            InternalTools.DoActionDelayed(() =>
                            {
                                _abilitiesController.CallAbility(card, card.Model,
                                    Enumerators.CardKind.ITEM, boardItem, CallItemCardPlay, true, (status) =>
                                    {
                                        if (status)
                                        {
                                            player.ThrowPlayCardEvent(card.Model, 0);
                                        }

                                        rankBuffAction.ForceActionDone();
                                    }, callAbilityAction, target, handCard, skipEntryAbilities);

                                _actionsQueueController.ForceContinueAction(callAbilityAction);
                            }, 0.75f);
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                card.HandBoardCard.ResetToInitialPosition();
            }
        }



        public void PlayOpponentCard(
            Player player,
            InstanceId cardId,
            BoardObject target,
            Action<BoardUnitModel> cardFoundCallback,
            Action<BoardUnitModel, BoardObject> completePlayCardCallback
            )
        {
            OpponentHandCard opponentHandCard;
            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP || _gameplayManager.IsTutorial)
            {
                opponentHandCard =
                    _battlegroundController.OpponentHandCards.FirstOrDefault(x => x.Model.InstanceId == cardId);
            }
            else
            {
                if (_battlegroundController.OpponentHandCards.Count <= 0)
                    return;

                opponentHandCard = _battlegroundController.OpponentHandCards.FirstOrDefault(x => x.Model.InstanceId == cardId);
            }

            if (opponentHandCard is null)
            {
                Exception exception = new Exception($"[Out of sync] not found card in opponent hand! card Id: {cardId.Id}");
                Helpers.ExceptionReporter.LogExceptionAsWarning(Log, exception);
                return;
            }

            BoardUnitModel card = opponentHandCard.Model;

            _battlegroundController.OpponentHandCards.Remove(opponentHandCard);
            player.PlayerCardsController.RemoveCardFromHand(card);
            cardFoundCallback?.Invoke(card);

            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.EnemyOverlordCardPlayedStarted);

            _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND,
                Constants.CardsMoveSoundVolume);

            SortingGroup group = opponentHandCard.Transform.GetComponent<SortingGroup>();
            group.sortingLayerID = SRSortingLayers.Foreground;
            group.sortingOrder = _battlegroundController.OpponentHandCards.FindIndex(x => x == opponentHandCard);
            List<GameObject> allUnitObj = opponentHandCard.Transform.GetComponentsInChildren<Transform>().Select(x => x.gameObject).ToList();
            foreach (GameObject child in allUnitObj)
            {
                child.layer = LayerMask.NameToLayer("Default");
            }

            opponentHandCard.Transform.DOMove(Vector3.up * 2.5f, 0.6f).OnComplete(
                () =>
                {
                    opponentHandCard.Transform.Find("RemoveCardParticle").GetComponent<ParticleSystem>().Play();

                    opponentHandCard.Transform.DOScale(Vector3.one * 1.2f, 0.6f).OnComplete(
                        () =>
                        {
                            RemoveOpponentCard(opponentHandCard);

                            _timerManager.AddTimer(
                                x =>
                                {
                                    if (_gameplayManager.IsGameEnded)
                                        return;

                                    completePlayCardCallback?.Invoke(card, target);
                                },
                                null,
                                0.1f);

                            _timerManager.AddTimer(
                                x =>
                                {
                                    player.GraveyardCardsCount++;
                                });
                        });
                });

            opponentHandCard.Transform.DORotate(Vector3.zero, 0.5f);

            _battlegroundController.UpdatePositionOfCardsInOpponentHand(true);
        }

        public void DrawCardInfo(BoardUnitModel boardUnitModel)
        {
            GameObject go;
            BoardCardView boardCardView;
            switch (boardUnitModel.Prototype.Kind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard"));
                    boardCardView = new UnitBoardCard(go, boardUnitModel);
                    break;
                case Enumerators.CardKind.ITEM:
                    go = Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard"));
                    boardCardView = new ItemBoardCard(go, boardUnitModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            go.transform.position = new Vector3(-6, 0, 0);
            go.transform.localScale = Vector3.one * .3f;
            boardCardView.SetHighlightingEnabled(false);

            Object.Destroy(go, 2f);
        }

        public BoardUnitModel LowGooCostOfCardInHand(Player player, BoardUnitModel boardUnitModel, int value)
        {
            if (boardUnitModel == null && player.CardsInHand.Count > 0)
            {
                boardUnitModel = player.CardsInHand[Random.Range(0, player.CardsInHand.Count)];
            }

            if (boardUnitModel == null)
                return boardUnitModel;

            if (player.IsLocalPlayer)
            {
                BoardCardView boardCardView = _battlegroundController.PlayerHandCards.First(x => x.Model.Card == boardUnitModel.Card);

                boardUnitModel.AddToCurrentCostHistory(value, Enumerators.ReasonForValueChange.AbilityBuff);
                boardCardView.UpdateCardCost();
            }
            else
            {
                boardUnitModel.AddToCurrentCostHistory(value, Enumerators.ReasonForValueChange.AbilityBuff);
            }

            player.PlayerCardsController.InvokeHandChanged();

            return boardUnitModel;
        }

        public ValueHistory SetGooCostOfCardInHand(Player player, BoardUnitModel boardUnitModel, int value, BoardCardView boardCardView = null, bool forced = false)
        {
            if (player.IsLocalPlayer)
            {
                boardUnitModel.AddToCurrentCostHistory(value, Enumerators.ReasonForValueChange.AbilityBuff, forced);

                if (boardCardView == null)
                {
                    boardCardView = _battlegroundController.PlayerHandCards.FirstOrDefault(x => x.Model == boardUnitModel);
                }

                if (boardCardView != null) 
                    {
                        boardCardView.UpdateCardCost();

                        bool isActive = boardCardView.Model.CurrentCost < boardCardView.Model.Card.Prototype.Cost;
                        boardCardView.costHighlightObject.SetActive(isActive);
                    }

                return boardUnitModel.FindFirstForcedValueInValueHistory(boardUnitModel.CurrentCostHistory);
            }
            else
            {
                boardUnitModel.AddToCurrentCostHistory(value, Enumerators.ReasonForValueChange.AbilityBuff, forced);
                boardUnitModel.Prototype = new Card(
                    boardUnitModel.Prototype.MouldId,
                    boardUnitModel.Prototype.Name,
                    boardUnitModel.Prototype.Cost,
                    boardUnitModel.Prototype.Description,
                    boardUnitModel.Prototype.FlavorText,
                    boardUnitModel.Prototype.Picture,
                    boardUnitModel.Prototype.Damage,
                    boardUnitModel.Prototype.Defense,
                    boardUnitModel.Prototype.Faction,
                    boardUnitModel.Prototype.Frame,
                    boardUnitModel.Prototype.Kind,
                    boardUnitModel.Prototype.Rank,
                    boardUnitModel.Prototype.Type,
                    boardUnitModel.Prototype.Abilities
                        .Select(a => new AbilityData(a))
                        .ToList(),
                    new PictureTransform(boardUnitModel.Prototype.PictureTransform),
                    boardUnitModel.Prototype.UniqueAnimation,
                    boardUnitModel.Prototype.Hidden
                );

                return boardUnitModel.FindFirstForcedValueInValueHistory(boardUnitModel.CurrentCostHistory);
            }
        }

        public Enumerators.Faction GetSetOfCard(IReadOnlyCard card)
        {
            Faction set =
                _dataManager.CachedCardsLibraryData.Factions.Find(x => x.Cards.Find(y => y.Name.Equals(card.Name)) != null);

            return set.Name;
        }

        public WorkingCard CreateWorkingCardFromCardName(string cardName, Player owner)
        {
            Card card = _dataManager.CachedCardsLibraryData.GetCardFromName(cardName);
            return new WorkingCard(card, card, owner);
        }

        private void GameEndedHandler(Enumerators.EndGameType obj)
        {
            CardDistribution = false;
        }

        private void GameStartedHandler()
        {
            _cardInstanceId = 0;

            PlayerBoard = GameObject.Find("PlayerBoard");
            OpponentBoard = GameObject.Find("OpponentBoard");
        }

        private void DirectlyEndCardDistribution(object[] param)
        {
            EndCardDistribution();
        }

        private void CallCardPlay(BoardCardView card)
        {
        }

        private void CallItemCardPlay(BoardCardView card)
        {
        }

        public void CreateChoosableCardsForAbilities(List<AbilityData.ChoosableAbility> choosableAbilities, BoardUnitModel card)
        {
            ResetChoosalbeCardsList();

            GameObject container = new GameObject("[Container]ChoosableAbiltiies");
            BoxCollider2D collider = container.AddComponent<BoxCollider2D>();
            SortingGroup group = container.AddComponent<SortingGroup>();

            _parentOfSelectableCards = container.transform;
            collider.size = Vector2.one * 100f;
            group.sortingOrder = 22;
            group.sortingLayerID = SRSortingLayers.GameUI3;

            foreach (AbilityData.ChoosableAbility ability in choosableAbilities)
            {
                _currentListOfChoosableCards.Add(new ChoosableCardForAbility(_parentOfSelectableCards, ability, card));
            }

            float offset = 3.25f;
            float spacing = 6.5f;
            float zOffset = -0.5f;
            float yOffset = 0f;

            InternalTools.GroupHorizontalObjects(_parentOfSelectableCards, offset, spacing, yOffset, offsetZ: zOffset);

            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            _gameplayManager.CanDoDragActions = false;
        }

        public void ResetChoosalbeCardsList()
        {
            if (_currentListOfChoosableCards != null)
            {
                foreach (ChoosableCardForAbility card in _currentListOfChoosableCards)
                {
                    card.Dispose();
                }
                _currentListOfChoosableCards.Clear();
            }
            else
            {
                _currentListOfChoosableCards = new List<ChoosableCardForAbility>();
            }

            if (_parentOfSelectableCards != null && _parentOfSelectableCards)
            {
                Object.Destroy(_parentOfSelectableCards.gameObject);
            }
        }

        public void ChooseAbilityOfCard(AbilityData.ChoosableAbility choosableAbility)
        {
            ResetChoosalbeCardsList();

            GameClient.Get<ICameraManager>().FadeOut(null, 1);

            _gameplayManager.CanDoDragActions = true;

            CardForAbilityChoosed?.Invoke(choosableAbility);
        }

        public void DiscardCardFromHand(BoardUnitModel boardUnitModel)
        {
            if(boardUnitModel.Owner.IsLocalPlayer)
            {
                BoardCardView card = _battlegroundController.GetBoardUnitViewByModel<BoardCardView>(boardUnitModel);

                if (card != null && card != default(BoardCardView))
                {
                    if (_battlegroundController.PlayerHandCards.Contains(card))
                    {
                        _battlegroundController.PlayerHandCards.Remove(card);
                    }
                    card.Dispose();
                }
            }
            else
            {
                OpponentHandCard card = _battlegroundController.GetBoardUnitViewByModel<OpponentHandCard>(boardUnitModel);

                if (card != null && card != default(OpponentHandCard))
                {
                    if (_battlegroundController.OpponentHandCards.Contains(card))
                    {
                        _battlegroundController.OpponentHandCards.Remove(card);
                    }
                    card.Dispose();
                }
            }

            boardUnitModel.Owner.PlayerCardsController.RemoveCardFromHand(boardUnitModel);
        }
    }


    public class ChoosableCardForAbility
    {
        private ILoadObjectsManager _loadObjectsManager;
        private AbilitiesController _abilitiesController;
        private CardsController _cardsController;

        public GameObject SelfObject { get; }

        private SpriteRenderer _picture;
        private SpriteRenderer _frame;
        private SpriteRenderer _unitType;

        private TextMeshPro _gooCostText;
        private TextMeshPro _titleText;
        private TextMeshPro _descriptionText;
        private TextMeshPro _attackText;
        private TextMeshPro _defenseText;

        private OnBehaviourHandler _behaviourHandler;
        private AbilityData.ChoosableAbility _mainChoosableAbility;

        public ChoosableCardForAbility(Transform parent, AbilityData.ChoosableAbility choosableAbility, BoardUnitModel boardUnitModel)
        {
            _mainChoosableAbility = choosableAbility;

            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _abilitiesController = GameClient.Get<IGameplayManager>().GetController<AbilitiesController>();
            _cardsController = GameClient.Get<IGameplayManager>().GetController<CardsController>();

            string prefabName = boardUnitModel.Prototype.Kind == Enumerators.CardKind.CREATURE ? "Card_BoardUnit" : "Card_Item";

            SelfObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ForChooseAbilities/" + prefabName),
                                            parent,
                                            false);

            _picture = SelfObject.transform.Find("Image_Picture").GetComponent<SpriteRenderer>();
            _frame = SelfObject.transform.Find("Image_Frame").GetComponent<SpriteRenderer>();

            _gooCostText = SelfObject.transform.Find("Text_GooCost").GetComponent<TextMeshPro>();
            _titleText = SelfObject.transform.Find("Text_Title").GetComponent<TextMeshPro>();
            _descriptionText = SelfObject.transform.Find("Text_Description").GetComponent<TextMeshPro>();

            _behaviourHandler = SelfObject.GetComponent<OnBehaviourHandler>();
            _behaviourHandler.MouseUpTriggered += MouseUpTriggered;

            string setName = boardUnitModel.Prototype.Faction.ToString();
            string rarity = Enum.GetName(typeof(Enumerators.CardRank), boardUnitModel.Prototype.Rank);
            string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

            if (!string.IsNullOrEmpty(boardUnitModel.Prototype.Frame))
            {
                frameName = "Images/Cards/Frames/" + boardUnitModel.Prototype.Frame;
            }

            _frame.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);

            string imagePath = $"{Constants.PathToCardsIllustrations}{boardUnitModel.Prototype.Picture.ToLowerInvariant()}";

            if (!string.IsNullOrEmpty(_mainChoosableAbility.Attribute))
            {
                imagePath += $"_{ _mainChoosableAbility.Attribute}";
            }

            _picture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(imagePath);

            _titleText.text = boardUnitModel.Prototype.Name;
            _descriptionText.text = choosableAbility.Description;
            _gooCostText.text = boardUnitModel.CurrentCost.ToString();

            if (boardUnitModel.Prototype.Kind == Enumerators.CardKind.CREATURE)
            {
                _unitType = SelfObject.transform.Find("Image_Type").GetComponent<SpriteRenderer>();

                _attackText = SelfObject.transform.Find("Text_Attack").GetComponent<TextMeshPro>();
                _defenseText = SelfObject.transform.Find("Text_Defense").GetComponent<TextMeshPro>();

                _attackText.text = boardUnitModel.Prototype.Damage.ToString();
                _defenseText.text = boardUnitModel.Prototype.Defense.ToString();

                _unitType.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/{0}", boardUnitModel.InstanceCard.CardType + "_icon"));
            }
        }

        public void Dispose()
        {
            Object.Destroy(SelfObject);
        }

        private void MouseUpTriggered(GameObject gameObject)
        {
            _cardsController.ChooseAbilityOfCard(_mainChoosableAbility);
        }
    }
}
