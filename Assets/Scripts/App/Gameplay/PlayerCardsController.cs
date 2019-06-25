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

        public event Action<IReadOnlyList<CardModel>> MulliganStarted;

        public Player Player { get; }

        public Player OpponentPlayer => _gameplayManager.GetOpponentByPlayer(Player);

        public IReadOnlyList<CardModel> CardsInDeck => _cardsInDeck;

        public IReadOnlyList<CardModel> CardsInGraveyard => _cardsInGraveyard;

        public IReadOnlyList<CardModel> CardsInHand => _cardsInHand;

        public IReadOnlyList<CardModel> CardsOnBoard => _cardsOnBoard;

        public IReadOnlyList<CardModel> MulliganCards => _mulliganCards;

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

        private readonly ActionsReportController _actionsReportController;

        private readonly AnimationsController _animationsController;

        private readonly BoardController _boardController;

        private readonly CardsController _cardsController;

        private readonly UniquePositionedList<CardModel> _cardsInDeck = new UniquePositionedList<CardModel>(new PositionedList<CardModel>());
        private readonly UniquePositionedList<CardModel> _cardsInGraveyard = new UniquePositionedList<CardModel>(new PositionedList<CardModel>());
        private readonly UniquePositionedList<CardModel> _cardsInHand = new UniquePositionedList<CardModel>(new PositionedList<CardModel>());
        private readonly UniquePositionedList<CardModel> _cardsOnBoard = new UniquePositionedList<CardModel>(new PositionedList<CardModel>());
        private readonly UniquePositionedList<CardModel> _mulliganCards = new UniquePositionedList<CardModel>(new PositionedList<CardModel>());

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
            _actionsReportController = _gameplayManager.GetController<ActionsReportController>();
            _animationsController = _gameplayManager.GetController<AnimationsController>();
            _boardController = _gameplayManager.GetController<BoardController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
        }

        #region Cards Lists Manipulation

        #region Mulligan

        public int GetCardsOnBoardCount (bool filterOutDeadCards = false)
        {
            if (filterOutDeadCards) 
            {
                int count = 0;
                for (int i = CardsOnBoard.Count-1; i >= 0; i--)
                {
                    if (CardsOnBoard[i].IsDead == false && CardsOnBoard[i].CurrentDefense > 0)
                    {
                        count++;
                    }
                }

                return count;
            }

            return CardsOnBoard.Count;
        }

        public void SetCardsPreparingToHand(IReadOnlyList<CardModel> cards)
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

        public void AddCardToDeck(CardModel cardModel, bool shuffle = false)
        {
            CallLog($"{nameof(AddCardToDeck)}(CardModel cardModel = {cardModel}, bool shuffle = {shuffle})");
            
            ItemPosition position = shuffle ? (ItemPosition) MTwister.IRandom(0, _cardsInDeck.Count) : ItemPosition.End;
            _cardsInDeck.Insert(position, cardModel);
            InvokeDeckChanged();
        }

        public void RemoveCardFromDeck(CardModel cardModel)
        {
            CallLog($"{nameof(RemoveCardFromDeck)}(CardModel cardModel = {cardModel})");
            
            bool removed = _cardsInDeck.Remove(cardModel);
            if (!removed)
            {
                CallLog($"{nameof(RemoveCardFromDeck)}: item {cardModel} wasn't present in the list", true);
            }

            InvokeDeckChanged();
        }

        public void SetCardsInDeck(IReadOnlyList<CardModel> cards)
        {
            CallLog($"{nameof(SetCardsInDeck)}(IEnumerable<CardModel> cards = {Utilites.FormatCallLogList(cards)})");
            
            _cardsInDeck.Clear();

            switch (_matchManager.MatchType)
            {
                case Enumerators.MatchType.LOCAL:
                    if (!_gameplayManager.IsTutorial)
                    {
                        List<CardModel> cardModels = cards.ToList();
                        cardModels.ShuffleList();
                        cards = cardModels;
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

        public IView AddCardFromDeckToHand(CardModel cardModel = null, bool removeCardsFromDeck = true)
        {
            CallLog($"{nameof(AddCardFromDeckToHand)}(CardModel cardModel = {cardModel}, bool removeCardsFromDeck = {removeCardsFromDeck})");
            
            if (cardModel == null)
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

                cardModel = CardsInDeck[0];
            }

            if (removeCardsFromDeck)
            {
                RemoveCardFromDeck(cardModel);
            }

            if (CheckIsMoreThanMaxCards(cardModel))
            {
                CallLog($"{nameof(AddCardFromDeckToHand)} returned null");
                AddCardToGraveyard(cardModel);
                return null;
            }

            IView cardView = AddCardToHand(cardModel);
            CallLog($"{nameof(AddCardFromDeckToHand)} returned {cardView}");
            return cardView;
        }

        public IView AddCardToHand(CardModel cardModel, bool silent = false)
        {
            CallLog($"{nameof(AddCardToHand)}(CardModel cardModel = {cardModel}, bool silent = {silent})");
            
            IView cardView;
            _cardsInHand.Insert(ItemPosition.End, cardModel);

            if (Player.IsLocalPlayer)
            {
                cardView = CreateAndAddPlayerHandCard(cardModel);
                _battlegroundController.UpdatePositionOfCardsInPlayerHand();
            }
            else
            {
                cardView = CreateAndAddOpponentHandCard(cardModel);
                _battlegroundController.UpdatePositionOfCardsInOpponentHand(true, !silent);
            }

            InvokeHandChanged();
            CallLog($"{nameof(AddCardToHand)} returned {cardView}");

            return cardView;
        }

        public void AddCardFromBoardToHand(CardModel cardModel)
        {
            CallLog($"{nameof(AddCardFromBoardToHand)}(CardModel cardModel = {cardModel}");
            _cardsInHand.Insert(ItemPosition.End, cardModel);
            cardModel.Owner.PlayerCardsController.RemoveCardFromBoard(cardModel, false);
        }

        private BoardCardView CreateAndAddPlayerHandCard(CardModel cardModel, bool silent = false)
        {
            CallLog($"{nameof(CreateAndAddPlayerHandCard)}(CardModel cardModel = {cardModel}, bool silent = {silent})");

            BoardCardView boardCardView = CreateBoardCard(cardModel);

            if (_battlegroundController.CurrentTurn == 0)
            {
                boardCardView.SetDefaultAnimation();
            }

            _battlegroundController.RegisterCardView(boardCardView, boardCardView.Model.OwnerPlayer);

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

        private OpponentHandCardView CreateAndAddOpponentHandCard(CardModel cardModel)
        {
            CallLog($"{nameof(CreateAndAddOpponentHandCard)}(CardModel cardModel = {cardModel})");

            OpponentHandCardView opponentHandCard = CreateOpponentHandCard(cardModel);

            _battlegroundController.RegisterCardView(opponentHandCard, cardModel.OwnerPlayer);
            _abilitiesController.CallAbilitiesInHand(cardModel);

            CallLog($"{nameof(CreateAndAddPlayerHandCard)} returned {opponentHandCard}");
            return opponentHandCard;
        }

        private OpponentHandCardView CreateOpponentHandCard(CardModel cardModel)
        {
            CallLog($"{nameof(CreateOpponentHandCard)}(CardModel cardModel = {cardModel})");

            Player opponent = _gameplayManager.OpponentPlayer;
            GameObject go = Object.Instantiate(_cardsController.OpponentCardPrefab);
            go.GetComponent<SortingGroup>().sortingOrder = opponent.CardsInHand.Count;
            OpponentHandCardView opponentHandCard = new OpponentHandCardView(go, cardModel);

            CallLog($"{nameof(CreateOpponentHandCard)} returned {opponentHandCard}");
            return opponentHandCard;
        }

        public void AddCardToHandFromOtherPlayerDeck(CardModel cardModel = null)
        {
            CallLog($"{nameof(AddCardToHandFromOtherPlayerDeck)}(CardModel cardModel = {cardModel})");
            
            if (cardModel == null)
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

                cardModel = OpponentPlayer.CardsInDeck[0];
            }

            OpponentPlayer.PlayerCardsController.RemoveCardFromDeck(cardModel);

            if (CheckIsMoreThanMaxCards(cardModel))
                return;

            if (Player == OpponentPlayer)
            {
                AddCardToHand(cardModel);
            }
            else
            {
                AddCardToHandFromOpponentDeck(cardModel);
            }

            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
            {
                //await _gameplayManager.GetController<OpponentController>().ActionDrawCard(player, otherPlayer, player, Enumerators.AffectObjectType.Types.Enum.Player, card.Prototype.Name);
                _mulliganCards?.Insert(ItemPosition.End, cardModel);
            }
        }

        public void AddCardToHandFromOpponentDeck(CardModel cardModel)
        {
            CallLog($"{nameof(AddCardToHandFromOpponentDeck)}(CardModel cardModel = {cardModel})");
            
            cardModel.Card.Owner = Player;
            _cardsInHand.Insert(ItemPosition.End, cardModel);

            if (Player.IsLocalPlayer)
            {
                _animationsController.MoveCardFromPlayerDeckToPlayerHandAnimation(
                    OpponentPlayer,
                    Player,
                    CreateBoardCard(cardModel));
            }
            else
            {
                _animationsController.MoveCardFromPlayerDeckToOpponentHandAnimation(
                    OpponentPlayer,
                    Player,
                    CreateOpponentHandCard(cardModel)
                );
            }

            InvokeHandChanged();
        }

        public void RemoveCardFromHand(CardModel cardModel, bool silent = false)
        {
            CallLog($"{nameof(RemoveCardFromHand)}(CardModel cardModel = {cardModel}, bool silent = {silent})");
            
            _cardsInHand.Remove(cardModel);

            if (Player.IsLocalPlayer)
            {
                if (!silent)
                {
                    _battlegroundController.UpdatePositionOfCardsInPlayerHand();
                }
            }

            InvokeHandChanged();
        }

        public void SetCardsInHand(IReadOnlyList<CardModel> cards)
        {
            CallLog($"{nameof(SetCardsInHand)}(IEnumerable<CardModel> cards = {Utilites.FormatCallLogList(cards)})");

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

        public void SetFirstHandForPvPMatch(IReadOnlyList<CardModel> cardModels, bool removeCardsFromDeck = true)
        {
            CallLog($"{nameof(SetFirstHandForPvPMatch)}(IReadOnlyList<CardModel> cardModels = {Utilites.FormatCallLogList(cardModels)}], bool removeCardsFromDeck = {removeCardsFromDeck})");

            foreach (CardModel cardModel in cardModels)
            {
                if (Player.IsLocalPlayer && !_gameplayManager.IsTutorial)
                {
                    _mulliganCards.Insert(ItemPosition.End, cardModel);
                }
                else
                {
                    AddCardFromDeckToHand(cardModel, removeCardsFromDeck);
                }
            }

            InvokeMulliganStarted();
        }

        public CardModel CreateNewCardAndAddToHand(Card card)
        {
            CallLog($"{nameof(CreateNewCardAndAddToHand)}(Card card = {card})");

            const float animationDuration = 1.5f;
            WorkingCard workingCard = new WorkingCard(card, card, Player);
            if (_tutorialManager.IsTutorial)
            {
                workingCard.TutorialObjectId = DefaultIndexCustomCardForTutorial;
            }

            CardModel cardModel = new CardModel(workingCard);

            if (CheckIsMoreThanMaxCards(cardModel))
            {
                CallLog($"{nameof(CreateNewCardAndAddToHand)} CheckIsMoreThanMaxCards == true, returned {cardModel}");
                return cardModel;
            }

            if (Player.IsLocalPlayer)
            {
                BoardCardView boardCardView = CreateBoardCard(cardModel);

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
                        _battlegroundController.RegisterCardView(boardCardView, boardCardView.Model.OwnerPlayer);
                        _cardsInHand.Insert(ItemPosition.End, cardModel);

                        _battlegroundController.UpdatePositionOfCardsInPlayerHand(true);
                    },
                    animationDuration);
            }
            else
            {
                OpponentHandCardView handCard = CreateAndAddOpponentHandCard(cardModel);
                handCard.Transform.position = Vector3.zero;
                handCard.Transform.localScale = Vector3.zero;

                handCard.Transform.DOScale(Vector3.one, animationDuration);

                _timerManager.AddTimer(
                    x =>
                    {
                        _cardsInHand.Insert(ItemPosition.End, cardModel);
                        _battlegroundController.UpdatePositionOfCardsInOpponentHand(true);
                    },
                    null,
                    animationDuration);
            }

            CallLog($"{nameof(CreateNewCardAndAddToHand)} returned {cardModel}");
            return cardModel;
        }

        public void ReturnToHandBoardUnit(CardModel cardModel, Vector3 cardPosition, int addToMaxCards = 0)
        {
            CallLog($"{nameof(ReturnToHandBoardUnit)}(CardModel cardModel = {cardModel}, Vector3 cardPosition = {cardPosition})");

            IView cardView = AddCardToHand(cardModel, true);
            cardView.GameObject.transform.position = cardPosition;
            CallLog($"{nameof(ReturnToHandBoardUnit)}: created view {cardView.GetType().Name} for model {cardModel}");

            if (Player.IsLocalPlayer)
            {
                cardView.GameObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f); // size of the cards in hand
            }

            if(!_gameplayManager.CurrentTurnPlayer.IsLocalPlayer &&
                cardView is BoardCardView boardCardView)
            {
                boardCardView.SetHighlightingEnabled(false);
            }

            if (CheckIsMoreThanMaxCards(addToMaxCards: addToMaxCards))
            {
                _cardsController.DiscardCardFromHand(cardModel);
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

            OpponentHandCardView opponentHandCard = null;

            if (activateAbility)
            {
                _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitView.Model, false);
                _abilitiesController.ActivateAbilitiesOnCard(boardUnitView.Model, card.Model, Player);
            }

            if (Player.IsLocalPlayer)
            {
                _battlegroundController.RegisterCardView(boardUnitView, _gameplayManager.CurrentPlayer);
            }
            else
            {
                opponentHandCard = _battlegroundController.GetCardViewByModel<OpponentHandCardView>(card.Model);
                if (opponentHandCard != null)
                {
                    _battlegroundController.UnregisterCardView(opponentHandCard, opponentHandCard.Model.OwnerPlayer);
                }

                _battlegroundController.RegisterCardView(boardUnitView, _gameplayManager.OpponentPlayer);
            }

            AddCardToBoard(card.Model, ItemPosition.End);
            RemoveCardFromHand(card.Model);

            InternalTools.DoActionDelayed(() =>
                {
                    card.Model.Card.Owner.GraveyardCardsCount++;
                },
                1f);

            card.RemoveCardParticle.Play();

            _actionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam
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

        public void AddCardToBoard(CardModel cardModel, ItemPosition position)
        {
            CallLog($"{nameof(AddCardToBoard)}(CardModel cardModel = {cardModel}, ItemPosition position = {position})");

            if (CardsOnBoard.Contains(cardModel))
            {
                CallLog($"{nameof(AddCardToBoard)}: Attempt to add card {cardModel} to CardsOnBoard when it is already added", true);
                return;
            }

            _cardsOnBoard.Insert(InternalTools.GetSafePositionToInsert(position, CardsOnBoard), cardModel);
            InvokeBoardChanged();
        }

        public void RemoveCardFromBoard(CardModel cardModel, bool moveToGraveyard = true)
        {
            CallLog($"{nameof(RemoveCardFromBoard)}(CardModel cardModel = {cardModel}, bool moveToGraveyard = {moveToGraveyard})");

            _cardsOnBoard.Remove(cardModel);

            if (moveToGraveyard)
            {
                if (Player.IsLocalPlayer)
                {
                    _battlegroundController.RemovePlayerCardFromBoardToGraveyard(cardModel);
                }
                else
                {
                    _battlegroundController.RemoveOpponentCardFromBoardToGraveyard(cardModel);
                }
            }

            InvokeBoardChanged();
        }

        public void ClearCardsOnBoard()
        {
            CallLog($"{nameof(ClearCardsOnBoard)}()");

            foreach(CardModel model in _cardsOnBoard)
            {
                _battlegroundController.GetCardViewByModel<BoardUnitView>(model).CancelTargetingArrows();
            }
            _cardsOnBoard.Clear();
        }
        
        public void ClearCardsInHand()
        {
            CallLog($"{nameof(ClearCardsInHand)}()");

            _cardsInHand.Clear();
        }

        public BoardUnitView SpawnUnitOnBoard(string name, ItemPosition position, bool isPVPNetwork = false, Action onComplete = null, bool checkCardsOnBoardCondition = true)
        {
            CallLog($"{nameof(SpawnUnitOnBoard)}(string name = {name}, ItemPosition position = {position}, bool isPVPNetwork = {isPVPNetwork}, Action onComplete = {onComplete})");

            if (checkCardsOnBoardCondition)
            {
                if (Player.PlayerCardsController.GetCardsOnBoardCount(true) >= Player.MaxCardsInPlay)
                {
                    CallLog($"{nameof(SpawnUnitOnBoard)}: Player.PlayerCardsController.GetCardsOnBoardCount(true) >= Player.MaxCardsInPlay, returned null");
                    return null;
                }
            }

            Card prototype = new Card(_dataManager.CachedCardsLibraryData.GetCardByName(name));
            WorkingCard card = new WorkingCard(prototype, prototype, Player);
            CardModel cardModel = new CardModel(card);

            BoardUnitView view = SpawnUnitOnBoard(cardModel, position, isPVPNetwork, onComplete, checkCardsOnBoardCondition);
            CallLog($"{nameof(SpawnUnitOnBoard)}: created and returned unit view {view}");
            return view;
        }

        public BoardUnitView SpawnUnitOnBoard(
            CardModel cardModel,
            ItemPosition position,
            bool isPVPNetwork = false,
            Action onComplete = null,
            bool checkCardsOnBoardCondition = true)
        {
            CallLog($"{nameof(SpawnUnitOnBoard)}(CardModel cardModel = {cardModel}, ItemPosition position = {position}, bool isPVPNetwork = {isPVPNetwork}, Action onComplete = {onComplete})");

            if (checkCardsOnBoardCondition)
            {
                if (Player.PlayerCardsController.GetCardsOnBoardCount(true) >= Player.MaxCardsInPlay)
                {
                    CallLog($"{nameof(SpawnUnitOnBoard)}: Player.PlayerCardsController.GetCardsOnBoardCount(true) >= Player.MaxCardsInPlay, returned null");
                    return null;
                }
            }

            BoardUnitView unit = CreateBoardUnitForSpawn(cardModel, Player);
            _battlegroundController.RegisterCardView(unit, Player);
            
            if (cardModel.Owner.IsLocalPlayer || _gameplayManager.IsLocalPlayerTurn()) 
            {
                _abilitiesController.ResolveAllAbilitiesOnUnit(cardModel, false);
                _abilitiesController.ActivateAbilitiesOnCard(cardModel, cardModel, cardModel.Owner);
            }

            _abilitiesController.ResolveAllAbilitiesOnUnit(cardModel);

            AddCardToBoard(cardModel, position);

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

        public void AddCardToGraveyard(CardModel cardModel)
        {
            if (CardsInGraveyard.Contains(cardModel))
                return;

            _cardsInGraveyard.Insert(ItemPosition.End, cardModel);

            InvokeGraveyardChanged();
        }

        public void RemoveCardFromGraveyard(CardModel cardModel)
        {
            if (!CardsInGraveyard.Contains(cardModel))
                return;

            _cardsInGraveyard.Remove(cardModel);

            InvokeGraveyardChanged();
        }

        private void InvokeGraveyardChanged()
        {
            GraveyardChanged?.Invoke(CardsInGraveyard.Count);
        }

        #endregion

        #endregion

        public void TakeControlOfUnit(CardModel unit)
        {
            CallLog($"{nameof(TakeControlOfUnit)}(CardModel unit = {unit})");

            unit.OwnerPlayer.PlayerCardsController.RemoveCardFromBoard(unit, false);
            unit.Card.Owner = Player;

            _cardsOnBoard.Insert(ItemPosition.End, unit);
            InvokeBoardChanged();
        }

        public bool CheckIsMoreThanMaxCards(CardModel cardModel = null, int addToMaxCards = 0)
        {
            if (CardsInHand.Count >= Player.MaxCardsInHand+addToMaxCards)
            {
                return true;
            }

            return false;
        }

        private BoardCardView CreateBoardCard(CardModel cardModel)
        {
            GameObject go;
            BoardCardView boardCardView;
            switch (cardModel.Card.Prototype.Kind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(_cardsController.CreatureCardViewPrefab);
                    boardCardView = new UnitBoardCardView(go, cardModel);
                    break;
                case Enumerators.CardKind.ITEM:
                    go = Object.Instantiate(_cardsController.ItemCardViewPrefab);
                    boardCardView = new ItemBoardCardView(go, cardModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            HandBoardCard handCard = new HandBoardCard(go, boardCardView);
            handCard.BoardZone = _cardsController.PlayerBoard;
            boardCardView.HandBoardCard = handCard;
            handCard.CheckStatusOfHighlight();
            boardCardView.Transform.localScale = Vector3.one * .2f;

            _battlegroundController.RegisterCardView(boardCardView);
            _abilitiesController.CallAbilitiesInHand(cardModel);

            return boardCardView;
        }

        private BoardUnitView CreateBoardUnitForSpawn(CardModel cardModel, Player owner)
        {
            GameObject playerBoard = owner.IsLocalPlayer ?
                _battlegroundController.PlayerBoardObject :
                _battlegroundController.OpponentBoardObject;

            float unitYPositionOnBoard = owner.IsLocalPlayer ? -1.66f : 1.66f;

            if (cardModel.Card.Owner != owner)
                throw new Exception("card.Owner != owner, shouldn't those be the same");

            BoardUnitView boardUnitView = new BoardUnitView(cardModel, playerBoard.transform);
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
    }
}
