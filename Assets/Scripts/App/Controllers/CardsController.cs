using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
        public event Action<AbilityData.ChoosableAbility> CardForAbilityChoosed;

        public GameObject CreatureCardViewPrefab, OpponentCardPrefab, ItemCardViewPrefab;

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

        private GameObject _playerBoard;

        private GameObject _opponentBoard;

        private BoardUnitView _fakeBoardCard;

        private int _cardInstanceId;

        private int _indexOfCard;

        public event Action<Player> UpdateCardsStatusEvent;

        public bool CardDistribution { get; set; }

        private Vector3 _newCardPositionOfBoard;

        private bool _isHoveringCardOfBoard;

        public List<WorkingCard> MulliganCards;

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

            if (Constants.MulliganEnabled || GameClient.Get<IMatchManager>().MatchType != Enumerators.MatchType.PVP)
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
            if (!CardDistribution)
                return;

            _gameplayManager.IsPreparingEnded = true;

            GameClient.Get<ICameraManager>().FadeOut(immediately: true);

            _timerManager.StopTimer(DirectlyEndCardDistribution);

            // for local player
            foreach (WorkingCard card in _gameplayManager.CurrentPlayer.CardsPreparingToHand)
            {
                AddCardToHand(_gameplayManager.CurrentPlayer, card);
            }
            _gameplayManager.CurrentPlayer.CardsPreparingToHand.Clear();

            CardDistribution = false;

            _gameplayManager.CurrentPlayer.ThrowOnHandChanged();

            if (GameClient.Get<IMatchManager>().MatchType != Enumerators.MatchType.PVP && (!_gameplayManager.IsTutorial ||
                (_gameplayManager.IsTutorial &&
                _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.DisabledInitialization)))
            {
                _gameplayManager.CurrentPlayer.CardsInDeck.UnsafeGetUnderlyingList().ShuffleList();
            }

            _battlegroundController.StartGameplayTurns();

            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
            {
                int highestInstanceId =
                    _pvpManager.InitialGameState.PlayerStates
                    .SelectMany(state =>
                        state.MulliganCards
                            .Concat(state.CardsInDeck)
                            .Concat(state.CardsInHand)
                            .Concat(state.CardsInPlay)
                            .Concat(state.CardsInGraveyard))
                    .Max(card => card.InstanceId.Id);
                SetNewCardInstanceId(highestInstanceId);
            }
        }

        public void CardsDistribution(IReadOnlyList<WorkingCard> mulliganCards)
        {
            Player player = _gameplayManager.CurrentPlayer;
            List<WorkingCard> randomCards = new List<WorkingCard>();

            int count = 0;
            while (randomCards.Count < mulliganCards.Count) {
                if (!player.CardsPreparingToHand.Contains(player.CardsInDeck[count]) && !mulliganCards.Contains(player.CardsInDeck[count]))
                {
                    randomCards.Add(player.CardsInDeck[count]);
                }
                count++;
            }

            UniquePositionedList<WorkingCard> finalCards = player.CardsPreparingToHand.Except(mulliganCards).ToUniquePositionedList();
            player.CardsPreparingToHand.Clear();
            player.CardsPreparingToHand.InsertRange(ItemPosition.End, finalCards);
            player.CardsPreparingToHand.InsertRange(ItemPosition.End, randomCards);

            EndCardDistribution();
        }

        public void AddCardToDistributionState(Player player, WorkingCard card)
        {
            player.CardsPreparingToHand.Insert(ItemPosition.End, card);
        }

        public IView AddCardToHand(Player player, WorkingCard card = null, bool removeCardsFromDeck = true)
        {
            if (card == null)
            {
                if (player.CardsInDeck.Count == 0)
                {
                    if (!_tutorialManager.IsTutorial || (_tutorialManager.CurrentTutorial != null && _tutorialManager.IsLastTutorial))
                    {
                        player.DamageByNoMoreCardsInDeck++;
                        player.Defense -= player.DamageByNoMoreCardsInDeck;
                        _vfxController.SpawnGotDamageEffect(player, -player.DamageByNoMoreCardsInDeck);
                    }
                    return null;
                }

                card = player.CardsInDeck[0];
            }

            if (CheckIsMoreThanMaxCards(card, player))
                return null;

            /*
            if (_matchManager.MatchType == Enumerators.MatchType.PVP)
            {
                player.ThrowDrawCardEvent(card);
            }
*/
            if (removeCardsFromDeck)
            {
                player.RemoveCardFromDeck(card);
            }

            IView cardView = player.AddCardToHand(card);
            return cardView;
        }

        public void AddCardToHandFromOtherPlayerDeck(Player player, Player otherPlayer, WorkingCard card = null)
        {
            if (card == null)
            {
                if (otherPlayer.CardsInDeck.Count == 0)
                {
                    if (!_tutorialManager.IsTutorial)
                    {
                        otherPlayer.DamageByNoMoreCardsInDeck++;
                        otherPlayer.Defense -= otherPlayer.DamageByNoMoreCardsInDeck;
                        _vfxController.SpawnGotDamageEffect(otherPlayer, -otherPlayer.DamageByNoMoreCardsInDeck);
                    }
                    return;
                }

                card = otherPlayer.CardsInDeck[0];
            }

            otherPlayer.RemoveCardFromDeck(card);

            if (CheckIsMoreThanMaxCards(card, player))
                return;

            if (player.Equals(otherPlayer))
            {
                player.AddCardToHand(card);
            }
            else
            {
                player.AddCardToHandFromOpponentDeck(otherPlayer, card);
            }

            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
            {
                //await _gameplayManager.GetController<OpponentController>().ActionDrawCard(player, otherPlayer, player, Enumerators.AffectObjectType.Types.Enum.Player, card.LibraryCard.Name);
                MulliganCards?.Add(card);
            }
        }

        public BoardCard AddCardToHand(WorkingCard card, bool silent = false)
        {
            BoardCard boardCard = CreateBoardCard(card);

            if (_battlegroundController.CurrentTurn == 0)
            {
                boardCard.SetDefaultAnimation();
            }

            _battlegroundController.PlayerHandCards.Insert(ItemPosition.End, boardCard);

            if (silent)
            {
                boardCard.HandBoardCard.Enabled = false;

                _timerManager.AddTimer(
                    x =>
                    {
                        boardCard.HandBoardCard.Enabled = true;
                        boardCard.HandBoardCard.CheckStatusOfHighlight();
                    },
                    null,
                    2f);
            }
            else
            {
                boardCard.HandBoardCard.CheckStatusOfHighlight();
            }

            return boardCard;
        }

        public OpponentHandCard AddCardToOpponentHand(WorkingCard card, bool silent = false)
        {
            OpponentHandCard opponentHandCard = CreateOpponentHandCard(card);

            _battlegroundController.OpponentHandCards.Insert(ItemPosition.End, opponentHandCard);
            _abilitiesController.CallAbilitiesInHand(null, card);

            return opponentHandCard;
        }

        public OpponentHandCard CreateOpponentHandCard(WorkingCard card)
        {
            Player opponent = _gameplayManager.OpponentPlayer;
            GameObject go = Object.Instantiate(OpponentCardPrefab);
            go.GetComponent<SortingGroup>().sortingOrder = opponent.CardsInHand.Count;
            OpponentHandCard opponentHandCard = new OpponentHandCard(go, card);

            return opponentHandCard;
        }

        public void RemoveCard(object[] param)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CARD_BATTLEGROUND_TO_TRASH, Constants.CardsMoveSoundVolume);

            BoardCard card = (BoardCard) param[0];
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

        public void RemoveOpponentCard(object[] param)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CARD_BATTLEGROUND_TO_TRASH, Constants.CardsMoveSoundVolume);

            OpponentHandCard opponentHandCard = (OpponentHandCard) param[0];
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

        public void HoverPlayerCardOnBattleground(Player player, BoardCard card, HandBoardCard handCard)
        {
            IReadOnlyCard libraryCard = card.WorkingCard.LibraryCard;
            if (libraryCard.CardKind == Enumerators.CardKind.CREATURE &&
                _gameplayManager.CurrentPlayer.BoardCards.Count < _gameplayManager.CurrentPlayer.MaxCardsInPlay)
            {
                int newIndexOfCard = 0;
                float newCreatureCardPosition = card.Transform.position.x;

                // set correct position on board depends from card view position
                for (int i = 0; i < player.BoardCards.Count; i++)
                {
                    if (newCreatureCardPosition > player.BoardCards[i].Transform.position.x)
                    {
                        newIndexOfCard = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (player.BoardCards.Count > 0 && _indexOfCard != newIndexOfCard)
                {
                    _indexOfCard = newIndexOfCard;

                    IReadOnlyList<BoardUnitView> playerCards = _gameplayManager.CurrentPlayer.BoardCards;
                    List<BoardUnitView> toArrangeList = new List<BoardUnitView>();

                    for (int i = 0; i < playerCards.Count; i++)
                    {
                        toArrangeList.Add(playerCards[i]);
                    }

                    if (_fakeBoardCard != null)
                    {
                        _fakeBoardCard.DisposeGameObject();
                        _fakeBoardCard = null;
                    }

                    _fakeBoardCard = new BoardUnitView(new BoardUnitModel(), _playerBoard.transform);
                    toArrangeList.Insert(_indexOfCard, _fakeBoardCard);

                    _boardController.UpdateBoard(toArrangeList, true, null);

                    _newCardPositionOfBoard = _fakeBoardCard.PositionOfBoard;
                    _isHoveringCardOfBoard = true;
                }
            }
        }        

        public void ResetPlayerCardsOnBattlegroundPosition()
        {
            if (_indexOfCard != -1)
            {
                _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.CurrentPlayer, null);

                _indexOfCard = -1;
                if (_fakeBoardCard != null)
                {
                    _fakeBoardCard.DisposeGameObject();
                    _fakeBoardCard = null;
                }
            }
        }

        public void PlayPlayerCard(Player player,
                                   BoardCard card,
                                   HandBoardCard handCard,
                                   Action<PlayCardOnBoard> OnPlayPlayerCard,
                                   BoardObject target = null,
                                   bool skipEntryAbilities = false)
        {
            if (card.CanBePlayed(card.WorkingCard.Owner))
            {
                IReadOnlyCard libraryCard = card.WorkingCard.LibraryCard;

                card.Transform.DORotate(Vector3.zero, .1f);
                card.HandBoardCard.Enabled = false;
                if(!_gameplayManager.AvoidGooCost)
                    card.WorkingCard.Owner.CurrentGoo -= card.ManaCost;


                _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND,
                    Constants.CardsMoveSoundVolume);

                GameplayQueueAction<object> CallAbilityAction = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsage, blockQueue: true);
                GameplayQueueAction<object> RankBuffAction = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.RankBuff);

                switch (libraryCard.CardKind)
                {
                    case Enumerators.CardKind.CREATURE:
                        {
                            card.FuturePositionOnBoard = 0;
                            float newCreatureCardPosition = card.Transform.position.x;

                            // set correct position on board depends from card view position
                            for (int i = 0; i < player.BoardCards.Count; i++)
                            {
                                if (newCreatureCardPosition > player.BoardCards[i].Transform.position.x)
                                {
                                    card.FuturePositionOnBoard = i + 1;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            BoardUnitView boardUnitView = new BoardUnitView(new BoardUnitModel(), _playerBoard.transform);
                            boardUnitView.Transform.tag = SRTags.PlayerOwned;
                            boardUnitView.Transform.parent = _playerBoard.transform;
                            boardUnitView.Transform.position = new Vector2(1.9f * player.BoardCards.Count, 0);
                            boardUnitView.Model.OwnerPlayer = card.WorkingCard.Owner;
                            boardUnitView.SetObjectInfo(card.WorkingCard);
                            boardUnitView.Model.TutorialObjectId = card.WorkingCard.TutorialObjectId;

                            player.CardsInHand.Remove(card.WorkingCard);
                            player.BoardCards.Insert(Mathf.Clamp(card.FuturePositionOnBoard,0, player.BoardCards.Count), boardUnitView);
                            player.AddCardToBoard(card.WorkingCard, (ItemPosition) card.FuturePositionOnBoard);
                            _battlegroundController.PlayerHandCards.Remove(card);
                            _battlegroundController.PlayerBoardCards.Insert(Mathf.Clamp(card.FuturePositionOnBoard, 0,
                            _battlegroundController.PlayerBoardCards.Count), boardUnitView);
                            _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                            InternalTools.DoActionDelayed(
                                     () =>
                                     {
                                         card.WorkingCard.Owner.GraveyardCardsCount++;
                                     }, 1f);

                            card.RemoveCardParticle.Play();

                            _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitView.Model, false, _gameplayManager.CanDoDragActions);

                            if(_isHoveringCardOfBoard)
                            {
                                boardUnitView.PositionOfBoard = _newCardPositionOfBoard;
                                _isHoveringCardOfBoard = false;
                            }

                            _ranksController.UpdateRanksByElements(boardUnitView.Model.OwnerPlayer.BoardCards, boardUnitView.Model.Card, RankBuffAction);

                            boardUnitView.PlayArrivalAnimation(playUniqueAnimation: true);
                            _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.CurrentPlayer,
                                () =>
                                {
                                    card.HandBoardCard.GameObject.SetActive(false);

                                    _abilitiesController.CallAbility(libraryCard, card, card.WorkingCard,
                                        Enumerators.CardKind.CREATURE, boardUnitView.Model, CallCardPlay, true, (status) =>
                                        {
                                            UpdateCardsStatusEvent?.Invoke(player);

                                            if (status)
                                            {
                                                player.ThrowPlayCardEvent(card.WorkingCard, card.FuturePositionOnBoard);
                                                OnPlayPlayerCard?.Invoke(new PlayCardOnBoard(boardUnitView, card.ManaCost));
                                                if (card is UnitBoardCard)
                                                {
                                                    UnitBoardCard unitBoardCard = card as UnitBoardCard;
                                                    unitBoardCard.Damage = boardUnitView.Model.MaxCurrentDamage;
                                                    unitBoardCard.Health = boardUnitView.Model.MaxCurrentHp;
                                                }
                                            }
                                            else
                                            {
                                                RankBuffAction.Action = null;
                                                RankBuffAction.ForceActionDone();

                                                _battlegroundController.PlayerBoardCards.Remove(boardUnitView);
                                                player.BoardCards.Remove(boardUnitView);


                                                boardUnitView.DisposeGameObject();
                                                boardUnitView.Model.Die(true);

                                                _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.CurrentPlayer, null);
                                            }

                                        }, CallAbilityAction, target, handCard, skipEntryAbilities);

                                    _actionsQueueController.ForceContinueAction(CallAbilityAction);
                                });
                            break;
                        }
                    case Enumerators.CardKind.ITEM:
                        {
                            player.CardsInHand.Remove(card.WorkingCard);
                            _battlegroundController.PlayerHandCards.Remove(card);
                            _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                            card.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.BoardCards;
                            card.GameObject.GetComponent<SortingGroup>().sortingOrder = 1000;

                            BoardSpell boardSpell = new BoardSpell(card.GameObject, card.WorkingCard);

                            card.RemoveCardParticle.Play();

                            InternalTools.DoActionDelayed(() =>
                            {
                                _abilitiesController.CallAbility(libraryCard, card, card.WorkingCard,
                                    Enumerators.CardKind.ITEM, boardSpell, CallSpellCardPlay, true, (status) =>
                                    {
                                        if(status)
                                        {
                                            player.ThrowPlayCardEvent(card.WorkingCard, 0);
                                        }

                                        RankBuffAction.ForceActionDone();
                                    }, CallAbilityAction, target, handCard, skipEntryAbilities);

                                _actionsQueueController.ForceContinueAction(CallAbilityAction);
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

        public void SummonUnitFromHand(Player player, BoardCard card, bool activateAbility)
        {
            IReadOnlyCard libraryCard = card.WorkingCard.LibraryCard;

            card.Transform.DORotate(Vector3.zero, .1f);

            if (card.HandBoardCard != null)
            {
                card.HandBoardCard.Enabled = false;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND,
                Constants.CardsMoveSoundVolume);

            GameObject board = player.IsLocalPlayer ? _playerBoard : _opponentBoard;

            BoardUnitView boardUnitView = new BoardUnitView(new BoardUnitModel(), board.transform);
            boardUnitView.Transform.tag = player.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.parent = board.transform;
            boardUnitView.Transform.position = new Vector2(Constants.DefaultPositonOfUnitWhenSpawn * player.BoardCards.Count, 0);
            boardUnitView.Model.OwnerPlayer = card.WorkingCard.Owner;
            boardUnitView.SetObjectInfo(card.WorkingCard);
            boardUnitView.Model.TutorialObjectId = card.WorkingCard.TutorialObjectId;

            OpponentHandCard opponentHandCard = null;

            if(activateAbility)
            {
                _abilitiesController.ActivateAbilitiesOnCard(boardUnitView.Model, card.WorkingCard, player);
            }


            if (player.IsLocalPlayer)
            {
                _battlegroundController.PlayerHandCards.Remove(card);
                _battlegroundController.PlayerBoardCards.Insert(ItemPosition.End, boardUnitView);
            }
            else
            {
                opponentHandCard = _battlegroundController.OpponentHandCards.FirstOrDefault(cardOpponent => cardOpponent.WorkingCard.InstanceId == card.WorkingCard.InstanceId);
                _battlegroundController.OpponentHandCards.Remove(opponentHandCard);
                _battlegroundController.OpponentBoardCards.Insert(ItemPosition.End, boardUnitView);
            }


            player.AddCardToBoard(card.WorkingCard, ItemPosition.End);
            player.RemoveCardFromHand(card.WorkingCard);
            player.BoardCards.Insert(ItemPosition.End, boardUnitView);

            InternalTools.DoActionDelayed(() =>
            {
                card.WorkingCard.Owner.GraveyardCardsCount++;
            }, 1f);

            card.RemoveCardParticle.Play();

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.PlayCardFromHand,
                Caller = boardUnitView.Model,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
            });

            _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitView.Model, true, true);

            if (!player.IsLocalPlayer)
            {
                card.GameObject.SetActive(false);
            }

            if (player.IsLocalPlayer)
            {
                RemoveCard(new object[]
                {
                    card
                });
            }
            else
            {
                if (opponentHandCard != null)
                {
                    RemoveOpponentCard(new object[]
                    {
                    opponentHandCard
                    });
                }
                else
                {
                    Object.Destroy(card.GameObject);
                }
            }

            InternalTools.DoActionDelayed(() =>
            {
                boardUnitView.PlayArrivalAnimation();

                _boardController.UpdateCurrentBoardOfPlayer(player, null);
            }, 0.1f);
        }

        public void PlayOpponentCard(
            Player player,
            InstanceId cardId,
            BoardObject target,
            Action<WorkingCard> cardFoundCallback,
            Action<WorkingCard, BoardObject> completePlayCardCallback
            )
        {
            OpponentHandCard opponentHandCard;
            if(GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP || _gameplayManager.IsTutorial)
            {
                opponentHandCard =
                    _battlegroundController.OpponentHandCards.FirstOrDefault(x => x.WorkingCard.InstanceId == cardId);
            }
            else
            {
                if (_battlegroundController.OpponentHandCards.Count <= 0)
                    return;

                opponentHandCard = _battlegroundController.OpponentHandCards.FirstOrDefault(x => x.WorkingCard.InstanceId == cardId);
            }

            if(opponentHandCard == null || opponentHandCard is default(OpponentHandCard))
            {
                Exception exception = new Exception($"[Out of sync] not found card in opponent hand! card Id: {cardId.Id}");
                Helpers.ExceptionReporter.LogException(exception);
                Debug.LogException(exception);
                return;
            }

            WorkingCard card = opponentHandCard.WorkingCard;

            _battlegroundController.OpponentHandCards.Remove(opponentHandCard);
            player.CardsInHand.Remove(card);
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
                            RemoveOpponentCard(new object[]
                            {
                                opponentHandCard
                            });

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

        public void DrawCardInfo(WorkingCard card)
        {
            GameObject go;
            BoardCard boardCard;
            switch (card.LibraryCard.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/CreatureCard"));
                    boardCard = new UnitBoardCard(go);
                    break;
                case Enumerators.CardKind.ITEM:
                    go = Object.Instantiate(
                        _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Cards/ItemCard"));
                    boardCard = new SpellBoardCard(go);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            boardCard.Init(card);
            go.transform.position = new Vector3(-6, 0, 0);
            go.transform.localScale = Vector3.one * .3f;
            boardCard.SetHighlightingEnabled(false);

            Object.Destroy(go, 2f);
        }

        public void ReturnToHandBoardUnit(WorkingCard workingCard, Player player, Vector3 cardPosition)
        {
            if (CheckIsMoreThanMaxCards(workingCard, player))
                return;

            IView cardView = player.AddCardToHand(workingCard, true);
            cardView.Transform.position = cardPosition;

            if (player.IsLocalPlayer)
            {
                cardView.Transform.localScale =
                    new Vector3(0.25f, 0.25f, 0.25f); // size of the cards in hand         
            }
        }

        public WorkingCard LowGooCostOfCardInHand(Player player, WorkingCard card = null, int value = 1)
        {
            if (card == null && player.CardsInHand.Count > 0)
            {
                card = player.CardsInHand[Random.Range(0, player.CardsInHand.Count)];
            }

            if (card == null)
                return card;

            if (player.IsLocalPlayer)
            {
                BoardCard boardCard = _battlegroundController.PlayerHandCards.First(x => x.WorkingCard.Equals(card));

                boardCard.ChangeCardCostOn(value, true);
            }
            else
            {
                card.InstanceCard.Cost += value;
            }

            player.ThrowOnHandChanged();

            return card;
        }

        public void SetGooCostOfCardInHand(Player player, WorkingCard card, int value, BoardCard boardCard = null)
        {
            if (player.IsLocalPlayer)
            {
                if (boardCard == null)
                {
                    boardCard = _battlegroundController.PlayerHandCards.First(x => x.WorkingCard.Equals(card));
                }

                card.InstanceCard.Cost = Mathf.Clamp(value, 0, 99);
                boardCard.SetCardCost(value);

                bool isActive = boardCard.WorkingCard.InstanceCard.Cost < boardCard.WorkingCard.LibraryCard.Cost;
                boardCard.costHighlightObject.SetActive(isActive);
            }
            else
            {
                card.InstanceCard.Cost = Mathf.Clamp(value, 0, 99);
                card.LibraryCard = new Card(
                    card.LibraryCard.MouldId,
                    card.LibraryCard.Name,
                    card.LibraryCard.Cost,
                    card.LibraryCard.Description,
                    card.LibraryCard.FlavorText,
                    card.LibraryCard.Picture,
                    card.LibraryCard.Damage,
                    card.LibraryCard.Defense,
                    card.LibraryCard.Faction,
                    card.LibraryCard.Frame,
                    card.LibraryCard.CardKind,
                    card.LibraryCard.CardRank,
                    card.LibraryCard.CardType,
                    card.LibraryCard.Abilities
                        .Select(a => new AbilityData(a))
                        .ToList(),
                    new PictureTransform(card.LibraryCard.PictureTransform),
                    card.LibraryCard.UniqueAnimation,
                    card.LibraryCard.Hidden
                );
            }
        }

        public Enumerators.Faction GetSetOfCard(IReadOnlyCard card)
        {
            CardSet set =
                _dataManager.CachedCardsLibraryData.Sets.Find(x => x.Cards.Find(y => y.Name.Equals(card.Name)) != null);

            return set.Name;
        }

        public WorkingCard CreateNewCardByNameAndAddToHand(Player player, string name)
        {
            float animationDuration = 1.5f;

            Card card = new Card(_dataManager.CachedCardsLibraryData.GetCardFromName(name));
            WorkingCard workingCard = new WorkingCard(card, card, player);
            if(_tutorialManager.IsTutorial)
            {
                workingCard.TutorialObjectId = DefaultIndexCustomCardForTutorial;
            }

            if (CheckIsMoreThanMaxCards(workingCard, player))
                return workingCard;

            if (player.IsLocalPlayer)
            {
                BoardCard boardCard = CreateBoardCard(workingCard);

                boardCard.Transform.position = Vector3.zero;
                boardCard.Transform.localScale = Vector3.zero;

                boardCard.Transform.DOScale(Vector3.one * .3f, animationDuration);

                InternalTools.DoActionDelayed(() =>
                {
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerCreatedNewCardAndMovedToHand);
                }, animationDuration - Time.deltaTime);

                InternalTools.DoActionDelayed(() =>
                {
                    _battlegroundController.PlayerHandCards.Insert(ItemPosition.End, boardCard);
                    player.CardsInHand.Insert(ItemPosition.End, workingCard);

                    _battlegroundController.UpdatePositionOfCardsInPlayerHand(true);
                }, animationDuration);
            }
            else
            {
                OpponentHandCard handCard = AddCardToOpponentHand(workingCard);
                handCard.Transform.position = Vector3.zero;
                handCard.Transform.localScale = Vector3.zero;

                handCard.Transform.DOScale(Vector3.one, animationDuration);

                _timerManager.AddTimer(
                    x =>
                    {
                        player.CardsInHand.Insert(ItemPosition.End, workingCard);
                        _battlegroundController.UpdatePositionOfCardsInOpponentHand(true);
                    },
                    null,
                    animationDuration);
            }

            return workingCard;
        }

        public BoardCard GetBoardCard(WorkingCard card)
        {
            return CreateBoardCard(card);
        }

        public OpponentHandCard GetOpponentBoardCard(WorkingCard card)
        {
            return CreateOpponentHandCard(card);
        }

        public void ReturnCardToHand(BoardUnitView unit)
        {
            Player unitOwner = unit.Model.OwnerPlayer;
            WorkingCard returningCard = unit.Model.Card;

            returningCard.InstanceCard.Cost = returningCard.LibraryCard.Cost;

            Vector3 unitPosition = unit.Transform.position;

            _battlegroundController.DeactivateAllAbilitiesOnUnit(unit);

            unit.Model.InvokeUnitPrepairingToDie();

            InternalTools.DoActionDelayed(() =>
            {
                unitOwner.BoardCards.Remove(unit);

                unit.Model.Die(true);
                unit.DisposeGameObject();

                unitOwner.RemoveCardFromBoard(returningCard);

                ReturnToHandBoardUnit(returningCard, unitOwner, unitPosition);

                _gameplayManager.RearrangeHands();
            }, 2f);
        }

        public WorkingCard GetWorkingCardFromCardName(string cardName, Player owner)
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

            _playerBoard = GameObject.Find("PlayerBoard");
            _opponentBoard = GameObject.Find("OpponentBoard");
        }

        private void DirectlyEndCardDistribution(object[] param)
        {
            EndCardDistribution();
        }

        private BoardCard CreateBoardCard(WorkingCard card)
        {
            GameObject go;
            BoardCard boardCard;
            switch (card.LibraryCard.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(CreatureCardViewPrefab);
                    boardCard = new UnitBoardCard(go);
                    break;
                case Enumerators.CardKind.ITEM:
                    go = Object.Instantiate(ItemCardViewPrefab);
                    boardCard = new SpellBoardCard(go);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            boardCard.Init(card);
            boardCard.CurrentTurn = _battlegroundController.CurrentTurn;

            HandBoardCard handCard = new HandBoardCard(go, boardCard);
            handCard.OwnerPlayer = card.Owner;
            handCard.BoardZone = _playerBoard;
            boardCard.HandBoardCard = handCard;
            handCard.CheckStatusOfHighlight();
            boardCard.Transform.localScale = Vector3.one * .3f;

            _abilitiesController.CallAbilitiesInHand(boardCard, card);

            return boardCard;
        }

        private void CallCardPlay(BoardCard card)
        {
        }

        private void CallSpellCardPlay(BoardCard card)
        {
        }

        private bool CheckIsMoreThanMaxCards(WorkingCard workingCard, Player player)
        {
            // TODO : Temp fix to not to check max cards in hand for now
            // TODO : because the cards in hand is not matching on both the clients
            if (_matchManager.MatchType == Enumerators.MatchType.PVP)
                return false;

            if (player.CardsInHand.Count >= player.MaxCardsInHand)
            {
                // IMPROVE ANIMATION
                return true;
            }

            return false;
        }

        public BoardUnitView SpawnUnitOnBoard(Player owner, string name, ItemPosition position, bool isPVPNetwork = false, Action onComplete = null)
        {
            if (owner.BoardCards.Count >= owner.MaxCardsInPlay)
                return null;

            Card libraryCard = new Card(_dataManager.CachedCardsLibraryData.GetCardFromName(name));

            WorkingCard card = new WorkingCard(libraryCard, libraryCard, owner);
            BoardUnitView unit = CreateBoardUnitForSpawn(card, owner);

            owner.AddCardToBoard(card, ItemPosition.End);

            if (isPVPNetwork)
            {
                owner.BoardCards.Insert(ItemPosition.End, unit);
            }
            else
            {
                if (position.GetIndex(owner.BoardCards) <= Constants.MaxBoardUnits)
                {
                    owner.BoardCards.Insert(position, unit);
                }
                else
                {
                    owner.BoardCards.Insert(ItemPosition.End, unit);
                }
            }

            _abilitiesController.ResolveAllAbilitiesOnUnit(unit.Model);

            _boardController.UpdateCurrentBoardOfPlayer(owner, onComplete);

            return unit;
        }

        public BoardUnitView SpawnUnitOnBoard(Player owner, WorkingCard card, ItemPosition position, bool isPVPNetwork = false, Action onComplete = null)
        {
            if (owner.BoardCards.Count >= owner.MaxCardsInPlay)
                return null;

            BoardUnitView unit = CreateBoardUnitForSpawn(card, owner);

            owner.AddCardToBoard(card, ItemPosition.End);

            if (isPVPNetwork)
            {
                owner.BoardCards.Insert(ItemPosition.End, unit);
            }
            else
            {
                if (position.GetIndex(owner.BoardCards) <= Constants.MaxBoardUnits)
                {
                    owner.BoardCards.Insert(position, unit);
                }
                else
                {
                    owner.BoardCards.Insert(ItemPosition.End, unit);
                }
            }

            _abilitiesController.ResolveAllAbilitiesOnUnit(unit.Model);

            _boardController.UpdateCurrentBoardOfPlayer(owner, onComplete);

            return unit;
        }

        private BoardUnitView CreateBoardUnitForSpawn(WorkingCard card, Player owner)
        {
            GameObject playerBoard = owner.IsLocalPlayer ? _battlegroundController.PlayerBoardObject : _battlegroundController.OpponentBoardObject;

            float unitYPositionOnBoard = owner.IsLocalPlayer ? -1.66f : 1.66f;

            BoardUnitView boardUnitView = new BoardUnitView(new BoardUnitModel(), playerBoard.transform);
            boardUnitView.Transform.tag = owner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.parent = playerBoard.transform;
            boardUnitView.Transform.position = new Vector2(2f * owner.BoardCards.Count, unitYPositionOnBoard);
            boardUnitView.Model.OwnerPlayer = owner;
            boardUnitView.SetObjectInfo(card);
            boardUnitView.Model.TutorialObjectId = card.TutorialObjectId;

            boardUnitView.PlayArrivalAnimation();

            return boardUnitView;
        }

        public void CreateChoosableCardsForAbilities(List<AbilityData.ChoosableAbility> choosableAbilities, WorkingCard card)
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

        public ChoosableCardForAbility(Transform parent, AbilityData.ChoosableAbility choosableAbility, WorkingCard card)
        {
            _mainChoosableAbility = choosableAbility;

            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _abilitiesController = GameClient.Get<IGameplayManager>().GetController<AbilitiesController>();
            _cardsController = GameClient.Get<IGameplayManager>().GetController<CardsController>();

            string prefabName = card.LibraryCard.CardKind == Enumerators.CardKind.CREATURE ? "Card_BoardUnit" : "Card_Item";

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

            string setName = card.LibraryCard.Faction.ToString();
            string rarity = Enum.GetName(typeof(Enumerators.CardRank), card.LibraryCard.CardRank);
            string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

            if (!string.IsNullOrEmpty(card.LibraryCard.Frame))
            {
                frameName = "Images/Cards/Frames/" + card.LibraryCard.Frame;
            }

            _frame.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);
            _picture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format(
                "Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLowerInvariant(), rarity.ToLowerInvariant(),
                card.LibraryCard.Picture.ToLowerInvariant()));

            _titleText.text = card.LibraryCard.Name;
            _descriptionText.text = choosableAbility.Description;
            _gooCostText.text = card.InstanceCard.Cost.ToString();

            if (card.LibraryCard.CardKind == Enumerators.CardKind.CREATURE)
            {
                _unitType = SelfObject.transform.Find("Image_Type").GetComponent<SpriteRenderer>();

                _attackText = SelfObject.transform.Find("Text_Attack").GetComponent<TextMeshPro>();
                _defenseText = SelfObject.transform.Find("Text_Defense").GetComponent<TextMeshPro>();

                _attackText.text = card.LibraryCard.Damage.ToString();
                _defenseText.text = card.LibraryCard.Defense.ToString();

                _unitType.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/{0}", card.InstanceCard.CardType + "_icon"));
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
