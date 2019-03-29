using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.View;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class PlayerCardsController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(PlayerCardsController));

        public event Action<int> DeckChanged;

        public event Action<int> HandChanged;

        public event Action<int> GraveyardChanged;

        public event Action<int> BoardChanged;

        public event Action<IReadOnlyList<BoardUnitModel>> MulliganStarted;

        public Player Player { get; }

        public Player OpponentPlayer => _gameplayManager.GetOpponentByPlayer(Player);

        public IReadOnlyList<BoardUnitModel> CardsInDeck => _cardsInDeck;

        public IReadOnlyList<BoardUnitModel> CardsInGraveyard => _cardsInGraveyard;

        public IReadOnlyList<BoardUnitModel> CardsInHand => _cardsInHand;

        public IReadOnlyList<BoardUnitModel> CardsOnBoard => _cardsOnBoard;

        public IReadOnlyList<BoardUnitModel> MulliganCards => _mulliganCards;

        private const int DefaultIndexCustomCardForTutorial = -1;

        private readonly IGameplayManager _gameplayManager;

        private readonly ITimerManager _timerManager;

        private readonly IDataManager _dataManager;

        private readonly ISoundManager _soundManager;

        private readonly ITutorialManager _tutorialManager;

        private readonly IMatchManager _matchManager;

        private readonly BattlegroundController _battlegroundController;

        private readonly VfxController _vfxController;

        private readonly AbilitiesController _abilitiesController;

        private readonly ActionsQueueController _actionsQueueController;

        private readonly AnimationsController _animationsController;

        private readonly BoardController _boardController;

        private readonly CardsController _cardsController;

        private readonly UniquePositionedList<BoardUnitModel> _cardsInDeck = new UniquePositionedList<BoardUnitModel>(new PositionedList<BoardUnitModel>());
        private readonly UniquePositionedList<BoardUnitModel> _cardsInGraveyard = new UniquePositionedList<BoardUnitModel>(new PositionedList<BoardUnitModel>());
        private readonly UniquePositionedList<BoardUnitModel> _cardsInHand = new UniquePositionedList<BoardUnitModel>(new PositionedList<BoardUnitModel>());
        private readonly UniquePositionedList<BoardUnitModel> _cardsOnBoard = new UniquePositionedList<BoardUnitModel>(new PositionedList<BoardUnitModel>());
        private readonly UniquePositionedList<BoardUnitModel> _mulliganCards = new UniquePositionedList<BoardUnitModel>(new PositionedList<BoardUnitModel>());

        public PlayerCardsController(Player player)
        {
            Player = player;

            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _matchManager = GameClient.Get<IMatchManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _vfxController = _gameplayManager.GetController<VfxController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _animationsController = _gameplayManager.GetController<AnimationsController>();
            _boardController = _gameplayManager.GetController<BoardController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
        }

        #region Cards Lists Manipulation

        #region Mulligan

        public void SetCardsPreparingToHand(IReadOnlyList<BoardUnitModel> cards)
        {
            _mulliganCards.Clear();
            _mulliganCards.InsertRange(ItemPosition.End, cards);
        }

        private void InvokeMulliganStarted()
        {
            MulliganStarted?.Invoke(MulliganCards);
        }

        #endregion

        #region Deck

        public void AddCardToDeck(BoardUnitModel boardUnitModel, bool shuffle = false)
        {
            CallLog($"{nameof(AddCardToDeck)}(BoardUnitModel boardUnitModel = {boardUnitModel}, bool shuffle = {shuffle})");
            
            ItemPosition position = shuffle ? (ItemPosition) MTwister.IRandom(0, _cardsInDeck.Count) : ItemPosition.End;
            _cardsInDeck.Insert(position, boardUnitModel);
            InvokeDeckChanged();
        }

        public void RemoveCardFromDeck(BoardUnitModel boardUnitModel)
        {
            CallLog($"{nameof(RemoveCardFromDeck)}(BoardUnitModel boardUnitModel = {boardUnitModel})");
            
            bool removed = _cardsInDeck.Remove(boardUnitModel);
            if (!removed)
            {
                CallLog($"{nameof(RemoveCardFromDeck)}: item {boardUnitModel} wasn't present in the list", true);
            }

            InvokeDeckChanged();
        }

        public void SetCardsInDeck(IReadOnlyList<BoardUnitModel> cards)
        {
            CallLog($"{nameof(SetCardsInDeck)}(IEnumerable<BoardUnitModel> cards = {FormatCallLogList(cards)})");
            
            _cardsInDeck.Clear();

            switch (_matchManager.MatchType)
            {
                case Enumerators.MatchType.LOCAL:
                    if (!_gameplayManager.IsTutorial)
                    {
                        List<BoardUnitModel> boardUnitModels = cards.ToList();
                        boardUnitModels.ShuffleList();
                        cards = boardUnitModels;
                    }

                    _cardsInDeck.InsertRange(ItemPosition.End, cards);

                    break;
                case Enumerators.MatchType.PVP:
                    _cardsInDeck.InsertRange(ItemPosition.Start, cards);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            InvokeDeckChanged();
        }

        public void ShuffleCardsInDeck()
        {
            _cardsInDeck.UnsafeGetUnderlyingList().ShuffleList();
        }

        private void InvokeDeckChanged()
        {
            DeckChanged?.Invoke(CardsInDeck.Count);
        }

        #endregion

        #region Hand

        public IView AddCardFromDeckToHand(BoardUnitModel boardUnitModel = null, bool removeCardsFromDeck = true)
        {
            CallLog($"{nameof(AddCardFromDeckToHand)}(BoardUnitModel boardUnitModel = {boardUnitModel}, bool removeCardsFromDeck = {removeCardsFromDeck})");
            
            if (boardUnitModel == null)
            {
                if (CardsInDeck.Count == 0)
                {
                    if (!_tutorialManager.IsTutorial || (_tutorialManager.CurrentTutorial != null && _tutorialManager.IsLastTutorial))
                    {
                        Player.DamageByNoMoreCardsInDeck++;
                        Player.Defense -= Player.DamageByNoMoreCardsInDeck;
                        _vfxController.SpawnGotDamageEffect(Player, -Player.DamageByNoMoreCardsInDeck);
                    }

                    CallLog($"{nameof(AddCardFromDeckToHand)} returned null");
                    return null;
                }

                boardUnitModel = CardsInDeck[0];
            }

            if (removeCardsFromDeck)
            {
                RemoveCardFromDeck(boardUnitModel);
            }

            if (CheckIsMoreThanMaxCards(boardUnitModel))
            {
                CallLog($"{nameof(AddCardFromDeckToHand)} returned null");
                AddCardToGraveyard(boardUnitModel);
                return null;
            }

            IView cardView = AddCardToHand(boardUnitModel);
            CallLog($"{nameof(AddCardFromDeckToHand)} returned {cardView}");
            return cardView;
        }

        public IView AddCardToHand(BoardUnitModel boardUnitModel, bool silent = false)
        {
            CallLog($"{nameof(AddCardToHand)}(BoardUnitModel boardUnitModel = {boardUnitModel}, bool silent = {silent})");
            
            IView cardView;
            _cardsInHand.Insert(ItemPosition.End, boardUnitModel);

            if (Player.IsLocalPlayer)
            {
                cardView = CreateAndAddPlayerHandCard(boardUnitModel);
                _battlegroundController.UpdatePositionOfCardsInPlayerHand();
            }
            else
            {
                cardView = CreateAndAddOpponentHandCard(boardUnitModel);
                _battlegroundController.UpdatePositionOfCardsInOpponentHand(true, !silent);
            }

            InvokeHandChanged();
            CallLog($"{nameof(AddCardToHand)} returned {cardView}");

            return cardView;
        }

        public void AddCardFromBoardToHand(BoardUnitModel boardUnitModel)
        {
            CallLog($"{nameof(AddCardFromBoardToHand)}(BoardUnitModel boardUnitModel = {boardUnitModel}");
            _cardsInHand.Insert(ItemPosition.End, boardUnitModel);
            boardUnitModel.Owner.PlayerCardsController.RemoveCardFromBoard(boardUnitModel, false);
        }

        private BoardCardView CreateAndAddPlayerHandCard(BoardUnitModel boardUnitModel, bool silent = false)
        {
            CallLog($"{nameof(CreateAndAddPlayerHandCard)}(BoardUnitModel boardUnitModel = {boardUnitModel}, bool silent = {silent})");

            BoardCardView boardCardView = CreateBoardCard(boardUnitModel);

            if (_battlegroundController.CurrentTurn == 0)
            {
                boardCardView.SetDefaultAnimation();
            }

            _battlegroundController.RegisterBoardUnitView(boardCardView, boardCardView.Model.OwnerPlayer);

            if (silent)
            {
                boardCardView.HandBoardCard.Enabled = false;

                _timerManager.AddTimer(
                    x =>
                    {
                        boardCardView.HandBoardCard.Enabled = true;
                        boardCardView.HandBoardCard.CheckStatusOfHighlight();
                    },
                    null,
                    2f);
            }
            else
            {
                boardCardView.HandBoardCard.CheckStatusOfHighlight();
            }

            CallLog($"{nameof(CreateAndAddPlayerHandCard)} returned {boardCardView}");
            return boardCardView;
        }

        private OpponentHandCard CreateAndAddOpponentHandCard(BoardUnitModel boardUnitModel)
        {
            CallLog($"{nameof(CreateAndAddOpponentHandCard)}(BoardUnitModel boardUnitModel = {boardUnitModel})");

            OpponentHandCard opponentHandCard = CreateOpponentHandCard(boardUnitModel);

            _battlegroundController.RegisterBoardUnitView(opponentHandCard, boardUnitModel.OwnerPlayer);
            _abilitiesController.CallAbilitiesInHand(null, boardUnitModel);

            CallLog($"{nameof(CreateAndAddPlayerHandCard)} returned {opponentHandCard}");
            return opponentHandCard;
        }

        private OpponentHandCard CreateOpponentHandCard(BoardUnitModel boardUnitModel)
        {
            CallLog($"{nameof(CreateOpponentHandCard)}(BoardUnitModel boardUnitModel = {boardUnitModel})");

            Player opponent = _gameplayManager.OpponentPlayer;
            GameObject go = Object.Instantiate(_cardsController.OpponentCardPrefab);
            go.GetComponent<SortingGroup>().sortingOrder = opponent.CardsInHand.Count;
            OpponentHandCard opponentHandCard = new OpponentHandCard(go, boardUnitModel);

            CallLog($"{nameof(CreateOpponentHandCard)} returned {opponentHandCard}");
            return opponentHandCard;
        }

        public void AddCardToHandFromOtherPlayerDeck(BoardUnitModel boardUnitModel = null)
        {
            CallLog($"{nameof(AddCardToHandFromOtherPlayerDeck)}(BoardUnitModel boardUnitModel = {boardUnitModel})");
            
            if (boardUnitModel == null)
            {
                if (OpponentPlayer.CardsInDeck.Count == 0)
                {
                    if (!_tutorialManager.IsTutorial)
                    {
                        OpponentPlayer.DamageByNoMoreCardsInDeck++;
                        OpponentPlayer.Defense -= OpponentPlayer.DamageByNoMoreCardsInDeck;
                        _vfxController.SpawnGotDamageEffect(OpponentPlayer, -OpponentPlayer.DamageByNoMoreCardsInDeck);
                    }

                    return;
                }

                boardUnitModel = OpponentPlayer.CardsInDeck[0];
            }

            OpponentPlayer.PlayerCardsController.RemoveCardFromDeck(boardUnitModel);

            if (CheckIsMoreThanMaxCards(boardUnitModel))
                return;

            if (Player == OpponentPlayer)
            {
                AddCardToHand(boardUnitModel);
            }
            else
            {
                AddCardToHandFromOpponentDeck(boardUnitModel);
            }

            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
            {
                //await _gameplayManager.GetController<OpponentController>().ActionDrawCard(player, otherPlayer, player, Enumerators.AffectObjectType.Types.Enum.Player, card.Prototype.Name);
                _mulliganCards?.Insert(ItemPosition.End, boardUnitModel);
            }
        }

        public void AddCardToHandFromOpponentDeck(BoardUnitModel boardUnitModel)
        {
            CallLog($"{nameof(AddCardToHandFromOpponentDeck)}(BoardUnitModel boardUnitModel = {boardUnitModel})");
            
            boardUnitModel.Card.Owner = Player;
            _cardsInHand.Insert(ItemPosition.End, boardUnitModel);

            if (Player.IsLocalPlayer)
            {
                _animationsController.MoveCardFromPlayerDeckToPlayerHandAnimation(
                    OpponentPlayer,
                    Player,
                    CreateBoardCard(boardUnitModel));
            }
            else
            {
                _animationsController.MoveCardFromPlayerDeckToOpponentHandAnimation(
                    OpponentPlayer,
                    Player,
                    CreateOpponentHandCard(boardUnitModel)
                );
            }

            InvokeHandChanged();
        }

        public void RemoveCardFromHand(BoardUnitModel boardUnitModel, bool silent = false)
        {
            CallLog($"{nameof(RemoveCardFromHand)}(BoardUnitModel boardUnitModel = {boardUnitModel}, bool silent = {silent})");
            
            _cardsInHand.Remove(boardUnitModel);

            if (Player.IsLocalPlayer)
            {
                if (!silent)
                {
                    _battlegroundController.UpdatePositionOfCardsInPlayerHand();
                }
            }

            InvokeHandChanged();
        }

        public void SetCardsInHand(IReadOnlyList<BoardUnitModel> cards)
        {
            CallLog($"{nameof(SetCardsInHand)}(IEnumerable<BoardUnitModel> cards = {FormatCallLogList(cards)})");

            _cardsInHand.Clear();
            _cardsInHand.InsertRange(ItemPosition.Start, cards);

            InvokeHandChanged();
        }

        public void SetFirstHandForLocalMatch(bool skip)
        {
            CallLog($"{nameof(SetFirstHandForLocalMatch)}(bool skip = {skip})");

            if (skip)
                return;

            for (int i = 0; i < Player.InitialCardsInHandCount; i++)
            {
                if (i >= CardsInDeck.Count)
                    break;

                if (Player.IsLocalPlayer && (!_gameplayManager.IsTutorial ||
                    (_gameplayManager.IsTutorial &&
                        _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent()
                            .SpecificBattlegroundInfo.DisabledInitialization)))
                {
                    _mulliganCards.Insert(ItemPosition.End, CardsInDeck[i]);
                }
                else
                {
                    AddCardFromDeckToHand(CardsInDeck[0]);
                }
            }

            InvokeMulliganStarted();
        }

        public void SetFirstHandForPvPMatch(IReadOnlyList<BoardUnitModel> boardUnitModels, bool removeCardsFromDeck = true)
        {
            CallLog($"{nameof(SetFirstHandForPvPMatch)}(IReadOnlyList<BoardUnitModel> boardUnitModels = {FormatCallLogList(boardUnitModels)}], bool removeCardsFromDeck = {removeCardsFromDeck})");

            foreach (BoardUnitModel boardUnitModel in boardUnitModels)
            {
                if (Player.IsLocalPlayer && !_gameplayManager.IsTutorial)
                {
                    _mulliganCards.Insert(ItemPosition.End, boardUnitModel);
                }
                else
                {
                    AddCardFromDeckToHand(boardUnitModel, removeCardsFromDeck);
                }
            }

            InvokeMulliganStarted();
        }

        public BoardUnitModel CreateNewCardByNameAndAddToHand(string name)
        {
            CallLog($"{nameof(CreateNewCardByNameAndAddToHand)}(string name = {name})");

            float animationDuration = 1.5f;

            Card card = new Card(_dataManager.CachedCardsLibraryData.GetCardFromName(name));
            WorkingCard workingCard = new WorkingCard(card, card, Player);
            if (_tutorialManager.IsTutorial)
            {
                workingCard.TutorialObjectId = DefaultIndexCustomCardForTutorial;
            }

            BoardUnitModel boardUnitModel = new BoardUnitModel(workingCard);

            if (CheckIsMoreThanMaxCards(boardUnitModel))
            {
                CallLog($"{nameof(CreateNewCardByNameAndAddToHand)} CheckIsMoreThanMaxCards == true, returned {boardUnitModel}");
                return boardUnitModel;
            }

            if (Player.IsLocalPlayer)
            {
                BoardCardView boardCardView = CreateBoardCard(boardUnitModel);

                boardCardView.Transform.position = Vector3.zero;
                boardCardView.Transform.localScale = Vector3.zero;

                boardCardView.Transform.DOScale(Vector3.one * .3f, animationDuration);

                InternalTools.DoActionDelayed(() =>
                    {
                        _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerCreatedNewCardAndMovedToHand);
                    },
                    animationDuration - Time.deltaTime);

                InternalTools.DoActionDelayed(() =>
                    {
                        _battlegroundController.RegisterBoardUnitView(boardCardView, boardCardView.Model.OwnerPlayer);
                        _cardsInHand.Insert(ItemPosition.End, boardUnitModel);

                        _battlegroundController.UpdatePositionOfCardsInPlayerHand(true);
                    },
                    animationDuration);
            }
            else
            {
                OpponentHandCard handCard = CreateAndAddOpponentHandCard(boardUnitModel);
                handCard.Transform.position = Vector3.zero;
                handCard.Transform.localScale = Vector3.zero;

                handCard.Transform.DOScale(Vector3.one, animationDuration);

                _timerManager.AddTimer(
                    x =>
                    {
                        _cardsInHand.Insert(ItemPosition.End, boardUnitModel);
                        _battlegroundController.UpdatePositionOfCardsInOpponentHand(true);
                    },
                    null,
                    animationDuration);
            }

            CallLog($"{nameof(CreateNewCardByNameAndAddToHand)} returned {boardUnitModel}");
            return boardUnitModel;
        }

        public void ReturnToHandBoardUnit(BoardUnitModel boardUnitModel, Vector3 cardPosition)
        {
            CallLog($"{nameof(ReturnToHandBoardUnit)}(BoardUnitModel boardUnitModel = {boardUnitModel}, Vector3 cardPosition = {cardPosition})");
            if (CheckIsMoreThanMaxCards(boardUnitModel))
                return;

            IView cardView = AddCardToHand(boardUnitModel, true);
            cardView.Transform.position = cardPosition;
            CallLog($"{nameof(ReturnToHandBoardUnit)}: created view {cardView.GetType().Name} for model {boardUnitModel}");

            if (Player.IsLocalPlayer)
            {
                cardView.Transform.localScale = new Vector3(0.25f, 0.25f, 0.25f); // size of the cards in hand
            }
        }

        public void SummonUnitFromHand(BoardCardView card, bool activateAbility)
        {
            CallLog($"{nameof(SummonUnitFromHand)}(BoardCardView card = {card}, bool activateAbility = {activateAbility})");

            IReadOnlyCard prototype = card.Model.Card.Prototype;

            card.Transform.DORotate(Vector3.zero, .1f);

            if (card.HandBoardCard != null)
            {
                card.HandBoardCard.Enabled = false;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND,
                Constants.CardsMoveSoundVolume);

            GameObject board = Player.IsLocalPlayer ? _cardsController.PlayerBoard : _cardsController.OpponentBoard;

            BoardUnitView boardUnitView = new BoardUnitView(card.Model, board.transform);
            boardUnitView.Transform.tag = Player.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.parent = board.transform;
            boardUnitView.Transform.position = new Vector2(Constants.DefaultPositonOfUnitWhenSpawn * Player.CardsOnBoard.Count, 0);

            CallLog($"{nameof(SummonUnitFromHand)}: created BoardUnitView {boardUnitView} for model {boardUnitView.Model}");

            OpponentHandCard opponentHandCard = null;

            if (activateAbility)
            {
                _abilitiesController.ActivateAbilitiesOnCard(boardUnitView.Model, card.Model, Player);
            }

            if (Player.IsLocalPlayer)
            {
                //_battlegroundController.PlayerHandCards.Remove(card);
                _battlegroundController.RegisterBoardUnitView(boardUnitView, _gameplayManager.CurrentPlayer);
            }
            else
            {
                opponentHandCard = _battlegroundController.GetBoardUnitViewByModel<OpponentHandCard>(card.Model);
                if (opponentHandCard != null)
                {
                    _battlegroundController.UnregisterBoardUnitView(opponentHandCard, opponentHandCard.Model.OwnerPlayer);
                }

                _battlegroundController.RegisterBoardUnitView(boardUnitView, _gameplayManager.OpponentPlayer);
            }

            AddCardToBoard(card.Model, ItemPosition.End);
            RemoveCardFromHand(card.Model);

            InternalTools.DoActionDelayed(() =>
                {
                    card.Model.Card.Owner.GraveyardCardsCount++;
                },
                1f);

            card.RemoveCardParticle.Play();

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam
            {
                ActionType = Enumerators.ActionType.PlayCardFromHand,
                Caller = boardUnitView.Model,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
            });

            _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitView.Model, true, true);

            if (!Player.IsLocalPlayer)
            {
                card.GameObject.SetActive(false);
            }

            if (Player.IsLocalPlayer)
            {
                _cardsController.RemoveCard(card);
            }
            else
            {
                if (opponentHandCard != null)
                {
                    _cardsController.RemoveOpponentCard(opponentHandCard);
                }
                else
                {
                    Object.Destroy(card.GameObject);
                }
            }

            InternalTools.DoActionDelayed(() =>
                {
                    boardUnitView.PlayArrivalAnimation();

                    _boardController.UpdateCurrentBoardOfPlayer(Player, null);
                },
                0.1f);
        }

        public void InvokeHandChanged()
        {
            HandChanged?.Invoke(CardsInHand.Count);
        }

        #endregion

        #region Board

        public void AddCardToBoard(BoardUnitModel boardUnitModel, ItemPosition position)
        {
            CallLog($"{nameof(AddCardToBoard)}(BoardUnitModel boardUnitModel = {boardUnitModel}, ItemPosition position = {position})");

            if (CardsOnBoard.Contains(boardUnitModel))
            {
                CallLog($"{nameof(AddCardToBoard)}: Attempt to add card {boardUnitModel} to CardsOnBoard when it is already added", true);
                return;
            }

            _cardsOnBoard.Insert(InternalTools.GetSafePositionToInsert(position, CardsOnBoard), boardUnitModel);
            InvokeBoardChanged();
        }

        public void RemoveCardFromBoard(BoardUnitModel boardUnitModel, bool moveToGraveyard = true)
        {
            CallLog($"{nameof(RemoveCardFromBoard)}(BoardUnitModel boardUnitModel = {boardUnitModel}, bool moveToGraveyard = {moveToGraveyard})");

            _cardsOnBoard.Remove(boardUnitModel);

            if (moveToGraveyard)
            {
                if (Player.IsLocalPlayer)
                {
                    _battlegroundController.RemovePlayerCardFromBoardToGraveyard(boardUnitModel);
                }
                else
                {
                    _battlegroundController.RemoveOpponentCardFromBoardToGraveyard(boardUnitModel);
                }
            }

            InvokeBoardChanged();
        }

        public void ClearCardsOnBoard()
        {
            CallLog($"{nameof(ClearCardsOnBoard)}()");

            _cardsOnBoard.Clear();
        }
        
        public void ClearCardsInHand()
        {
            CallLog($"{nameof(ClearCardsInHand)}()");

            _cardsInHand.Clear();
        }

        public BoardUnitView SpawnUnitOnBoard(string name, ItemPosition position, bool isPVPNetwork = false, Action onComplete = null)
        {
            CallLog($"{nameof(SpawnUnitOnBoard)}(string name = {name}, ItemPosition position = {position}, bool isPVPNetwork = {isPVPNetwork}, Action onComplete = {onComplete})");

            if (CardsOnBoard.Count >= Player.MaxCardsInPlay)
            {
                CallLog($"{nameof(SpawnUnitOnBoard)}: CardsOnBoard.Count >= Player.MaxCardsInPlay, returned null");
                return null;
            }

            Card prototype = new Card(_dataManager.CachedCardsLibraryData.GetCardFromName(name));
            WorkingCard card = new WorkingCard(prototype, prototype, Player);
            BoardUnitModel boardUnitModel = new BoardUnitModel(card);

            BoardUnitView view = SpawnUnitOnBoard(boardUnitModel, position, isPVPNetwork, onComplete);
            CallLog($"{nameof(SpawnUnitOnBoard)}: created and returned unit view {view}");
            return view;
        }

        public BoardUnitView SpawnUnitOnBoard(
            BoardUnitModel boardUnitModel,
            ItemPosition position,
            bool isPVPNetwork = false,
            Action onComplete = null)
        {
            CallLog($"{nameof(SpawnUnitOnBoard)}(BoardUnitModel boardUnitModel = {boardUnitModel}, ItemPosition position = {position}, bool isPVPNetwork = {isPVPNetwork}, Action onComplete = {onComplete})");

            if (CardsOnBoard.Count >= Player.MaxCardsInPlay)
            {
                CallLog($"{nameof(SpawnUnitOnBoard)}: CardsOnBoard.Count >= Player.MaxCardsInPlay, returned null");
                return null;
            }

            BoardUnitView unit = CreateBoardUnitForSpawn(boardUnitModel, Player);

            AddCardToBoard(boardUnitModel, position);

            if (isPVPNetwork)
            {
                _battlegroundController.RegisterBoardUnitView(unit, Player);
            }
            else
            {
                //Player.BoardCards.Insert(position, unit);
                _battlegroundController.RegisterBoardUnitView(unit, Player);
            }

            _abilitiesController.ResolveAllAbilitiesOnUnit(unit.Model);

            _boardController.UpdateCurrentBoardOfPlayer(Player, onComplete);

            CallLog($"{nameof(SpawnUnitOnBoard)}: created and returned unit view {unit}");
            return unit;
        }

        private void InvokeBoardChanged()
        {
            BoardChanged?.Invoke(CardsOnBoard.Count);
        }

        #endregion

        #region Graveyard

        public void AddCardToGraveyard(BoardUnitModel boardUnitModel)
        {
            if (CardsInGraveyard.Contains(boardUnitModel))
                return;

            _cardsInGraveyard.Insert(ItemPosition.End, boardUnitModel);

            InvokeGraveyardChanged();
        }

        public void RemoveCardFromGraveyard(BoardUnitModel boardUnitModel)
        {
            if (!CardsInGraveyard.Contains(boardUnitModel))
                return;

            _cardsInGraveyard.Remove(boardUnitModel);

            InvokeGraveyardChanged();
        }

        private void InvokeGraveyardChanged()
        {
            GraveyardChanged?.Invoke(CardsInGraveyard.Count);
        }

        #endregion

        #endregion

        public void TakeControlOfUnit(BoardUnitModel unit)
        {
            CallLog($"{nameof(TakeControlOfUnit)}(BoardUnitModel unit = {unit})");

            unit.OwnerPlayer.PlayerCardsController.RemoveCardFromBoard(unit, false);
            unit.Card.Owner = Player;

            _cardsOnBoard.Insert(ItemPosition.End, unit);
        }

        public bool CheckIsMoreThanMaxCards(BoardUnitModel boardUnitModel)
        {
            if (CardsInHand.Count >= Player.MaxCardsInHand)
            {
                return true;
            }

            return false;
        }

        private BoardCardView CreateBoardCard(BoardUnitModel boardUnitModel)
        {
            GameObject go;
            BoardCardView boardCardView;
            switch (boardUnitModel.Card.Prototype.Kind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(_cardsController.CreatureCardViewPrefab);
                    boardCardView = new UnitBoardCard(go, boardUnitModel);
                    break;
                case Enumerators.CardKind.ITEM:
                    go = Object.Instantiate(_cardsController.ItemCardViewPrefab);
                    boardCardView = new ItemBoardCard(go, boardUnitModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            HandBoardCard handCard = new HandBoardCard(go, boardCardView);
            handCard.BoardZone = _cardsController.PlayerBoard;
            boardCardView.HandBoardCard = handCard;
            handCard.CheckStatusOfHighlight();
            boardCardView.Transform.localScale = Vector3.one * .3f;

            _abilitiesController.CallAbilitiesInHand(boardCardView, boardUnitModel);

            return boardCardView;
        }

        private BoardUnitView CreateBoardUnitForSpawn(BoardUnitModel boardUnitModel, Player owner)
        {
            GameObject playerBoard = owner.IsLocalPlayer ?
                _battlegroundController.PlayerBoardObject :
                _battlegroundController.OpponentBoardObject;

            float unitYPositionOnBoard = owner.IsLocalPlayer ? -1.66f : 1.66f;

            if (boardUnitModel.Card.Owner != owner)
                throw new Exception("card.Owner != owner, shouldn't those be the same");

            BoardUnitView boardUnitView = new BoardUnitView(boardUnitModel, playerBoard.transform);
            boardUnitView.Transform.tag = owner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.parent = playerBoard.transform;
            boardUnitView.Transform.position = new Vector2(2f * owner.CardsOnBoard.Count, unitYPositionOnBoard);

            boardUnitView.PlayArrivalAnimation();

            return boardUnitView;
        }

        private void CallLog(string message, bool isWarning = false)
        {
            message = (Player.IsLocalPlayer ? "CurrentPlayer." : "OpponentPlayer.") + message;
            if (isWarning)
            {
                Log.Warn(message);
            }
            else
            {
                Log.Debug(message);
            }
        }

        private static string FormatCallLogList<T>(IReadOnlyList<T> list)
        {
            return $"[({list.Count} items) {String.Join(", ", list)}]";
        }
    }
}
