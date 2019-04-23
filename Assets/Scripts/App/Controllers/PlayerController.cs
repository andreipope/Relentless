using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using UnityEngine.Rendering;
using Deck = Loom.ZombieBattleground.Data.Deck;
using InstanceId = Loom.ZombieBattleground.Data.InstanceId;

namespace Loom.ZombieBattleground
{
    public class PlayerController : IController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(PlayerController));

        private IGameplayManager _gameplayManager;

        private IDataManager _dataManager;

        private IPvPManager _pvpManager;

        private ITutorialManager _tutorialManager;

        private ITimerManager _timerManager;

        private IMatchManager _matchManager;

        private CardsController _cardsController;

        private BattlegroundController _battlegroundController;
        private BoardArrowController _boardArrowController;

        private bool _startedOnClickDelay;

        private bool _isPreviewHandCard;

        private float _delayTimerOfClick;

        private bool _cardsZooming;

        private bool _isHovering;

        private float _timeHovering;

        private bool _isMoveHoveringCard = false;

        private BoardCardView _hoveringHandCard;

        private BoardCardView _hoveringBoardCard;

        private BoardCardView _topmostBoardCard;

        private PointerEventSolver _pointerEventSolver;

        public bool IsPlayerStunned { get; set; }

        public bool IsCardSelected { get; set; }

        public bool IsActive { get; set; }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _matchManager = GameClient.Get<IMatchManager>();
            _pvpManager = GameClient.Get<IPvPManager>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();

            _gameplayManager.GameStarted += GameStartedHandler;
            _gameplayManager.GameEnded += GameEndedHandler;

            _pointerEventSolver = new PointerEventSolver();
            _pointerEventSolver.DragStarted += PointerSolverDragStartedHandler;
            _pointerEventSolver.Clicked += PointerEventSolverClickedHandler;
            _pointerEventSolver.Ended += PointerEventSolverEndedHandler;

            _gameplayManager.GetController<InputController>().ClickedOnBoardObjectEvent += ClickedOnBoardObjectEventHandler;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            if (!_gameplayManager.IsGameStarted || _gameplayManager.IsGameEnded)
                return;

            HandleInput();

            if (_tutorialManager.IsTutorial)
            {
                if (!_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.DisabledInitialization)
                {
                    if (_tutorialManager.CurrentTutorialStep != null && _tutorialManager.CurrentTutorialStep is TutorialGameplayStep &&
                    !_tutorialManager.CurrentTutorialStep.ToGameplayStep().CanInteractWithGameplay)
                        return;
                }
            }

            _pointerEventSolver.Update();
        }

        public void ResetAll()
        {
            StopHandTimer();
            _timerManager.StopTimer(SetStatusZoomingFalse);
        }

        public void InitializePlayer(Data.InstanceId instanceId)
        {
            Player player = new Player(instanceId, GameObject.Find("Player"), false);

            _gameplayManager.CurrentPlayer = player;

            GameClient.Get<IOverlordExperienceManager>().InitializeExperienceInfoInMatch(player.SelfOverlord);

            if (!_gameplayManager.IsSpecificGameplayBattleground ||
                (_gameplayManager.IsTutorial &&
                _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().
                SpecificBattlegroundInfo.DisabledInitialization))
            {
                List<WorkingCard> workingDeck = new List<WorkingCard>();

                bool isMainTurnSecond;
                switch (_matchManager.MatchType)
                {
                    case Enumerators.MatchType.LOCAL:
                        int deckId = _gameplayManager.PlayerDeckId;
                        Deck deck = _dataManager.CachedDecksData.Decks.First(d => d.Id == deckId);
                        foreach (DeckCardData card in deck.Cards)
                        {
                            for (int i = 0; i < card.Amount; i++)
                            {
                                workingDeck.Add(_cardsController.CreateWorkingCardFromCardName(card.CardName, player));
                            }
                        }

                        isMainTurnSecond = false;
                        break;
                    case Enumerators.MatchType.PVP:
                        foreach (CardInstance cardInstance in player.InitialPvPPlayerState.CardsInDeck)
                        {
                            workingDeck.Add(cardInstance.FromProtobuf(player));
                        }

                        Log.Info(
                            $"Player ID {instanceId}, local: {player.IsLocalPlayer}, added CardsInDeck:\n" +
                            String.Join("\n", workingDeck.Cast<object>().ToArray())
                        );

                        isMainTurnSecond = !GameClient.Get<IPvPManager>().IsFirstPlayer();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                CardModel[] cardModels = workingDeck.Select(card => new CardModel(card)).ToArray();
                player.PlayerCardsController.SetCardsInDeck(cardModels);
            }

            player.TurnStarted += OnTurnStartedStartedHandler;
            player.TurnEnded += OnTurnEndedEndedHandler;
        }

        public void SetHand()
        {
            Player player = _gameplayManager.CurrentPlayer;
            switch (_matchManager.MatchType)
            {
                case Enumerators.MatchType.LOCAL:

                    bool tutorialStatus = false;

                    if (_gameplayManager.IsTutorial)
                    {
                        tutorialStatus = _gameplayManager.IsSpecificGameplayBattleground;
                        tutorialStatus = !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.DisabledInitialization;
                    }

                    player.PlayerCardsController.SetFirstHandForLocalMatch(tutorialStatus);
                    break;
                case Enumerators.MatchType.PVP:
                    List<WorkingCard> workingCards =
                        player.InitialPvPPlayerState.CardsInHand
                        .Select(instance => instance.FromProtobuf(player))
                        .ToList();

                    Log.Info(
                        $"Player ID {player.InstanceId}, local: {player.IsLocalPlayer}, added CardsInHand:\n" +
                        String.Join("\n", workingCards.Cast<object>().ToArray())
                    );

                    CardModel[] cardModels = workingCards.Select(card => new CardModel(card)).ToArray();
                    player.PlayerCardsController.SetFirstHandForPvPMatch(cardModels);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _battlegroundController.UpdatePositionOfCardsInPlayerHand();
            player.MulliganWasStarted = true;
        }

        public virtual void GameStartedHandler()
        {
        }

        public virtual void GameEndedHandler(Enumerators.EndGameType endGameType)
        {
            IsActive = false;
            IsPlayerStunned = false;
            IsCardSelected = false;
        }

        public void HideCardPreview()
        {
            StopHandTimer();
            _battlegroundController.DestroyCardPreview();

            _delayTimerOfClick = 0f;
            _startedOnClickDelay = false;
            _topmostBoardCard = null;
        }

        public void HandCardPreview(object[] param)
        {
            if (_gameplayManager.IsTutorial &&
                !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().
                SpecificBattlegroundInfo.DisabledInitialization)
            {
                int id = 0;

                switch (param[0])
                {
                    case BoardUnitView unit:
                        id = unit.Model.TutorialObjectId;
                        break;
                    default:
                        break;
                }

                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.BattleframeSelected, id);

                if (!_tutorialManager.CheckAvailableTooltipByOwnerId(id))
                    return;
            }

            Vector3 cardPosition;

            if (!InternalTools.IsTabletScreen())
            {
                cardPosition = new Vector3(-9f, -3f, 0f);
            }
            else
            {
                cardPosition = new Vector3(-6f, -2.5f, 0f);
            }

            _battlegroundController.CreateCardPreview((ICardView) param[0], cardPosition, false);
        }

        public void OnTurnEndedEndedHandler()
        {
        }

        public void OnTurnStartedStartedHandler()
        {
        }

        public void UpdateHandCardsHighlight()
        {
            if (_gameplayManager.CurrentTurnPlayer == _gameplayManager.CurrentPlayer)
            {
                IReadOnlyList<BoardCardView> views = _battlegroundController.GetCardViewsFromModels<BoardCardView>(_gameplayManager.CurrentPlayer.CardsInHand);
                foreach (BoardCardView card in views)
                {
                    card?.SetHighlightingEnabled(card.Model.CanBeBuyed(_gameplayManager.CurrentPlayer));
                }
            }
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonUp(0))
            {
                _pointerEventSolver.PopPointer();
            }

            if (_boardArrowController.IsBoardArrowNowInTheBattle ||
                !_gameplayManager.CanDoDragActions ||
                _gameplayManager.IsGameplayInputBlocked ||
                _battlegroundController.TurnWaitingForEnd)
                return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                List<GameObject> hitCards = new List<GameObject>();
                bool hitHandCard = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.gameObject != null &&
                        _battlegroundController.GetBoardCardFromHisObject(hit.collider.gameObject) != null)
                    {
                        hitCards.Add(hit.collider.gameObject);
                        hitHandCard = true;
                    }
                }

                if (hitHandCard)
                {
                    if (hitCards.Count > 0)
                    {
                        hitCards = hitCards.OrderBy(x => x.GetComponent<SortingGroup>().sortingOrder).ToList();

                        BoardCardView topmostBoardCard =
                            _battlegroundController.GetBoardCardFromHisObject(hitCards[hitCards.Count - 1]);
                        if (topmostBoardCard != null && !topmostBoardCard.IsPreview)
                        {
                            float delta = Application.isMobilePlatform ?
                                Constants.PointerMinDragDelta * 2f :
                                Constants.PointerMinDragDeltaMobile;
                            _pointerEventSolver.PushPointer(delta);

                            _startedOnClickDelay = true;
                            _isPreviewHandCard = true;
                            _topmostBoardCard = topmostBoardCard;
                        }
                    }
                }
                else
                {
                    if (_battlegroundController.IsPreviewActive)
                    {
                        HideCardPreview();
                    }
                    else
                    {
                        _timerManager.StopTimer(SetStatusZoomingFalse);
                        _cardsZooming = true;
                        _timerManager.AddTimer(SetStatusZoomingFalse);
                        
                        _battlegroundController.CardsZoomed = false;
                        _battlegroundController.UpdatePositionOfCardsInPlayerHand();
                    }
                }
            }

            if (_startedOnClickDelay)
            {
                _delayTimerOfClick += Time.deltaTime;
            }

            if (_boardArrowController.CurrentBoardArrow != null && _boardArrowController.CurrentBoardArrow.IsDragging())
            {
                _battlegroundController.DestroyCardPreview();
            }

            if(!Application.isMobilePlatform && _boardArrowController.CurrentBoardArrow == null)
            {
                CastRay(mousePos);
            }
        }

        private void CastRay(Vector3 point)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, SRLayerMask.Default);
            hits = hits.Where(hit => !hit.collider.name.Equals(Constants.BattlegroundTouchZone)).ToArray();

            if (hits.Length > 0 && !IsCardSelected)
            {
                if (_tutorialManager.IsTutorial)
                {
                    foreach (RaycastHit2D hit in hits)
                    {
                        CheckColliders(hit.collider);
                    }
                }
                else if(!_isMoveHoveringCard && _gameplayManager.CanDoDragActions && !_battlegroundController.IsPreviewActive && !_cardsZooming && !_startedOnClickDelay)
                {
                    List<BoardCardView> boardCardViews = new List<BoardCardView>();
                    BoardCardView boardCardView = null;
                    foreach (RaycastHit2D hit in hits)
                    {
                        boardCardView = _gameplayManager.GetController<BattlegroundController>().GetBoardCardFromHisObject(hit.collider.gameObject);
                        if(boardCardView != null && boardCardView.HandBoardCard != null && !boardCardView.HandBoardCard.IsReturnToHand)
                        {
                            boardCardViews.Add(boardCardView);
                        }
                        boardCardView = null;
                    }

                    if (boardCardViews.Count > 0)
                    {
                        boardCardView = boardCardViews.Find(view => Vector3.Distance(view.PositionOnHand, point) == boardCardViews.Min(card => Vector3.Distance(card.PositionOnHand, point)));
                    }

                    if (boardCardView != null)
                    {
                        if (_hoveringHandCard != boardCardView)
                        {
                            if (_hoveringHandCard != null)
                            {
                                HideHoveringAndZoom();
                            }

                            _hoveringHandCard = boardCardView;
                            ShowHoveringAndZoom();
                        }
                    }
                    else if (_hoveringHandCard != null)
                    {
                        HideHoveringAndZoom();
                    }
                }
            }
            else
            {
                ClearHovering();
                if(_hoveringHandCard != null)
                {
                    HideHoveringAndZoom(!_cardsZooming);
                }
            }
        }

        private void CheckColliders(Collider2D collider)
        {
            BoardCardView boardCardView = _gameplayManager.GetController<BattlegroundController>().GetBoardCardFromHisObject(collider.gameObject);
            if (boardCardView != null)
            {
                UpdateHovering(boardCardView);
            }
        }

        private void ShowHoveringAndZoom()
        {
            _hoveringHandCard.HandBoardCard?.HoveringAndZoom();
        }

        private void HideHoveringAndZoom(bool isMove = true)
        {
            _isMoveHoveringCard = true;
            Action onComplete = () =>
            {
                _isMoveHoveringCard = false;
            };
            _hoveringHandCard.HandBoardCard?.ResetHoveringAndZoom(isMove, onComplete);
            _hoveringHandCard = null;
        }

        private void UpdateHovering(BoardCardView boardCardView)
        {
            if (_hoveringBoardCard != boardCardView)
            {
                _isHovering = false;
                _hoveringBoardCard = boardCardView;
                _timeHovering = 0;
            }
            else if (!_isHovering)
            {
                _timeHovering += Time.deltaTime;
                if (_timeHovering >= Constants.MaxTimeForHovering)
                {
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerCardInHandSelected, _hoveringBoardCard.Model.Card.TutorialObjectId);

                    _isHovering = true;
                }
            }
        }

        private void ClearHovering()
        {
            _isHovering = false;
            _hoveringBoardCard = null;
            _timeHovering = 0;
        }

        private void PointerSolverDragStartedHandler()
        {
            if (_hoveringHandCard != null)
            {
                HideHoveringAndZoom(false);
            }

            _topmostBoardCard?.HandBoardCard?.OnSelected();

            if (_boardArrowController.CurrentBoardArrow == null)
            {
                HideCardPreview();
            }

            
        }

        private void PointerEventSolverClickedHandler()
        {
            if (_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerCardInHandSelected);
            }

            if (_isPreviewHandCard && _battlegroundController.CardsZoomed)
            {
                if (_topmostBoardCard != null && !_cardsZooming)
                {
                    StopHandTimer();
                    _battlegroundController.DestroyCardPreview();

                    if (_boardArrowController.CurrentBoardArrow == null)
                    {
                        HandCardPreview(new object[]
                        {
                                _topmostBoardCard
                        });
                    }
                }
            }

            _timerManager.StopTimer(SetStatusZoomingFalse);
            _cardsZooming = true;
            _timerManager.AddTimer(SetStatusZoomingFalse, null, .8f);

            if (_hoveringHandCard != null)
            {
                HideHoveringAndZoom(false);
            }

            _battlegroundController.CardsZoomed = true;
            _battlegroundController.UpdatePositionOfCardsInPlayerHand();
        }

        private void ClickedOnBoardObjectEventHandler(IBoardObject boardObject)
        {
            if (GameClient.Get<IUIManager>().GetPopup<SettingsWithCreditsPopup>().Self != null)
                return;
        
            switch (boardObject)
            {
                case CardModel unit:
                    if (!unit.IsAttacking)
                    {
                        StopHandTimer();
                        _battlegroundController.DestroyCardPreview();

                        if (!_boardArrowController.IsBoardArrowNowInTheBattle)
                        {
                            HandCardPreview(new object[]
                            {
                                _battlegroundController.GetCardViewByModel<BoardUnitView>(unit)
                            });
                        }
                    }
                    break;
            }
        }

        private void PointerEventSolverEndedHandler()
        {
            _delayTimerOfClick = 0f;
            _startedOnClickDelay = false;

            _topmostBoardCard = null;

            _battlegroundController.CurrentPreviewedCardId = InstanceId.Invalid;
        }

        private void StopHandTimer()
        {
            GameClient.Get<ITimerManager>().StopTimer(HandCardPreview);
        }

        private void SetStatusZoomingFalse(object[] param)
        {
            _cardsZooming = false;
        }
    }
}
