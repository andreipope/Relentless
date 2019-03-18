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

        public UniquePositionedList<BoardUnitView> OpponentGraveyardCards { get; } =  new UniquePositionedList<BoardUnitView>(new PositionedList<BoardUnitView>());

        public UniquePositionedList<OpponentHandCard> OpponentHandCards { get; } =  new UniquePositionedList<OpponentHandCard>(new PositionedList<OpponentHandCard>());

        public UniquePositionedList<BoardUnitView> PlayerGraveyardCards { get; } = new UniquePositionedList<BoardUnitView>(new PositionedList<BoardUnitView>());

        public UniquePositionedList<BoardCardView> PlayerHandCards { get; } = new UniquePositionedList<BoardCardView>(new PositionedList<BoardCardView>());

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

        public bool TurnWaitingForEnd { get; private set; }

        public UniqueList<IBoardUnitView> BoardUnitViews = new UniqueList<IBoardUnitView>();

        public T GetBoardUnitViewByModel<T>(BoardUnitModel boardUnitModel) where T : IBoardUnitView
        {
            if (boardUnitModel == null)
                throw new ArgumentNullException(nameof(boardUnitModel));

            T view =
                BoardUnitViews
                .OfType<T>()
                .Where(v => v.Model == boardUnitModel)
                .SingleOrDefault();

            //if (view == null)
            //    throw new Exception($"No view found for model {boardUnitModel}");

            return view;
        }

        public IReadOnlyList<BoardUnitView> GetBoardUnitViewsFromModels(IReadOnlyList<BoardUnitModel> models)
        {
            return models.Select(GetBoardUnitViewByModel<BoardUnitView>).ToList();
        }

        public void RegisterBoardUnitView(Player player, IBoardUnitView view, ItemPosition position = default(ItemPosition))
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            BoardUnitViews.Add(view);
        }

        public void UnregisterBoardUnitView(Player player, IBoardUnitView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            BoardUnitViews.Remove(view);
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

                foreach (BoardUnitView item in GetBoardUnitViewsFromModels(_gameplayManager.CurrentPlayer.CardsOnBoard))
                {
                    item.Update();
                }

                foreach (BoardUnitView item in GetBoardUnitViewsFromModels(_gameplayManager.OpponentPlayer.CardsOnBoard))
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
                            StopTurn();
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

        public void KillBoardCard(BoardUnitModel boardUnitModel, bool withDeathEffect = true, bool updateBoard = true)
        {
            BoardUnitView boardUnitView = GetBoardUnitViewByModel<BoardUnitView>(boardUnitModel);

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
                    boardUnitView.DisposeGameObject();

                    boardUnitView.WasDestroyed = true;
                };

                Action endOfAnimationCallback = () =>
                {
                    boardUnitView.Model.InvokeUnitDied();

                    UnregisterBoardUnitView(boardUnitModel.OwnerPlayer, boardUnitView);
                    boardUnitModel.OwnerPlayer.PlayerCardsController.RemoveCardFromBoard(boardUnitModel);
                    boardUnitModel.OwnerPlayer.PlayerCardsController.AddCardToGraveyard(boardUnitModel);

                    if(_tutorialManager.IsTutorial)
                    {
                        if (boardUnitModel.OwnerPlayer.IsLocalPlayer)
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
                    endOfAnimationCallback();
                    endOfDestroyAnimationCallback();

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
            PlayerHandCards.Clear();
            OpponentHandCards.Clear();

            _gameplayManager.CurrentPlayer.PlayerCardsController.ClearCardsOnBoard();
            _gameplayManager.OpponentPlayer.PlayerCardsController.ClearCardsOnBoard();

            PlayerGraveyardCards.Clear();
            OpponentGraveyardCards.Clear();
        }

        public void InitializeBattleground()
        {
            CurrentTurn = 0;

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
                TurnTimer = _gameplayManager.CurrentTurnPlayer.TurnTime;
                _turnTimerCounting = true;
            }

            CurrentTurn++;

            _gameplayManager.CurrentTurnPlayer.Turn++;

            _uiManager.GetPage<GameplayPage>().SetEndTurnButtonStatus(_gameplayManager.IsLocalPlayerTurn());

            UpdatePositionOfCardsInOpponentHand();
            _playerController.IsActive = _gameplayManager.IsLocalPlayerTurn();

            IReadOnlyList<BoardUnitView> currentPlayerCardsOnBoardUnitViews = GetBoardUnitViewsFromModels(_gameplayManager.CurrentPlayer.CardsOnBoard);
            IReadOnlyList<BoardUnitView> opponentPlayerCardsOnBoardUnitViews = GetBoardUnitViewsFromModels(_gameplayManager.OpponentPlayer.CardsOnBoard);
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
                    UnregisterBoardUnitView(_gameplayManager.CurrentPlayer, item);
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

                foreach (BoardCardView card in PlayerHandCards)
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

        public void EndTurn()
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

                foreach (BoardUnitModel card in _gameplayManager.CurrentPlayer.CardsOnBoard)
                {
                    card.OnEndTurn();
                }
            }
            else
            {
                foreach (BoardUnitModel card in _gameplayManager.OpponentPlayer.CardsOnBoard)
                {
                    card.OnEndTurn();
                }
            }

            _gameplayManager.CurrentPlayer.InvokeTurnEnded();
            _gameplayManager.OpponentPlayer.InvokeTurnEnded();


            if (_gameplayManager.IsLocalPlayerTurn())
            {
                TurnEnded?.Invoke();
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.EndTurn);
            }

            _gameplayManager.CurrentTurnPlayer = _gameplayManager.IsLocalPlayerTurn() ?
                _gameplayManager.OpponentPlayer :
                _gameplayManager.CurrentPlayer;
        }

        public void StopTurn(GameState pvpControlGameState = null)
        {
            TurnWaitingForEnd = true;

            _gameplayManager.GetController<ActionsQueueController>().AddNewActionInToQueue(
                 (parameter, completeCallback) =>
                 {
                     float delay = (!_tutorialManager.IsTutorial && _matchManager.MatchType == Enumerators.MatchType.PVP) ? 2 : 0;
                     InternalTools.DoActionDelayed(() =>
                     {
                         TurnWaitingForEnd = false;
                         ValidateGameState(pvpControlGameState);
                         EndTurn();

                         if (_gameplayManager.IsLocalPlayerTurn())
                         {
                             _uiManager.DrawPopup<YourTurnPopup>();

                             _timerManager.AddTimer((x) =>
                             {
                                 StartTurn();
                                 completeCallback?.Invoke();
                             }, null, Constants.DelayBetweenYourTurnPopup);
                         }
                         else
                         {
                             StartTurn();
                             completeCallback?.Invoke();
                         }
                     }, delay);
                 },  Enumerators.QueueActionType.StopTurn);
        }

        public void RemovePlayerCardFromBoardToGraveyard(BoardUnitModel boardUnitModel)
        {
            BoardUnitView boardCardView = GetBoardUnitViewByModel<BoardUnitView>(boardUnitModel);
            if (boardCardView == null)
                return;

            if (!boardCardView.WasDestroyed)
            {
                boardCardView.Transform.localPosition = new Vector3(boardCardView.Transform.localPosition.x,
                boardCardView.Transform.localPosition.y, -0.2f);
            }

            UnregisterBoardUnitView(_gameplayManager.CurrentPlayer, boardCardView);
            PlayerGraveyardCards.Insert(ItemPosition.End, boardCardView);

            boardCardView.SetHighlightingEnabled(false);
            boardCardView.StopSleepingParticles();

            if (!boardCardView.WasDestroyed)
            {
                boardCardView.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.BoardCards;
                Object.Destroy(boardCardView.GameObject.GetComponent<BoxCollider2D>());
            }
        }

        public void RemoveOpponentCardFromBoardToGraveyard(BoardUnitModel boardUnitModel)
        {
            Vector3 graveyardPos = OpponentGraveyardObject.transform.position + new Vector3(0.0f, -0.2f, 0.0f);
            BoardUnitView boardCardView = GetBoardUnitViewByModel<BoardUnitView>(boardUnitModel);
            if (boardCardView != null)
            {
                if (!boardCardView.WasDestroyed)
                {
                    boardCardView.Transform.localPosition = new Vector3(boardCardView.Transform.localPosition.x,
                        boardCardView.Transform.localPosition.y, -0.2f);
                }

                UnregisterBoardUnitView(_gameplayManager.OpponentPlayer, boardCardView);

                boardCardView.SetHighlightingEnabled(false);
                boardCardView.StopSleepingParticles();

                if (!boardCardView.WasDestroyed)
                {
                    boardCardView.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.BoardCards;
                    Object.Destroy(boardCardView.GameObject.GetComponent<BoxCollider2D>());
                }
            }
            else if (_aiController.CurrentItemCard != null && boardUnitModel == _aiController.CurrentItemCard.Model)
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
        public void CreateCardPreview(object target, Vector3 pos, bool highlight = true)
        {
            IsPreviewActive = true;

            switch (target)
            {
                case BoardCardView card:
                    CurrentPreviewedCardId = card.Model.Card.InstanceId;
                    break;
                case BoardUnitView unit:
                    _lastBoardUntilOnPreview = unit;
                    CurrentPreviewedCardId = unit.Model.Card.InstanceId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }

            CreatePreviewCoroutine = MainApp.Instance.StartCoroutine(CreateCardPreviewAsync(target, pos, highlight));
        }

        // rewrite
        public IEnumerator CreateCardPreviewAsync(object target, Vector3 pos, bool highlight)
        {
            yield return new WaitForSeconds(0.3f);

            WorkingCard card = null;
            BoardUnitModel boardUnitModel = null;

            switch (target)
            {
                case BoardCardView card1:
                    card = card1.Model.Card;
                    boardUnitModel = card1.Model;
                    break;
                case BoardUnitView unit:
                    card = unit.Model.Card;
                    boardUnitModel = unit.Model;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }

            BoardCardView boardCardView;
            switch (card.Prototype.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    CurrentBoardCard = Object.Instantiate(_cardsController.CreatureCardViewPrefab);
                    boardCardView = new UnitBoardCard(CurrentBoardCard, boardUnitModel);
                    break;
                case Enumerators.CardKind.ITEM:
                    CurrentBoardCard = Object.Instantiate(_cardsController.ItemCardViewPrefab);
                    boardCardView = new ItemBoardCard(CurrentBoardCard, boardUnitModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (highlight)
            {
                highlight = boardCardView.Model.CanBePlayed(card.Owner) && boardCardView.Model.CanBeBuyed(card.Owner);
            }

            boardCardView.SetHighlightingEnabled(highlight);
            boardCardView.IsPreview = true;

            InternalTools.SetLayerRecursively(boardCardView.GameObject, 0);

            switch (target)
            {
                case BoardUnitView boardUnit:
                    boardCardView.DrawTooltipInfoOfUnit(boardUnit);
                    UnitBoardCard boardCardUnit = boardCardView as UnitBoardCard;
                    boardCardUnit.Model.Card.InstanceCard.Damage = boardUnit.Model.MaxCurrentDamage;
                    boardCardUnit.Model.Card.InstanceCard.Defense = boardUnit.Model.MaxCurrentDefense;
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

            CurrentBoardCard.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI3;
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

            for (int i = 0; i < PlayerHandCards.Count; i++)
            {
                handWidth += spacing;
            }

            handWidth -= spacing;

            if (PlayerHandCards.Count == 1)
            {
                twistPerCard = 0;
            }

            int totalTwist = twistPerCard * PlayerHandCards.Count;
            float startTwist = (totalTwist - twistPerCard) / 2f;
            float scalingFactor = 0.04f;
            Vector3 moveToPosition = Vector3.zero;

            for (int i = 0; i < PlayerHandCards.Count; i++)
            {
                BoardCardView card = PlayerHandCards[i];
                float twist = startTwist - i * twistPerCard;
                float nudge = Mathf.Abs(twist);

                nudge *= scalingFactor;
                moveToPosition = new Vector3(pivot.x - handWidth / 2, pivot.y - nudge,
                    (PlayerHandCards.Count - i) * 0.1f);

                if (isMove)
                {
                    card.IsNewCard = false;
                }

                card.UpdateCardPositionInHand(moveToPosition, Vector3.forward * twist, Vector3.one * scaling);

                pivot.x += handWidth / PlayerHandCards.Count;

                card.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.HandCards;
                card.GameObject.GetComponent<SortingGroup>().sortingOrder = i;
            }
        }

        public void UpdatePositionOfCardsInOpponentHand(bool isMove = false, bool isNewCard = false)
        {
            float handWidth = 0.0f;
            float spacing = -1.0f;
            float zPositionKoef = -0.1f;

            for (int i = 0; i < OpponentHandCards.Count; i++)
            {
                handWidth += spacing;
            }

            handWidth -= spacing;

            Vector3 pivot = new Vector3(-3.2f, 8.5f, 0f);
            int twistPerCard = 5;

            if (OpponentHandCards.Count == 1)
            {
                twistPerCard = 0;
            }

            int totalTwist = twistPerCard * OpponentHandCards.Count;
            float startTwist = (totalTwist - twistPerCard) / 2f;

            for (int i = 0; i < OpponentHandCards.Count; i++)
            {
                OpponentHandCard card = OpponentHandCards[i];
                float twist = startTwist - i * twistPerCard;

                Vector3 movePosition = new Vector3(pivot.x - handWidth / 2, pivot.y, i * zPositionKoef);
                Vector3 rotatePosition = new Vector3(0, 0, twist);

                if (isMove)
                {
                    if (i == OpponentHandCards.Count - 1 && isNewCard)
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

                pivot.x += handWidth / OpponentHandCards.Count;
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
            BoardCardView card = PlayerHandCards.FirstOrDefault(x => x.GameObject == cardObject);

            return card;
        }

        public void DestroyBoardUnit(BoardUnitModel unit,
                                    bool withDeathEffect = true,
                                    bool isForceDestroy = false,
                                    bool handleShield = false,
                                    bool updateBoard = true)
        {
            if (!isForceDestroy && unit.HasBuffShield && handleShield)
            {
                unit.UseShieldFromBuff();
            }
            else
            {
                _gameplayManager.GetController<BattleController>().CheckOnKillEnemyZombie(unit);

                unit?.Die(withDeathEffect: withDeathEffect, updateBoard: updateBoard);
            }
        }

        public void TakeControlUnit(Player newPlayerOwner, BoardUnitModel unit)
        {
            BoardUnitView view = GetBoardUnitViewByModel<BoardUnitView>(unit);

            if (unit.OwnerPlayer.IsLocalPlayer)
            {

                UnregisterBoardUnitView(_gameplayManager.CurrentPlayer, view);
                RegisterBoardUnitView(_gameplayManager.OpponentPlayer, view);
            }
            else
            {
                UnregisterBoardUnitView(_gameplayManager.OpponentPlayer, view);
                RegisterBoardUnitView(_gameplayManager.CurrentPlayer, view);
            }

            foreach (AbilityBase ability in _abilitiesController.GetAbilitiesConnectedToUnit(unit))
            {
                ability.ChangePlayerCallerOfAbility(newPlayerOwner);
            }


            UnregisterBoardUnitView(unit.OwnerPlayer, view);
            newPlayerOwner.PlayerCardsController.TakeControlOfUnit(unit);
            RegisterBoardUnitView(newPlayerOwner, view);

            view.Transform.tag = newPlayerOwner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;

            _boardController.UpdateWholeBoard(null);

            foreach (AbilityBase ability in _abilitiesController.GetAbilitiesConnectedToUnit(unit))
            {
                ability.PlayerCallerOfAbility = newPlayerOwner;
            }
        }

        public void DistractUnit(BoardUnitModel boardUnit)
        {
            boardUnit.BuffedDamage = 0;
            boardUnit.BuffedDefense = 0;
            boardUnit.HasSwing = false;
            boardUnit.TakeFreezeToAttacked = false;
            boardUnit.HasBuffRush = false;
            boardUnit.HasBuffHeavy = false;
            boardUnit.SetAsWalkerUnit();
            boardUnit.UseShieldFromBuff();
            boardUnit.AttackRestriction = Enumerators.AttackRestriction.ANY;
            boardUnit.AttackTargetsAvailability = new List<Enumerators.SkillTargetType>()
            {
                Enumerators.SkillTargetType.OPPONENT,
                Enumerators.SkillTargetType.OPPONENT_CARD
            };

            DeactivateAllAbilitiesOnUnit(boardUnit);

            boardUnit.Distract();
        }

        public void DeactivateAllAbilitiesOnUnit(BoardUnitModel boardUnit)
        {
            boardUnit.BuffsOnUnit.Clear();

            boardUnit.ClearEffectsOnUnit();

            List<AbilityBase> abilities = _abilitiesController.GetAbilitiesConnectedToUnit(boardUnit);

            foreach (AbilityBase ability in abilities)
            {
                ability.Deactivate();
                ability.Dispose();
            }
        }

        public BoardUnitView CreateBoardUnit(Player owner, BoardUnitModel boardUnitModel)
        {
            GameObject playerBoard = owner.IsLocalPlayer ? PlayerBoardObject : OpponentBoardObject;

            BoardUnitView boardUnitView = new BoardUnitView(boardUnitModel, playerBoard.transform);
            boardUnitView.Transform.tag = owner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.SetParent(playerBoard.transform);
            boardUnitView.Transform.position = new Vector2(1.9f * owner.CardsOnBoard.Count, 0);

            boardUnitView.PlayArrivalAnimation();

            return boardUnitView;
        }


        public BoardObject GetTargetByInstanceId(InstanceId id) {
            if (_gameplayManager.CurrentPlayer.InstanceId == id)
                return _gameplayManager.CurrentPlayer;

            if (_gameplayManager.OpponentPlayer.InstanceId == id)
                return _gameplayManager.OpponentPlayer;

            BoardUnitModel boardUnitModelById = GetBoardUnitModelByInstanceId(id);
            if (boardUnitModelById != null)
                return boardUnitModelById;

            BoardUnitModel card = GetBoardUnitModelByInstanceId(id);
            if (card != null)
            {
                BoardCardView boardCardView = CreateCustomHandBoardCard(card);
                Object.Destroy(boardCardView.GameObject);
                return boardCardView.HandBoardCard;
            }

            return null;
        }

        public List<BoardObject> GetTargetsByInstanceId(IList<Unit> targetUnits)
        {
            List<BoardObject> boardObjects = new List<BoardObject>();

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

        public BoardUnitModel GetBoardUnitModelByInstanceId(InstanceId id)
        {
            BoardUnitModel boardUnitModel =
                _gameplayManager.OpponentPlayer.CardsOnBoard
                    .Concat(_gameplayManager.CurrentPlayer.CardsOnBoard)
                    .Concat(_gameplayManager.CurrentPlayer.CardsInHand)
                    .Concat(_gameplayManager.OpponentPlayer.CardsInHand)
                    .Concat(_gameplayManager.CurrentPlayer.CardsInDeck)
                    .Concat(_gameplayManager.OpponentPlayer.CardsInDeck)
                    .FirstOrDefault(model => model != null && model.Card.InstanceId == id);

            return boardUnitModel;
        }

        public BoardObject GetBoardObjectByInstanceId(InstanceId id)
        {
            BoardUnitModel boardUnitModel = GetBoardUnitModelByInstanceId(id);
            if(boardUnitModel != null)
                return boardUnitModel;

            List<BoardObject> boardObjects = new List<BoardObject>
            {
                _gameplayManager.CurrentPlayer,
                _gameplayManager.OpponentPlayer,
            };
            boardObjects.AddRange(_gameplayManager.CurrentPlayer.BoardItemsInUse);
            boardObjects.AddRange(_gameplayManager.OpponentPlayer.BoardItemsInUse);

            BoardObject foundObject = boardObjects.Find(boardObject =>
            {
                switch (boardObject)
                {
                    case BoardItem boardItem:
                        return boardItem.Model.InstanceId == id;
                    case IInstanceIdOwner instanceIdOwner:
                        return instanceIdOwner.InstanceId == id;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(boardObject), boardObject, null);
                }
            });

            return foundObject;
        }

        public List<BoardUnitModel> GetAdjacentUnitsToUnit(BoardUnitModel targetUnit)
        {
            IReadOnlyList<BoardUnitModel> boardCards = targetUnit.OwnerPlayer.CardsOnBoard;

            int targetIndex = boardCards.IndexOf(targetUnit);

            return boardCards.Where(unit =>
                    unit != targetUnit &&
                    (boardCards.IndexOf(unit) == Mathf.Clamp(targetIndex - 1, 0, boardCards.Count - 1) ||
                        boardCards.IndexOf(unit) == Mathf.Clamp(targetIndex + 1, 0, boardCards.Count - 1) &&
                        boardCards.IndexOf(unit) != targetIndex)
                )
                .ToList();
        }

        public BoardCardView CreateCustomHandBoardCard(BoardUnitModel boardUnitModel)
        {
            BoardCardView boardCardView = new UnitBoardCard(Object.Instantiate(_cardsController.CreatureCardViewPrefab), boardUnitModel);
            boardCardView.GameObject.transform.position = boardUnitModel.OwnerPlayer.IsLocalPlayer ? Constants.DefaultPositionOfPlayerBoardCard :
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
                _gameplayManager.CurrentPlayer.PlayerCardsController.AddCardToHand(new BoardUnitModel(card), true);
            }

            foreach (SpecificBattlegroundInfo.OverlordCardInfo cardInfo in opponentCards)
            {
                card = _cardsController.CreateWorkingCardFromCardName(cardInfo.Name, _gameplayManager.OpponentPlayer);
                card.TutorialObjectId = cardInfo.TutorialObjectId;
                _gameplayManager.OpponentPlayer.PlayerCardsController.AddCardToHand(new BoardUnitModel(card), true);
            }
        }

        private void SetupOverlordsDeckByPlayerAsSpecific(List<SpecificBattlegroundInfo.OverlordCardInfo> cards, Player player)
        {
            List<BoardUnitModel> boardUnitModels =
                cards
                    .Select(cardInfo =>
                    {
                        Card card = _dataManager.CachedCardsLibraryData.GetCardFromName(cardInfo.Name);
                        WorkingCard workingCard = new WorkingCard(card, card, player);
                        workingCard.TutorialObjectId = cardInfo.TutorialObjectId;
                        return new BoardUnitModel(workingCard);
                    })
                    .ToList();

           player.PlayerCardsController.SetCardsInDeck(boardUnitModels);
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
                workingUnitView.Model.CurrentDefense += cardInfo.BuffedDefense;
                workingUnitView.Model.BuffedDefense += cardInfo.BuffedDefense;
                workingUnitView.Model.CurrentDamage += cardInfo.BuffedDamage;
                workingUnitView.Model.BuffedDamage += cardInfo.BuffedDamage;
                RegisterBoardUnitView(_gameplayManager.CurrentPlayer, workingUnitView);
            }

            foreach (SpecificBattlegroundInfo.UnitOnBoardInfo cardInfo in opponentCards)
            {
                workingUnitView = _gameplayManager.CurrentPlayer.PlayerCardsController.SpawnUnitOnBoard(cardInfo.Name, ItemPosition.End);
                workingUnitView.Model.CantAttackInThisTurnBlocker = !cardInfo.IsManuallyPlayable;
                RegisterBoardUnitView(_gameplayManager.OpponentPlayer, workingUnitView);
            }
        }

        private void SetupGeneralUIAsSpecific(SpecificBattlegroundInfo specificBattlegroundInfo)
        {
            // todo implement logic
        }

        private void OnGameInitializedHandler()
        {
        }
    }

    #endregion
}
