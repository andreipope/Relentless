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
    public partial class Player
    {
        public class PlayerLocalCardsController
        {
            private static readonly ILog Log = Logging.GetLog(nameof(PlayerLocalCardsController));

            public Player Player { get; }

            public Player OpponentPlayer => _gameplayManager.GetOpponentByPlayer(Player);

            public UniquePositionedList<BoardItem> BoardItemsInUse => _boardItemsInUse;

            public IReadOnlyList<BoardUnitModel> CardsInDeck => _cardsInDeck;

            public IReadOnlyList<BoardUnitModel> CardsInGraveyard => _cardsInGraveyard;

            public IReadOnlyList<BoardUnitModel> CardsInHand => _cardsInHand;

            public IReadOnlyList<BoardUnitModel> CardsOnBoard => _cardsOnBoard;

            public IReadOnlyList<BoardUnitModel> CardsPreparingToHand => _cardsPreparingToHand;

            private const int DefaultIndexCustomCardForTutorial = -1;

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

            private readonly CardsController _cardsController;

            private readonly UniquePositionedList<BoardItem> _boardItemsInUse =
                new UniquePositionedList<BoardItem>(new PositionedList<BoardItem>());
            private readonly UniquePositionedList<BoardUnitModel> _cardsInDeck =
                new UniquePositionedList<BoardUnitModel>(new PositionedList<BoardUnitModel>());
            private readonly UniquePositionedList<BoardUnitModel> _cardsInGraveyard =
                new UniquePositionedList<BoardUnitModel>(new PositionedList<BoardUnitModel>());
            private readonly UniquePositionedList<BoardUnitModel> _cardsInHand =
                new UniquePositionedList<BoardUnitModel>(new PositionedList<BoardUnitModel>());
            private readonly UniquePositionedList<BoardUnitModel> _cardsOnBoard =
                new UniquePositionedList<BoardUnitModel>(new PositionedList<BoardUnitModel>());
            private readonly UniquePositionedList<BoardUnitModel> _cardsPreparingToHand =
                new UniquePositionedList<BoardUnitModel>(new PositionedList<BoardUnitModel>());

            public PlayerLocalCardsController(Player player)
            {
                Player = player;

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
                _cardsController = _gameplayManager.GetController<CardsController>();
            }

            #region Cards Lists Manipulation

            #region Deck

            public void AddCardToDeck(BoardUnitModel boardUnitModel, bool shuffle = false)
            {
                ItemPosition position = shuffle ? (ItemPosition) MTwister.IRandom(0, _cardsInDeck.Count) : ItemPosition.End;
                _cardsInDeck.Insert(position, boardUnitModel);
                InvokeDeckChanged();
            }

            public void RemoveCardFromDeck(BoardUnitModel boardUnitModel)
            {
                bool removed = _cardsInDeck.Remove(boardUnitModel);
                Assert.AreEqual(true, removed, $"Item {boardUnitModel} not removed");

                InvokeDeckChanged();
            }

            public void SetCardsInDeck(IEnumerable<BoardUnitModel> cards)
            {
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

                        int startInstanceId = Player == _gameplayManager.OpponentPlayer ? Constants.MinDeckSize : 0;

                        _cardsController.SetNewCardInstanceId(startInstanceId);
                        _cardsInDeck.InsertRange(ItemPosition.End, cards);

                        break;
                    case Enumerators.MatchType.PVP:
                        _cardsInDeck.InsertRange(ItemPosition.Start, cards);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Log.Info("Updating player cards");
                Log.Info(CardsInDeck.Count);

                InvokeDeckChanged();
            }

            public void ShuffleCardsInDeck()
            {
                _cardsInDeck.UnsafeGetUnderlyingList().ShuffleList();
            }

            private void InvokeDeckChanged()
            {
                Player.DeckChanged?.Invoke(CardsInDeck.Count);
            }

            #endregion

            #region Hand

            public IView AddCardFromDeckToHand(BoardUnitModel card = null, bool removeCardsFromDeck = true)
            {
                if (card == null)
                {
                    if (CardsInDeck.Count == 0)
                    {
                        if (!_tutorialManager.IsTutorial || (_tutorialManager.CurrentTutorial != null && _tutorialManager.IsLastTutorial))
                        {
                            Player.DamageByNoMoreCardsInDeck++;
                            Player.Defense -= Player.DamageByNoMoreCardsInDeck;
                            _vfxController.SpawnGotDamageEffect(Player, -Player.DamageByNoMoreCardsInDeck);
                        }

                        return null;
                    }

                    card = CardsInDeck[0];
                }

                if (CheckIsMoreThanMaxCards(card))
                    return null;

                if (removeCardsFromDeck)
                {
                    RemoveCardFromDeck(card);
                }

                IView cardView = AddCardToHand(card);
                return cardView;
            }

            public IView AddCardToHand(BoardUnitModel boardUnitModel, bool silent = false)
            {
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
                return cardView;
            }

            public BoardCardView CreateAndAddPlayerHandCard(BoardUnitModel boardUnitModel, bool silent = false)
            {
                BoardCardView boardCardView = CreateBoardCard(boardUnitModel);

                if (_battlegroundController.CurrentTurn == 0)
                {
                    boardCardView.SetDefaultAnimation();
                }

                _battlegroundController.PlayerHandCards.Insert(ItemPosition.End, boardCardView);

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

                return boardCardView;
            }

            public OpponentHandCard CreateAndAddOpponentHandCard(BoardUnitModel boardUnitModel)
            {
                OpponentHandCard opponentHandCard = CreateOpponentHandCard(boardUnitModel);

                _battlegroundController.OpponentHandCards.Insert(ItemPosition.End, opponentHandCard);
                _abilitiesController.CallAbilitiesInHand(null, boardUnitModel);

                return opponentHandCard;
            }

            public OpponentHandCard CreateOpponentHandCard(BoardUnitModel boardUnitModel)
            {
                Player opponent = _gameplayManager.OpponentPlayer;
                GameObject go = Object.Instantiate(_cardsController.OpponentCardPrefab);
                go.GetComponent<SortingGroup>().sortingOrder = opponent.CardsInHand.Count;
                OpponentHandCard opponentHandCard = new OpponentHandCard(go, boardUnitModel);

                return opponentHandCard;
            }

            public void AddCardToHandFromOtherPlayerDeck(BoardUnitModel card = null)
            {
                if (card == null)
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

                    card = OpponentPlayer.CardsInDeck[0];
                }

                OpponentPlayer.LocalCardsController.RemoveCardFromDeck(card);

                if (CheckIsMoreThanMaxCards(card))
                    return;

                if (Player == OpponentPlayer)
                {
                    AddCardToHand(card);
                }
                else
                {
                    AddCardToHandFromOpponentDeck(card);
                }

                if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
                {
                    //await _gameplayManager.GetController<OpponentController>().ActionDrawCard(player, otherPlayer, player, Enumerators.AffectObjectType.Types.Enum.Player, card.Prototype.Name);
                    _cardsController.MulliganCards?.Add(card);
                }
            }

            public void AddCardToHandFromOpponentDeck(BoardUnitModel boardUnitModel)
            {
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

            public void SetCardsInHand(IEnumerable<BoardUnitModel> cards)
            {
                _cardsInHand.Clear();
                _cardsInHand.InsertRange(ItemPosition.Start, cards);

                InvokeHandChanged();
            }

            public void SetFirstHandForLocalMatch(bool skip)
            {
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
                        _cardsPreparingToHand.Insert(ItemPosition.End, CardsInDeck[i]);
                    }
                    else
                    {
                        AddCardFromDeckToHand(CardsInDeck[0]);
                    }
                }

                Player.ThrowMulliganCardsEvent(_cardsController.MulliganCards);
            }

            public void SetFirstHandForPvPMatch(List<WorkingCard> workingCards, bool removeCardsFromDeck = true)
            {
                foreach (WorkingCard workingCard in workingCards)
                {
                    BoardUnitModel boardUnitModel = new BoardUnitModel(workingCard);
                    if (Player.IsLocalPlayer && !_gameplayManager.IsTutorial)
                    {
                        _cardsPreparingToHand.Insert(ItemPosition.End, boardUnitModel);
                    }
                    else
                    {
                        AddCardFromDeckToHand(boardUnitModel, removeCardsFromDeck);
                    }
                }

                Player.ThrowMulliganCardsEvent(_cardsController.MulliganCards);
            }

            public void InvokeHandChanged()
            {
                Player.HandChanged?.Invoke(CardsInHand.Count);
            }

            #endregion

            #region Board

            public void AddCardToBoard(BoardUnitModel boardUnitModel, ItemPosition position)
            {
                if (CardsOnBoard.Contains(boardUnitModel))
                {
                    Log.Warn($"Attempt to add card {boardUnitModel} to CardsOnBoard when it is already added");
                    return;
                }

                _cardsOnBoard.Insert(InternalTools.GetSafePositionToInsert(position, CardsOnBoard), boardUnitModel);
                InvokeBoardChanged();
            }

            public void RemoveCardFromBoard(BoardUnitModel boardUnitModel, bool moveToGraveyard = true)
            {
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
                _cardsOnBoard.Clear();
            }

            private void InvokeBoardChanged()
            {
                Player.BoardChanged?.Invoke(CardsOnBoard.Count);
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
                Player.GraveyardChanged?.Invoke(CardsInGraveyard.Count);
            }

            #endregion

            #endregion

            public void TakeControlOfUnit(BoardUnitModel unit)
            {
                unit.OwnerPlayer.LocalCardsController.RemoveCardFromBoard(unit);
                unit.OwnerPlayer.LocalCardsController._cardsOnBoard.Remove(unit);

                unit.Card.Owner = Player;

                Player.LocalCardsController._cardsOnBoard.Insert(ItemPosition.End, unit);
            }

            public BoardUnitModel CreateNewCardByNameAndAddToHand(string name)
            {
                float animationDuration = 1.5f;

                Card card = new Card(_dataManager.CachedCardsLibraryData.GetCardFromName(name));
                WorkingCard workingCard = new WorkingCard(card, card, Player);
                if (_tutorialManager.IsTutorial)
                {
                    workingCard.TutorialObjectId = DefaultIndexCustomCardForTutorial;
                }

                BoardUnitModel boardUnitModel = new BoardUnitModel(workingCard);

                if (CheckIsMoreThanMaxCards(boardUnitModel))
                    return boardUnitModel;

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
                            _battlegroundController.PlayerHandCards.Insert(ItemPosition.End, boardCardView);
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

                return boardUnitModel;
            }

            public void ReturnToHandBoardUnit(BoardUnitModel boardUnitModel, Vector3 cardPosition)
            {
                if (CheckIsMoreThanMaxCards(boardUnitModel))
                    return;

                IView cardView = AddCardToHand(boardUnitModel, true);
                cardView.Transform.position = cardPosition;

                if (Player.IsLocalPlayer)
                {
                    cardView.Transform.localScale =
                        new Vector3(0.25f, 0.25f, 0.25f); // size of the cards in hand
                }
            }

            public BoardUnitView SpawnUnitOnBoard(string name, ItemPosition position, bool isPVPNetwork = false, Action onComplete = null)
            {
                if (CardsOnBoard.Count >= Player.MaxCardsInPlay)
                    return null;

                Card prototype = new Card(_dataManager.CachedCardsLibraryData.GetCardFromName(name));
                WorkingCard card = new WorkingCard(prototype, prototype, Player);
                BoardUnitModel boardUnitModel = new BoardUnitModel(card);

                return SpawnUnitOnBoard(boardUnitModel, position, isPVPNetwork, onComplete);
            }

            public BoardUnitView SpawnUnitOnBoard(
                BoardUnitModel boardUnitModel,
                ItemPosition position,
                bool isPVPNetwork = false,
                Action onComplete = null)
            {
                if (CardsOnBoard.Count >= Player.MaxCardsInPlay)
                    return null;

                BoardUnitView unit = CreateBoardUnitForSpawn(boardUnitModel, Player);

                AddCardToBoard(boardUnitModel, position);

                if (isPVPNetwork)
                {
                    _battlegroundController.RegisterBoardUnitView(Player, unit);
                }
                else
                {
                    //Player.BoardCards.Insert(position, unit);
                    _battlegroundController.RegisterBoardUnitView(Player, unit);
                }

                _abilitiesController.ResolveAllAbilitiesOnUnit(unit.Model);

                _boardController.UpdateCurrentBoardOfPlayer(Player, onComplete);

                return unit;
            }

            public bool CheckIsMoreThanMaxCards(BoardUnitModel boardUnitModel)
            {
                // TODO : Temp fix to not to check max cards in hand for now
                // TODO : because the cards in hand is not matching on both the clients
                if (_matchManager.MatchType == Enumerators.MatchType.PVP)
                    return false;

                if (CardsInHand.Count >= Player.MaxCardsInHand)
                {
                    // IMPROVE ANIMATION
                    return true;
                }

                return false;
            }

            public void SummonUnitFromHand(BoardCardView card, bool activateAbility)
            {
                IReadOnlyCard prototype = card.Model.Card.Prototype;

                card.Transform.DORotate(Vector3.zero, .1f);

                if (card.HandBoardCard != null)
                {
                    card.HandBoardCard.Enabled = false;
                }

                _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND,
                    Constants.CardsMoveSoundVolume);

                GameObject board = Player.IsLocalPlayer ? _cardsController.PlayerBoard : _cardsController.OpponentBoard;

                BoardUnitView boardUnitView = new BoardUnitView(new BoardUnitModel(card.Model.Card), board.transform);
                boardUnitView.Transform.tag = Player.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
                boardUnitView.Transform.parent = board.transform;
                boardUnitView.Transform.position = new Vector2(Constants.DefaultPositonOfUnitWhenSpawn * Player.CardsOnBoard.Count, 0);
                boardUnitView.Model.Card.Owner = card.Model.Card.Owner;
                boardUnitView.Model.Card.TutorialObjectId = card.Model.Card.TutorialObjectId;

                OpponentHandCard opponentHandCard = null;

                if (activateAbility)
                {
                    _abilitiesController.ActivateAbilitiesOnCard(boardUnitView.Model, card.Model, Player);
                }

                if (Player.IsLocalPlayer)
                {
                    _battlegroundController.PlayerHandCards.Remove(card);
                    _battlegroundController.RegisterBoardUnitView(_gameplayManager.CurrentPlayer, boardUnitView);
                }
                else
                {
                    opponentHandCard = _battlegroundController.OpponentHandCards.FirstOrDefault(cardOpponent =>
                        cardOpponent.Model.InstanceId == card.Model.Card.InstanceId);
                    _battlegroundController.OpponentHandCards.Remove(opponentHandCard);
                    _battlegroundController.RegisterBoardUnitView(_gameplayManager.OpponentPlayer, boardUnitView);
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

            private BoardCardView CreateBoardCard(BoardUnitModel boardUnitModel)
            {
                GameObject go;
                BoardCardView boardCardView;
                switch (boardUnitModel.Card.Prototype.CardKind)
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

            public void SetCardsPreparingToHand(IReadOnlyList<BoardUnitModel> cards)
            {
                _cardsPreparingToHand.Clear();
                _cardsPreparingToHand.InsertRange(ItemPosition.End, cards);
            }
        }
    }
}
