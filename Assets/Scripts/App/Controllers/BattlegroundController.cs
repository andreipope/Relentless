using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DG.Tweening;
using KellermanSoftware.CompareNetObjects;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Card = Loom.ZombieBattleground.Data.Card;
using InstanceId = Loom.ZombieBattleground.Data.InstanceId;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class BattlegroundController : IController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(BattlegroundController));

        public bool IsPreviewActive;

        public bool CardsZoomed = false;

        public Coroutine CreatePreviewCoroutine;

        public GameObject CurrentBoardCard;

        public InstanceId CurrentPreviewedCardId;

        public int CurrentTurn;

        public GameObject PlayerBoardObject, OpponentBoardObject, PlayerGraveyardObject, OpponentGraveyardObject;

        private AIController _aiController;

        private bool _battleDynamic;

        private CardsController _cardsController;

        private SkillsController _skillsController;

        private IDataManager _dataManager;

        private IGameplayManager _gameplayManager;

        private BoardUnitView _lastBoardUntilOnPreview;

        private PlayerController _playerController;

        private VfxController _vfxController;

        private AbilitiesController _abilitiesController;

        private ActionsQueueController _actionsQueueController;

        private BoardController _boardController;

        private IPlayerManager _playerManager;

        private ISoundManager _soundManager;

        private ITimerManager _timerManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private IPvPManager _pvpManager;

        private IMatchManager _matchManager;

        private Sequence _rearrangingTopRealTimeSequence, _rearrangingBottomRealTimeSequence;

        private bool _turnTimerCounting = false;

        private GameObject _endTurnRingsAnimationGameObject;

        private Animator _endTurnButtonAnimationAnimator;

        private ParticleSystem[] _endTurnRingsAnimationParticleSystems;

        public event Action<int> PlayerGraveyardUpdated;

        public event Action<int> OpponentGraveyardUpdated;

        public event Action TurnStarted;

        public event Action TurnEnded;

        public float TurnTimer { get; private set; }

        public bool IsOnShorterTime = false;

        public bool TurnWaitingForEnd { get; private set; }

        public IReadOnlyList<ICardView> CardViews => _cardViews;

        private readonly UniqueList<ICardView> _cardViews = new UniqueList<ICardView>();

        public T GetCardViewByModel<T>(CardModel cardModel) where T : ICardView
        {
            if (cardModel == null)
            {
                Log.Warn("GetCardViewByModel: Input cardModel == null, returning null");
                return default(T);
                //throw new ArgumentNullException(nameof(cardModel));
            }

            T view =
                _cardViews
                    .OfType<T>()
                    .Where(v => v.Model == cardModel)
                    .SingleOrDefault();

            if (view == null)
            {
                Log.Warn($"GetCardViewByModel: View of type {typeof(T).Name} not found for model {cardModel}");
                //throw new Exception($"No view found for model {cardModel}");
            }


            return view;
        }

        public IReadOnlyList<T> GetCardViewsByModels<T>(IReadOnlyList<CardModel> models) where T : ICardView
        {
            return models.Select(GetCardViewByModel<T>).ToList();
        }

        public void RegisterCardView(ICardView view, Player player = null, ItemPosition position = default(ItemPosition))
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            Log.Info($"RegisterBoardUnitView(IBoardUnitView view == {view})");
            if (_cardViews.Contains(view))
            {
                Log.Warn($"{nameof(RegisterCardView)}: Attempt to add card view {view} to BoardUnitViews when it is already added");
                return;
            }

            _cardViews.Add(view);
        }

        public void UnregisterCardView(ICardView view, Player player = null)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            Log.Info($"UnregisterBoardUnitView(IBoardUnitView view == {view})");
            bool removed = _cardViews.Remove(view);
            if (!removed)
            {
                Log.Info($"UnregisterBoardUnitView: attempted to unregister non-registered view {view}");
            }
        }


        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _pvpManager = GameClient.Get<IPvPManager>();
            _matchManager = GameClient.Get<IMatchManager>();

            _playerController = _gameplayManager.GetController<PlayerController>();
            _vfxController = _gameplayManager.GetController<VfxController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _aiController = _gameplayManager.GetController<AIController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _boardController = _gameplayManager.GetController<BoardController>();

            _gameplayManager.GameEnded += GameEndedHandler;

            _gameplayManager.GameInitialized += OnGameInitializedHandler;

            _pvpManager.EndTurnActionReceived += OnGetEndTurnHandler;
        }

        private void OnGetEndTurnHandler(GameState controlGameState)
        {
            StopTurn(controlGameState);
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            if (_gameplayManager.IsGameStarted && !_gameplayManager.IsGameEnded)
            {
                CheckGameDynamic();

                foreach (BoardUnitView item in GetCardViewsByModels<BoardUnitView>(_gameplayManager.CurrentPlayer.CardsOnBoard))
                {
                    item.Update();
                }

                foreach (BoardUnitView item in GetCardViewsByModels<BoardUnitView>(_gameplayManager.OpponentPlayer.CardsOnBoard))
                {
                    item.Update();
                }

                if (_matchManager.MatchType == Enumerators.MatchType.PVP)
                {
                    if (!_tutorialManager.IsTutorial && _turnTimerCounting)
                    {
                        TurnTimer -= Time.unscaledDeltaTime;

                        if (TurnTimer <= 0)
                        {
                            StopTurn(turnEndTimeout:true);
                        }
                        else if (TurnTimer <= Constants.TimeForStartEndTurnAnimation && !_endTurnRingsAnimationGameObject.activeInHierarchy)
                        {
                            _endTurnRingsAnimationGameObject.SetActive(true);
                            _endTurnButtonAnimationAnimator.enabled = true;
                            _endTurnButtonAnimationAnimator.Play("TurnTimer");

                            float speed = Constants.TimeForStartEndTurnAnimation / TurnTimer;
                            foreach (ParticleSystem particleSystem in _endTurnRingsAnimationParticleSystems)
                            {
                                ParticleSystem.MainModule mainModule = particleSystem.main;
                                mainModule.simulationSpeed = speed;
                            }
                        }
                    }
                }
            }
        }

        public void ResetAll()
        {
            if (CreatePreviewCoroutine != null)
            {
                MainApp.Instance.StopCoroutine(CreatePreviewCoroutine);
            }

            CreatePreviewCoroutine = null;

            if (CurrentBoardCard != null && CurrentBoardCard)
            {
                Object.Destroy(CurrentBoardCard);
            }

            CurrentBoardCard = null;

            ClearBattleground();
        }

        public void KillBoardCard(CardModel cardModel, bool withDeathEffect = true, bool updateBoard = true)
        {
            BoardUnitView boardUnitView = GetCardViewByModel<BoardUnitView>(cardModel);

            if (boardUnitView == null)
                return;

            if (_lastBoardUntilOnPreview != null && boardUnitView == _lastBoardUntilOnPreview)
            {
                DestroyCardPreview();
            }

            Action completeCallback = () => { };

            boardUnitView.Transform.position = new Vector3(boardUnitView.Transform.position.x,
                boardUnitView.Transform.position.y, boardUnitView.Transform.position.z + 0.2f);

            InternalTools.DoActionDelayed(() =>
            {
                Action endOfDestroyAnimationCallback = () =>
                {
                    boardUnitView.GameObject.SetActive(false);

                    boardUnitView.WasDestroyed = true;
                };

                Action endOfAnimationCallback = () =>
                {
                    boardUnitView.Dispose();
                    cardModel.OwnerPlayer.PlayerCardsController.RemoveCardFromBoard(cardModel);
                    cardModel.OwnerPlayer.PlayerCardsController.AddCardToGraveyard(cardModel);

                    boardUnitView.Model.InvokeUnitDied();

                    if (_tutorialManager.IsTutorial)
                    {
                        if (cardModel.OwnerPlayer.IsLocalPlayer)
                        {
                            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerBattleframeDied);
                        }
                        else
                        {
                            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.EnemyBattleframeDied);
                        }
                    }
                };

                if (withDeathEffect)
                {
                    _vfxController.CreateDeathZombieAnimation(boardUnitView, endOfDestroyAnimationCallback, endOfAnimationCallback, completeCallback);
                }
                else
                {
                    endOfDestroyAnimationCallback();
                    endOfAnimationCallback();

                    if (updateBoard)
                    {
                        _boardController.UpdateWholeBoard(null);
                    }

                    completeCallback?.Invoke();
                }

            }, Time.deltaTime * 60f / 2f);

            _actionsQueueController.ForceContinueAction(boardUnitView.Model.ActionForDying);
        }

        public void CheckGameDynamic()
        {
            if (!_battleDynamic)
            {
                _soundManager.CrossfaidSound(Enumerators.SoundType.BATTLEGROUND, null, true);
            }

            _battleDynamic = true;
        }

        public void UpdateGraveyard(int index, Player player)
        {
            if (player.IsLocalPlayer)
            {
                PlayerGraveyardUpdated?.Invoke(index);
            }
            else
            {
                OpponentGraveyardUpdated?.Invoke(index);
            }
        }

        public void ClearBattleground()
        {
            _gameplayManager.CurrentPlayer?.PlayerCardsController.ClearCardsInHand();
            _gameplayManager.OpponentPlayer?.PlayerCardsController.ClearCardsInHand();

            _gameplayManager.CurrentPlayer?.PlayerCardsController.ClearCardsOnBoard();
            _gameplayManager.OpponentPlayer?.PlayerCardsController.ClearCardsOnBoard();

            _cardViews.Clear();
        }

        public void InitializeBattleground()
        {
            CurrentTurn = 0;
            TurnWaitingForEnd = false;

            if (Constants.DevModeEnabled)
            {
                _gameplayManager.OpponentPlayer.Defense = 99;
                _gameplayManager.CurrentPlayer.Defense = 99;
            }

            PlayerBoardObject = GameObject.Find("PlayerBoard");
            OpponentBoardObject = GameObject.Find("OpponentBoard");
            PlayerGraveyardObject = GameObject.Find("GraveyardPlayer");
            OpponentGraveyardObject = GameObject.Find("GraveyardOpponent");

            TurnTimer = 0f;
            _turnTimerCounting = false;
            IsOnShorterTime = false;

            _endTurnButtonAnimationAnimator = GameObject.Find("EndTurnButton/_1_btn_endturn").GetComponent<Animator>();
            _endTurnRingsAnimationGameObject = GameObject.Find("EndTurnButton").transform.Find("ZB_ANM_TurnTimerEffect").gameObject;
            _endTurnRingsAnimationParticleSystems = _endTurnRingsAnimationGameObject.gameObject.GetComponentsInChildren<ParticleSystem>();
            _endTurnRingsAnimationGameObject.SetActive(false);
            _endTurnButtonAnimationAnimator.enabled = false;
        }

        public void StartGameplayTurns()
        {
            if (_gameplayManager.IsTutorial &&
               _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.GameplayBeginManually &&
               !_tutorialManager.CurrentTutorialStep.ToGameplayStep().LaunchGameplayManually)
                return;

            StartTurn();
        }

        public void GameEndedHandler(Enumerators.EndGameType endGameType)
        {
            CurrentTurn = 0;

            ClearBattleground();
        }

        public void StartTurn()
        {
            if (_gameplayManager.IsGameEnded)
                return;

            if (!_tutorialManager.IsTutorial &&
                _matchManager.MatchType == Enumerators.MatchType.PVP &&
                !_turnTimerCounting &&
                _gameplayManager.CurrentTurnPlayer.IsLocalPlayer)
            {
                if (IsOnShorterTime)
                {
                    TurnTimer = Constants.ShortTurnTime;
                }
                else
                {
                    TurnTimer = _gameplayManager.CurrentTurnPlayer.TurnTime;
                }
                _turnTimerCounting = true;
            }

            CurrentTurn++;

            _gameplayManager.CurrentTurnPlayer.Turn++;

            _uiManager.GetPage<GameplayPage>().SetEndTurnButtonStatus(_gameplayManager.IsLocalPlayerTurn());

            UpdatePositionOfCardsInOpponentHand();
            _playerController.IsActive = _gameplayManager.IsLocalPlayerTurn();

            IReadOnlyList<BoardUnitView> currentPlayerCardsOnBoardUnitViews = GetCardViewsByModels<BoardUnitView>(_gameplayManager.CurrentPlayer.CardsOnBoard);
            IReadOnlyList<BoardUnitView> opponentPlayerCardsOnBoardUnitViews = GetCardViewsByModels<BoardUnitView>(_gameplayManager.OpponentPlayer.CardsOnBoard);
            IReadOnlyList<BoardCardView> currentPlayerCardsInHandUnitViews = GetCardViewsByModels<BoardCardView>(_gameplayManager.CurrentPlayer.CardsInHand);

            if (_gameplayManager.IsLocalPlayerTurn())
            {
                List<BoardUnitView> creatures = new List<BoardUnitView>();

                foreach (BoardUnitView card in currentPlayerCardsOnBoardUnitViews)
                {
                    if (_playerController == null || !card.GameObject)
                    {
                        creatures.Add(card);
                        continue;
                    }

                    card.Model.OnStartTurn();
                }

                foreach (BoardUnitView item in creatures)
                {
                    UnregisterCardView(item, _gameplayManager.CurrentPlayer);
                }

                creatures.Clear();

                foreach (BoardUnitView card in currentPlayerCardsOnBoardUnitViews)
                {
                    card.SetHighlightingEnabled(true);
                }

                foreach (BoardUnitView card in opponentPlayerCardsOnBoardUnitViews)
                {
                    card.SetHighlightingEnabled(false);
                }
            }
            else
            {
                foreach (BoardUnitView card in opponentPlayerCardsOnBoardUnitViews)
                {
                    card.Model.OnStartTurn();
                }

                foreach (BoardCardView card in currentPlayerCardsInHandUnitViews)
                {
                    card.SetHighlightingEnabled(false);
                }

                foreach (BoardUnitView card in currentPlayerCardsOnBoardUnitViews)
                {
                    card.SetHighlightingEnabled(false);
                }
            }

            _gameplayManager.CurrentPlayer.InvokeTurnStarted();
            if (_gameplayManager.CurrentPlayer == null)
            {
                return;
            }
            _gameplayManager.OpponentPlayer.InvokeTurnStarted();
            if (_gameplayManager.OpponentPlayer == null)
            {
                return;
            }

            _playerController.UpdateHandCardsHighlight();

            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.StartTurn);

            TurnStarted?.Invoke();
        }

        private void EndTurnPart1Prepare()
        {
            if (_gameplayManager.IsGameEnded)
                return;

            if (!_tutorialManager.IsTutorial &&
                _matchManager.MatchType == Enumerators.MatchType.PVP && _turnTimerCounting)
            {
                _turnTimerCounting = false;

                if (_endTurnRingsAnimationGameObject.activeInHierarchy)
                {
                    _endTurnRingsAnimationGameObject.SetActive(false);
                }

                _endTurnButtonAnimationAnimator.enabled = false;
            }

            if (_gameplayManager.IsLocalPlayerTurn())
            {
                _uiManager.GetPage<GameplayPage>().SetEndTurnButtonStatus(false);

                foreach (CardModel card in _gameplayManager.CurrentPlayer.CardsOnBoard)
                {
                    card.OnEndTurn();
                }
            }
            else
            {
                foreach (CardModel card in _gameplayManager.OpponentPlayer.CardsOnBoard)
                {
                    card.OnEndTurn();
                }
            }
        }

        private void EndTurnPart2InvokePlayerTurnEnded()
        {
            if (_gameplayManager.IsGameEnded)
                return;
            
            _gameplayManager.CurrentPlayer.InvokeTurnEnded();
            _gameplayManager.OpponentPlayer.InvokeTurnEnded();
        }

        private void EndTurnPart3Finish()
        {
            if (_gameplayManager.IsGameEnded)
                return;
            
            if (_gameplayManager.IsLocalPlayerTurn())
            {
                TurnEnded?.Invoke();
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.EndTurn);
            }

            _gameplayManager.CurrentTurnPlayer = _gameplayManager.IsLocalPlayerTurn() ?
                _gameplayManager.OpponentPlayer :
                _gameplayManager.CurrentPlayer;
        }

        public void StopTurn(GameState pvpControlGameState = null, bool turnEndTimeout = false)
        {
            if (TurnWaitingForEnd)
                return;
            
            if (turnEndTimeout)
            {
                IsOnShorterTime = true;
            }

            TurnWaitingForEnd = true;

            _actionsQueueController.EnqueueAction(
                completeCallback =>
                {
                    float delay = (!_tutorialManager.IsTutorial && _matchManager.MatchType == Enumerators.MatchType.PVP) ? 2 : 0;
                    InternalTools.DoActionDelayed(() =>
                    {
                        ValidateGameState(pvpControlGameState);
                        EndTurnPart1Prepare();
                        completeCallback.Invoke();
                    }, delay);
                }, Enumerators.QueueActionType.StopTurnPart1Prepare, startupTime:1f);

            _actionsQueueController.EnqueueAction(
                completeCallback =>
                {
                    EndTurnPart2InvokePlayerTurnEnded();
                    completeCallback.Invoke();
                }, Enumerators.QueueActionType.StopTurnPart2InvokePlayerTurnEnded);

            _actionsQueueController.EnqueueAction(
                completeCallback =>
                {
                    TurnWaitingForEnd = false;
                    EndTurnPart3Finish();
                    if (_gameplayManager.IsLocalPlayerTurn())
                    {
                        _uiManager.DrawPopup<YourTurnPopup>();

                        YourTurnPopup yourTurnPopup = _uiManager.GetPopup<YourTurnPopup>();
                        
                        //We unregister from any possible previous event
                        //This should be the only handler that runs
                        if (yourTurnPopup.OnPopupHide != null)
                        {
                            foreach (Delegate d in yourTurnPopup.OnPopupHide.GetInvocationList())
                            {
                                yourTurnPopup.OnPopupHide -= (Action)d;
                            }
                        }
                        
                        yourTurnPopup.OnPopupHide += () => {
                            StartTurn();
                            completeCallback?.Invoke();
                        };
                    }
                    else
                    {
                        StartTurn();
                        completeCallback?.Invoke();
                    }
                }, Enumerators.QueueActionType.StopTurnPart3Finish, startupTime:1f);
        }

        public void RemovePlayerCardFromBoardToGraveyard(CardModel cardModel)
        {
            BoardUnitView boardCardView = GetCardViewByModel<BoardUnitView>(cardModel);
            if (boardCardView == null)
                return;

            if (!boardCardView.WasDestroyed)
            {
                boardCardView.Transform.localPosition = new Vector3(boardCardView.Transform.localPosition.x,
                boardCardView.Transform.localPosition.y, -0.2f);
            }

            UnregisterCardView(boardCardView, _gameplayManager.CurrentPlayer);

            boardCardView.SetHighlightingEnabled(false);
            boardCardView.StopSleepingParticles();

            if (!boardCardView.WasDestroyed)
            {
                boardCardView.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.BoardCards;
                Object.Destroy(boardCardView.GameObject.GetComponent<BoxCollider2D>());
            }
        }

        public void RemoveOpponentCardFromBoardToGraveyard(CardModel cardModel)
        {
            Vector3 graveyardPos = OpponentGraveyardObject.transform.position + new Vector3(0.0f, -0.2f, 0.0f);
            BoardUnitView boardCardView = GetCardViewByModel<BoardUnitView>(cardModel);
            if (boardCardView != null)
            {
                if (!boardCardView.WasDestroyed)
                {
                    boardCardView.Transform.localPosition = new Vector3(boardCardView.Transform.localPosition.x,
                        boardCardView.Transform.localPosition.y, -0.2f);
                }

                UnregisterCardView(boardCardView, _gameplayManager.OpponentPlayer);

                boardCardView.SetHighlightingEnabled(false);
                boardCardView.StopSleepingParticles();

                if (!boardCardView.WasDestroyed)
                {
                    boardCardView.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.BoardCards;
                    Object.Destroy(boardCardView.GameObject.GetComponent<BoxCollider2D>());
                }
            }
            else if (_aiController.CurrentItemCard != null && cardModel == _aiController.CurrentItemCard.Model)
            {
                _aiController.CurrentItemCard.SetHighlightingEnabled(false);
                _aiController.CurrentItemCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.BoardCards;
                Object.Destroy(_aiController.CurrentItemCard.GameObject.GetComponent<BoxCollider2D>());
                Sequence sequence = DOTween.Sequence();
                sequence.PrependInterval(2.0f);
                sequence.Append(_aiController.CurrentItemCard.Transform.DOMove(graveyardPos, 0.5f));
                sequence.Append(_aiController.CurrentItemCard.Transform.DOScale(new Vector2(0.6f, 0.6f), 0.5f));
                sequence.OnComplete(
                    () =>
                    {
                        _aiController.CurrentItemCard = null;
                    });
            }
        }

        // rewrite
        public void CreateCardPreview(ICardView target, Vector3 pos, bool highlight = true)
        {
            IsPreviewActive = true;
            CurrentPreviewedCardId = target.Model.Card.InstanceId;

            switch (target)
            {
                case BoardCardView _:
                    break;
                case BoardUnitView unit:
                    _lastBoardUntilOnPreview = unit;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }

            CreatePreviewCoroutine = MainApp.Instance.StartCoroutine(CreateCardPreviewAsync(target, pos, highlight));
        }

        // rewrite
        public IEnumerator CreateCardPreviewAsync(ICardView target, Vector3 pos, bool highlight)
        {
            yield return new WaitForSeconds(0.3f);

            CardModel cardModel = target.Model;
            CardModel previewModel = new CardModel(new WorkingCard(cardModel.Prototype, cardModel.Prototype, cardModel.OwnerPlayer, cardModel.InstanceId));
            BoardCardView boardCardView;
            switch (cardModel.Prototype.Kind)
            {
                case Enumerators.CardKind.CREATURE:
                    CurrentBoardCard = Object.Instantiate(_cardsController.CreatureCardViewPrefab);
                    boardCardView = new UnitBoardCardView(CurrentBoardCard, previewModel);
                    ((UnitBoardCardView) boardCardView).DrawOriginalStats();
                    break;
                case Enumerators.CardKind.ITEM:
                    CurrentBoardCard = Object.Instantiate(_cardsController.ItemCardViewPrefab);
                    boardCardView = new ItemBoardCardView(CurrentBoardCard, previewModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (highlight)
            {
                highlight = boardCardView.Model.CanBePlayed(target.Model.OwnerPlayer) && boardCardView.Model.CanBeBuyed(target.Model.OwnerPlayer);
            }

            boardCardView.SetHighlightingEnabled(highlight);
            boardCardView.IsPreview = true;

            InternalTools.SetLayerRecursively(boardCardView.GameObject, 0);

            switch (target)
            {
                case BoardUnitView boardUnit:
                    boardCardView.DrawTooltipInfoOfUnit(boardUnit);
                    break;
                case BoardCardView tooltipCard:
                    boardCardView.DrawTooltipInfoOfCard(tooltipCard);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }

            Vector3 newPos = pos;
            newPos.y += 2.0f;
            CurrentBoardCard.transform.position = newPos;
            CurrentBoardCard.transform.localRotation = Quaternion.Euler(Vector3.zero);

            Vector3 sizeOfCard = Vector3.one;

            sizeOfCard = !InternalTools.IsTabletScreen() ? new Vector3(.8f, .8f, .8f) : new Vector3(.4f, .4f, .4f);

            CurrentBoardCard.transform.localScale = sizeOfCard;

            CurrentBoardCard.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameplayInfo;
            CurrentBoardCard.layer = LayerMask.NameToLayer("Default");
            CurrentBoardCard.transform.DOMoveY(newPos.y + 1.0f, 0.1f);
        }

        // rewrite
        public void DestroyCardPreview()
        {
            if (!IsPreviewActive)
                return;

            GameClient.Get<ICameraManager>().FadeOut(null, 1, true);

            MainApp.Instance.StartCoroutine(DestroyCardPreviewAsync());
            if (CreatePreviewCoroutine != null)
            {
                MainApp.Instance.StopCoroutine(CreatePreviewCoroutine);
            }

            IsPreviewActive = false;
        }

        // rewrite
        public IEnumerator DestroyCardPreviewAsync()
        {
            if (CurrentBoardCard != null)
            {
                _lastBoardUntilOnPreview = null;
                GameObject oldCardPreview = CurrentBoardCard;
                foreach (SpriteRenderer renderer in oldCardPreview.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.DOFade(0.0f, 0.2f);
                }

                foreach (TextMeshPro text in oldCardPreview.GetComponentsInChildren<TextMeshPro>())
                {
                    text.DOFade(0.0f, 0.2f);
                }

                yield return new WaitForSeconds(0.5f);
                Object.Destroy(oldCardPreview.gameObject);
            }
        }

        public void UpdatePositionOfCardsInPlayerHand(bool isMove = false)
        {
            IReadOnlyList<BoardCardView> boardCardViews = GetCardViewsByModels<BoardCardView>(_gameplayManager.CurrentPlayer.CardsInHand);

            float handWidth = 0.0f;
            float spacing = -1.5f;
            float scaling = 0.25f;
            Vector3 pivot = new Vector3(6f, -7.5f, 0f);
            int twistPerCard = -5;

            if (CardsZoomed)
            {
                spacing = -2.6f;
                scaling = 0.31f;
                pivot = new Vector3(-1.3f, -6.5f, 0f);
                twistPerCard = -3;
            }

            for (int i = 0; i < boardCardViews.Count; i++)
            {
                handWidth += spacing;
            }

            handWidth -= spacing;

            if (boardCardViews.Count == 1)
            {
                twistPerCard = 0;
            }

            int totalTwist = twistPerCard * boardCardViews.Count;
            float startTwist = (totalTwist - twistPerCard) / 2f;
            float scalingFactor = 0.04f;
            Vector3 moveToPosition = Vector3.zero;

            for (int i = 0; i < boardCardViews.Count; i++)
            {
                BoardCardView card = boardCardViews[i];
                float twist = startTwist - i * twistPerCard;
                float nudge = Mathf.Abs(twist);

                nudge *= scalingFactor;
                moveToPosition = new Vector3(pivot.x - handWidth / 2, pivot.y - nudge,
                    (boardCardViews.Count - i) * 0.1f);

                if (isMove)
                {
                    card.IsNewCard = false;
                }

                card.UpdateCardPositionInHand(moveToPosition, Vector3.forward * twist, Vector3.one * scaling);

                pivot.x += handWidth / boardCardViews.Count;

                card.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.HandCards;
                card.GameObject.GetComponent<SortingGroup>().sortingOrder = i;
            }
        }

        public void UpdatePositionOfCardsInOpponentHand(bool isMove = false, bool isNewCard = false)
        {
            IReadOnlyList<OpponentHandCardView> boardCardViews = GetCardViewsByModels<OpponentHandCardView>(_gameplayManager.OpponentPlayer.CardsInHand);

            float handWidth = 0.0f;
            float spacing = -1.0f;
            float zPositionKoef = -0.1f;

            for (int i = 0; i < boardCardViews.Count; i++)
            {
                handWidth += spacing;
            }

            handWidth -= spacing;

            Vector3 pivot = new Vector3(-3.2f, 8.5f, 0f);
            int twistPerCard = 5;

            if (boardCardViews.Count == 1)
            {
                twistPerCard = 0;
            }

            int totalTwist = twistPerCard * boardCardViews.Count;
            float startTwist = (totalTwist - twistPerCard) / 2f;

            for (int i = 0; i < boardCardViews.Count; i++)
            {
                OpponentHandCardView card = boardCardViews[i];
                float twist = startTwist - i * twistPerCard;

                Vector3 movePosition = new Vector3(pivot.x - handWidth / 2, pivot.y, i * zPositionKoef);
                Vector3 rotatePosition = new Vector3(0, 0, twist);

                if (isMove)
                {
                    if (i == boardCardViews.Count - 1 && isNewCard)
                    {
                        card.Transform.position = new Vector3(-8.2f, 5.7f, 0);
                        card.Transform.eulerAngles = Vector3.forward * 90f;
                    }

                    card.Transform.DOMove(movePosition, 0.5f).OnComplete(() => UpdateOpponentHandCardLayer(card.GameObject));
                    card.Transform.DORotate(rotatePosition, 0.5f);
                }
                else
                {
                    card.Transform.position = movePosition;
                    card.Transform.rotation = Quaternion.Euler(rotatePosition);
                    UpdateOpponentHandCardLayer(card.GameObject);
                }

                pivot.x += handWidth / boardCardViews.Count;
            }
        }

        private void UpdateOpponentHandCardLayer(GameObject card)
        {
            if (card.layer == 0)
            {
                SortingGroup group = card.GetComponent<SortingGroup>();
                group.sortingLayerID = SRSortingLayers.Default;
                group.sortingOrder = -1;
                List<GameObject> allUnitObj = card.GetComponentsInChildren<Transform>().Select(x => x.gameObject).ToList();
                foreach (GameObject child in allUnitObj)
                {
                    child.layer = LayerMask.NameToLayer("Battleground");
                }
            }
        }

        public BoardCardView GetBoardCardFromHisObject(GameObject cardObject)
        {
            return _cardViews
                .OfType<BoardCardView>()
                .FirstOrDefault(view => view.GameObject == cardObject);
        }

        public void DestroyBoardUnit(CardModel unit,
                                    bool withDeathEffect = true,
                                    bool isForceDestroy = false,
                                    bool handleShield = false,
                                    bool updateBoard = true)
        {
            if (!isForceDestroy && unit.HasBuffShield && handleShield)
            {
                unit.HasUsedBuffShield = true;
                unit.ResolveBuffShield();
            }
            else
            {
                _gameplayManager.GetController<BattleController>().CheckOnKillEnemyZombie(unit);

                unit?.Die(withDeathEffect: withDeathEffect, updateBoard: updateBoard);
            }
        }

        public void TakeControlUnit(Player newPlayerOwner, CardModel unit)
        {
            BoardUnitView view = GetCardViewByModel<BoardUnitView>(unit);

            UnregisterCardView(view, unit.OwnerPlayer);
            newPlayerOwner.PlayerCardsController.TakeControlOfUnit(unit);
            RegisterCardView(view, newPlayerOwner);

            view.Transform.tag = newPlayerOwner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;

            _boardController.UpdateWholeBoard(null);

            foreach (AbilityBase ability in _abilitiesController.GetAbilitiesConnectedToUnit(unit))
            {
                ability.ChangePlayerCallerOfAbility(newPlayerOwner);
            }
        }

        public void DistractUnit(CardModel card)
        {
            card.BuffedDamage = 0;
            card.BuffedDefense = 0;
            card.DisableBuffsOnValueHistory(card.CurrentDamageHistory);
            card.DisableBuffsOnValueHistory(card.CurrentDefenseHistory);
            card.HasSwing = false;
            card.TakeFreezeToAttacked = false;
            card.HasBuffRush = false;
            card.HasBuffHeavy = false;
            card.SetMaximumDamageToUnit(999);
            card.SetAsWalkerUnit();
            card.HasUsedBuffShield = true;
            card.ResolveBuffShield();
            card.AttackRestriction = Enumerators.AttackRestriction.ANY;
            card.AttackTargetsAvailability = new List<Enumerators.SkillTarget>()
            {
                Enumerators.SkillTarget.OPPONENT,
                Enumerators.SkillTarget.OPPONENT_CARD
            };

            DeactivateAllAbilitiesOnUnit(card);

            card.Distract();
        }

        public void DeactivateAllAbilitiesOnUnit(CardModel card)
        {
            card.BuffsOnUnit.Clear();

            card.ClearEffectsOnUnit();

            List<AbilityBase> abilities = _abilitiesController.GetAbilitiesConnectedToUnit(card);

            foreach (AbilityBase ability in abilities)
            {
                ability.Deactivate();
                ability.Dispose();
            }
        }

        public BoardUnitView CreateBoardUnit(Player owner, CardModel cardModel, bool playArrivalImmediately = true)
        {
            GameObject playerBoard = owner.IsLocalPlayer ? PlayerBoardObject : OpponentBoardObject;

            BoardUnitView boardUnitView = new BoardUnitView(cardModel, playerBoard.transform);
            boardUnitView.Transform.tag = owner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.SetParent(playerBoard.transform);
            boardUnitView.Transform.position = new Vector2(1.9f * owner.CardsOnBoard.Count, 0);

            if (playArrivalImmediately)
            {
                boardUnitView.PlayArrivalAnimation();
            }

            return boardUnitView;
        }


        public IBoardObject GetTargetByInstanceId(InstanceId id, bool createHandCardByDefault = true)
        {
            if (_gameplayManager.CurrentPlayer.InstanceId == id)
                return _gameplayManager.CurrentPlayer;

            if (_gameplayManager.OpponentPlayer.InstanceId == id)
                return _gameplayManager.OpponentPlayer;

            CardModel cardModelById = GetCardModelByInstanceId(id);
            if (cardModelById != null)
                return cardModelById;

            if (createHandCardByDefault)
            {
                CardModel card = GetCardModelByInstanceId(id);
                if (card != null)
                {
                    BoardCardView boardCardView = CreateCustomHandBoardCard(card);
                    Object.Destroy(boardCardView.GameObject);
                    return boardCardView.HandBoardCard;
                }
            }

            return null;
        }

        public List<IBoardObject> GetTargetsByInstanceId(IList<Unit> targetUnits)
        {
            List<IBoardObject> boardObjects = new List<IBoardObject>();

            if (targetUnits != null)
            {
                foreach (Unit targetUnit in targetUnits)
                {
                    boardObjects.Add(GetTargetByInstanceId(targetUnit.InstanceId));
                }
            }

            return boardObjects;
        }

        public BoardSkill GetSkillById(Player owner, SkillId skillId)
        {
            if (!owner.IsLocalPlayer)
            {
                if (_skillsController.OpponentPrimarySkill.SkillId == skillId)
                    return _skillsController.OpponentPrimarySkill;
                else if (_skillsController.OpponentSecondarySkill.SkillId == skillId)
                    return _skillsController.OpponentSecondarySkill;
            }
            else
            {
                if (_skillsController.PlayerPrimarySkill.SkillId == skillId)
                    return _skillsController.PlayerPrimarySkill;
                else if (_skillsController.PlayerSecondarySkill.SkillId == skillId)
                    return _skillsController.PlayerSecondarySkill;
            }

            return null;
        }

        public CardModel GetCardModelByInstanceId(InstanceId id, bool onlyCardsInPlay = false)
        {
            IEnumerable<CardModel> cardModels;
            if (!onlyCardsInPlay)
            {
                cardModels =
                    _gameplayManager.CurrentPlayer.CardsOnBoard
                    .Concat(_gameplayManager.CurrentPlayer.CardsInHand)
                    .Concat(_gameplayManager.CurrentPlayer.CardsInDeck)
                    .Concat(_gameplayManager.OpponentPlayer.CardsOnBoard)
                    .Concat(_gameplayManager.OpponentPlayer.CardsInHand)
                    .Concat(_gameplayManager.OpponentPlayer.CardsInDeck);
            }
            else
            {
                cardModels =
                    _gameplayManager.CurrentPlayer.CardsOnBoard
                    .Concat(_gameplayManager.OpponentPlayer.CardsOnBoard);
            }

            CardModel cardModel =
                cardModels
                    .FirstOrDefault(model => model != null && model.Card.InstanceId == id);

            if (cardModel == null)
            {
                Log.Warn($"GetCardModelByInstanceId returned null (InstanceId id = {id.Id}, bool onlyCardsInPlay = {onlyCardsInPlay})");
            }

            return cardModel;
        }

        public IBoardObject GetBoardObjectByInstanceId(InstanceId id, bool handlePlayers = true)
        {
            CardModel cardModel = GetCardModelByInstanceId(id, true);
            if (cardModel != null)
                return cardModel;

            List<IBoardObject> boardObjects = new List<IBoardObject>();

            if (handlePlayers)
            {
                boardObjects.Add(_gameplayManager.CurrentPlayer);
                boardObjects.Add(_gameplayManager.OpponentPlayer);
            }


            IBoardObject foundObject = boardObjects.Find(boardObject =>
            {
                switch (boardObject)
                {
                    case IInstanceIdOwner instanceIdOwner:
                        return instanceIdOwner.InstanceId == id;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(boardObject), boardObject, null);
                }
            });

            if (foundObject == null)
            {
                Log.Warn($"GetBoardObjectByInstanceId returned null (InstanceId id = {id.Id})");
            }

            return foundObject;
        }

        public List<CardModel> GetAdjacentUnitsToUnit(CardModel targetUnit)
        {
            IReadOnlyList<CardModel> boardCards = targetUnit.OwnerPlayer.CardsOnBoard;
            int targetIndex = boardCards.IndexOf(targetUnit);

            boardCards = boardCards.Where(unit =>
                    unit != targetUnit &&
                    (boardCards.IndexOf(unit) == Mathf.Clamp(targetIndex - 1, 0, boardCards.Count - 1) ||
                        boardCards.IndexOf(unit) == Mathf.Clamp(targetIndex + 1, 0, boardCards.Count - 1) &&
                        boardCards.IndexOf(unit) != targetIndex)
                )
                .ToList();

            return GetAliveUnits(boardCards).ToList();
        }

        public BoardCardView CreateCustomHandBoardCard(CardModel cardModel)
        {
            BoardCardView boardCardView = new UnitBoardCardView(Object.Instantiate(_cardsController.CreatureCardViewPrefab), cardModel);
            boardCardView.GameObject.transform.position = cardModel.OwnerPlayer.IsLocalPlayer ? Constants.DefaultPositionOfPlayerBoardCard :
                                                                                 Constants.DefaultPositionOfOpponentBoardCard;
            boardCardView.GameObject.transform.localScale = Vector3.one * .3f;
            boardCardView.SetHighlightingEnabled(false);

            boardCardView.HandBoardCard = new HandBoardCard(boardCardView.GameObject, boardCardView);

            return boardCardView;
        }

        private static void ValidateGameState(GameState pvpControlGameState)
        {
            if (!Constants.GameStateValidationEnabled || pvpControlGameState == null)
                return;

            GameState currentGameState = GameStateConstructor.Create().CreateCurrentGameStateFromOnlineGame(true);
            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.ShowBreadcrumb = true;
            compareLogic.Config.TreatStringEmptyAndNullTheSame = true;
            compareLogic.Config.MaxDifferences = 25;
            compareLogic.Config.MembersToIgnore.Add("CardsInGraveyard");
            compareLogic.Config.MembersToIgnore.Add("CardsInHand");
            compareLogic.Config.MembersToIgnore.Add("CardsInDeck");
            compareLogic.Config.MembersToIgnore.Add("CurrentGoo");
            compareLogic.Config.MembersToIgnore.Add("GooVials");
            compareLogic.Config.ActualName = "OpponentState";
            compareLogic.Config.ExpectedName = "LocalState";
            ComparisonResult comparisonResult = compareLogic.Compare(currentGameState, pvpControlGameState);
            if (!comparisonResult.AreEqual)
            {
                GameStateDesyncException desyncException = new GameStateDesyncException(comparisonResult.DifferencesString);
                UserReportingScript.Instance.SummaryInput.text = "PvP De-sync Detected";
#if USE_PRODUCTION_BACKEND
                    Log.Error(desyncException);

                    if (!GameClient.Get<IGameplayManager>().IsDesyncDetected)
                    {
                        GameClient.Get<IGameplayManager>().IsDesyncDetected = true;
                        UserReportingScript.Instance.CreateUserReport(
                            true,
                            false,
                            desyncException.GetType().ToString(),
                            desyncException.ToString()
                        );
                    }
#elif UNITY_EDITOR
                Log.Error("", desyncException);
#else
                throw desyncException;
#endif
            }
        }

        #region specific setup of battleground

        public void SetupBattlegroundAsSpecific(SpecificBattlegroundInfo specificBattlegroundInfo)
        {
            SetupOverlordsAsSpecific(specificBattlegroundInfo);
            SetupOverlordsHandsAsSpecific(specificBattlegroundInfo.PlayerInfo.CardsInHand, specificBattlegroundInfo.OpponentInfo.CardsInHand);
            SetupOverlordsDeckByPlayerAsSpecific(specificBattlegroundInfo.PlayerInfo.CardsInDeck, _gameplayManager.CurrentPlayer);
            SetupOverlordsDeckByPlayerAsSpecific(specificBattlegroundInfo.OpponentInfo.CardsInDeck, _gameplayManager.OpponentPlayer);
            SetupOverlordsBoardUnitsAsSpecific(specificBattlegroundInfo.PlayerInfo.CardsOnBoard, specificBattlegroundInfo.OpponentInfo.CardsOnBoard);
            SetupGeneralUIAsSpecific(specificBattlegroundInfo);
        }

        public void SetOpponentDeckAsSpecific(SpecificBattlegroundInfo specificBattlegroundInfo)
        {
            SetupOverlordsDeckByPlayerAsSpecific(specificBattlegroundInfo.OpponentInfo.CardsInDeck, _gameplayManager.OpponentPlayer);
        }

        private void SetupOverlordsAsSpecific(SpecificBattlegroundInfo specificBattlegroundInfo)
        {
            _gameplayManager.OpponentPlayer.Defense = specificBattlegroundInfo.OpponentInfo.Defense;
            _gameplayManager.OpponentPlayer.GooVials = specificBattlegroundInfo.OpponentInfo.MaximumGoo;
            _gameplayManager.OpponentPlayer.CurrentGoo = specificBattlegroundInfo.OpponentInfo.CurrentGoo;

            _gameplayManager.CurrentPlayer.Defense = specificBattlegroundInfo.PlayerInfo.Defense;
            _gameplayManager.CurrentPlayer.GooVials = specificBattlegroundInfo.PlayerInfo.MaximumGoo;
            _gameplayManager.CurrentPlayer.CurrentGoo = specificBattlegroundInfo.PlayerInfo.CurrentGoo;
        }

        private void SetupOverlordsHandsAsSpecific(List<SpecificBattlegroundInfo.OverlordCardInfo> playerCards,
                                                    List<SpecificBattlegroundInfo.OverlordCardInfo> opponentCards)
        {
            WorkingCard card;
            foreach (SpecificBattlegroundInfo.OverlordCardInfo cardInfo in playerCards)
            {
                card = _cardsController.CreateWorkingCardFromCardName(cardInfo.Name, _gameplayManager.CurrentPlayer);
                card.TutorialObjectId = cardInfo.TutorialObjectId;
                _gameplayManager.CurrentPlayer.PlayerCardsController.AddCardToHand(new CardModel(card), true);
            }

            foreach (SpecificBattlegroundInfo.OverlordCardInfo cardInfo in opponentCards)
            {
                card = _cardsController.CreateWorkingCardFromCardName(cardInfo.Name, _gameplayManager.OpponentPlayer);
                card.TutorialObjectId = cardInfo.TutorialObjectId;
                _gameplayManager.OpponentPlayer.PlayerCardsController.AddCardToHand(new CardModel(card), true);
            }
        }

        private void SetupOverlordsDeckByPlayerAsSpecific(List<SpecificBattlegroundInfo.OverlordCardInfo> cards, Player player)
        {
            List<CardModel> cardModels =
                cards
                    .Select(cardInfo =>
                    {
                        Card card = _dataManager.CachedCardsLibraryData.GetCardFromName(cardInfo.Name);
                        WorkingCard workingCard = new WorkingCard(card, card, player);
                        workingCard.TutorialObjectId = cardInfo.TutorialObjectId;
                        return new CardModel(workingCard);
                    })
                    .ToList();

            player.PlayerCardsController.SetCardsInDeck(cardModels);
        }

        private void SetupOverlordsGraveyardsAsSpecific(List<string> playerCards, List<string> opponentCards)
        {
            // todo implement logic
        }

        private void SetupOverlordsBoardUnitsAsSpecific(List<SpecificBattlegroundInfo.UnitOnBoardInfo> playerCards,
                                                        List<SpecificBattlegroundInfo.UnitOnBoardInfo> opponentCards)
        {
            BoardUnitView workingUnitView = null;

            foreach (SpecificBattlegroundInfo.UnitOnBoardInfo cardInfo in playerCards)
            {
                workingUnitView = _gameplayManager.CurrentPlayer.PlayerCardsController.SpawnUnitOnBoard(cardInfo.Name, ItemPosition.End);
                workingUnitView.Model.Card.TutorialObjectId = cardInfo.TutorialObjectId;
                workingUnitView.Model.CantAttackInThisTurnBlocker = !cardInfo.IsManuallyPlayable;
                workingUnitView.Model.AddToCurrentDefenseHistory(cardInfo.BuffedDefense, Enumerators.ReasonForValueChange.AbilityBuff);
                workingUnitView.Model.BuffedDefense += cardInfo.BuffedDefense;
                workingUnitView.Model.AddToCurrentDamageHistory(cardInfo.BuffedDamage, Enumerators.ReasonForValueChange.AbilityBuff);
                workingUnitView.Model.BuffedDamage += cardInfo.BuffedDamage;
            }

            foreach (SpecificBattlegroundInfo.UnitOnBoardInfo cardInfo in opponentCards)
            {
                workingUnitView = _gameplayManager.CurrentPlayer.PlayerCardsController.SpawnUnitOnBoard(cardInfo.Name, ItemPosition.End);
                workingUnitView.Model.CantAttackInThisTurnBlocker = !cardInfo.IsManuallyPlayable;
            }
        }

        private void SetupGeneralUIAsSpecific(SpecificBattlegroundInfo specificBattlegroundInfo)
        {
            // todo implement logic
        }

        #endregion

        private void OnGameInitializedHandler()
        {
        }

        public List<CardModel> GetDeterministicRandomUnits(List<CardModel> units, int count)
        {
            return GetDeterministicRandomElements(units, count)
                .FindAll(card => card.CurrentDefense > 0 && !card.IsDead && card.IsUnitActive);
        }

        public List<T> GetDeterministicRandomElements<T>(List<T> elements, int count)
        {
            return InternalTools.GetRandomElementsFromList(elements, count, true);
        }

        public List<CardModel> GetRandomUnits(List<CardModel> units, int count)
        {
            return GetRandomElements(units, count)
                .FindAll(card => card.CurrentDefense > 0 && !card.IsDead && card.IsUnitActive);
        }

        public List<T> GetRandomElements<T>(List<T> elements, int count)
        {
            return InternalTools.GetRandomElementsFromList(elements, count);
        }

        public IEnumerable<CardModel> GetAliveUnits(IEnumerable<CardModel> units)
        {
            return units.Where(card => card.IsAlive());
        }

        public bool HasUnitInAttackingState(IEnumerable<BoardUnitView> units)
        {
            foreach (BoardUnitView unit in units)
            {
                if (unit!= null && unit.Model != null && unit.Model.IsAttacking)
                    return true;
            }

            return false;
        }
    }
}
