using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using log4net;
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
            foreach (BoardUnitModel card in _gameplayManager.CurrentPlayer.CardsPreparingToHand)
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

        public void CardsDistribution(IReadOnlyList<BoardUnitModel> mulliganCards)
        {
            Player player = _gameplayManager.CurrentPlayer;
            List<BoardUnitModel> randomCards = new List<BoardUnitModel>();

            int count = 0;
            while (randomCards.Count < mulliganCards.Count) {
                if (!player.CardsPreparingToHand.Contains(player.CardsInDeck[count]) && !mulliganCards.Contains(player.CardsInDeck[count]))
                {
                    randomCards.Add(player.CardsInDeck[count]);
                }
                count++;
            }

            UniquePositionedList<BoardUnitModel> finalCards = player.CardsPreparingToHand.Except(mulliganCards).ToUniquePositionedList();
            player.CardsPreparingToHand.Clear();
            player.CardsPreparingToHand.InsertRange(ItemPosition.End, finalCards);
            player.CardsPreparingToHand.InsertRange(ItemPosition.End, randomCards);

            EndCardDistribution();
        }

        public void AddCardToDistributionState(Player player, BoardUnitModel boardUnitModel)
        {
            player.CardsPreparingToHand.Insert(ItemPosition.End, boardUnitModel);
        }

        public IView AddCardToHand(Player player, BoardUnitModel card = null, bool removeCardsFromDeck = true)
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

        public void AddCardToHandFromOtherPlayerDeck(Player player, Player otherPlayer, BoardUnitModel card = null)
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
                //await _gameplayManager.GetController<OpponentController>().ActionDrawCard(player, otherPlayer, player, Enumerators.AffectObjectType.Types.Enum.Player, card.Prototype.Name);
                MulliganCards?.Add(card);
            }
        }

        public BoardCardView AddCardToHand(BoardUnitModel boardUnitModel, bool silent = false)
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

        public OpponentHandCard AddCardToOpponentHand(BoardUnitModel boardUnitModel)
        {
            OpponentHandCard opponentHandCard = CreateOpponentHandCard(boardUnitModel);

            _battlegroundController.OpponentHandCards.Insert(ItemPosition.End, opponentHandCard);
            _abilitiesController.CallAbilitiesInHand(null, boardUnitModel);

            return opponentHandCard;
        }

        public OpponentHandCard CreateOpponentHandCard(BoardUnitModel boardUnitModel)
        {
            Player opponent = _gameplayManager.OpponentPlayer;
            GameObject go = Object.Instantiate(OpponentCardPrefab);
            go.GetComponent<SortingGroup>().sortingOrder = opponent.CardsInHand.Count;
            OpponentHandCard opponentHandCard = new OpponentHandCard(go, boardUnitModel);

            return opponentHandCard;
        }

        public void RemoveCard(object[] param)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CARD_BATTLEGROUND_TO_TRASH, Constants.CardsMoveSoundVolume);

            BoardCardView card = (BoardCardView) param[0];
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

        public void HoverPlayerCardOnBattleground(Player player, BoardCardView card)
        {
            IReadOnlyCard prototype = card.BoardUnitModel.Card.Prototype;
            if (prototype.CardKind == Enumerators.CardKind.CREATURE &&
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

                    toArrangeList.Insert(_indexOfCard, null);
                    _boardController.UpdateBoard(toArrangeList, true, null, skipIndex: _indexOfCard);
                }
            }
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
            if (card.CanBePlayed(card.BoardUnitModel.Card.Owner))
            {
                card.Transform.DORotate(Vector3.zero, .1f);
                card.HandBoardCard.Enabled = false;
                if (!_gameplayManager.AvoidGooCost)
                {
                    card.BoardUnitModel.Card.Owner.CurrentGoo -= card.BoardUnitModel.Card.InstanceCard.Cost;
                }

                _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND,
                    Constants.CardsMoveSoundVolume);

                GameplayQueueAction<object> CallAbilityAction = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsage, blockQueue: true);
                GameplayQueueAction<object> RankBuffAction = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.RankBuff);

                switch (card.BoardUnitModel.Card.Prototype.CardKind)
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

                            BoardUnitView boardUnitView = new BoardUnitView(card.BoardUnitModel, _playerBoard.transform);
                            boardUnitView.Transform.tag = SRTags.PlayerOwned;
                            boardUnitView.Transform.parent = _playerBoard.transform;
                            boardUnitView.Transform.position = new Vector2(1.9f * player.BoardCards.Count, 0);
                            boardUnitView.Model.Card.Owner = card.BoardUnitModel.Card.Owner;
                            boardUnitView.Model.Card.TutorialObjectId = card.BoardUnitModel.Card.TutorialObjectId;

                            player.CardsInHand.Remove(card.BoardUnitModel);
                            player.BoardCards.Insert(InternalTools.GetSafePositionToInsert(card.FuturePositionOnBoard, player.BoardCards), boardUnitView);
                            player.AddCardToBoard(card.BoardUnitModel, (ItemPosition) card.FuturePositionOnBoard);
                            _battlegroundController.PlayerHandCards.Remove(card);
                            _battlegroundController.PlayerBoardCards.Insert(InternalTools.GetSafePositionToInsert(card.FuturePositionOnBoard,
                            _battlegroundController.PlayerBoardCards), boardUnitView);
                            _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                            InternalTools.DoActionDelayed(
                                     () =>
                                     {
                                         card.BoardUnitModel.Card.Owner.GraveyardCardsCount++;
                                     }, 1f);

                            card.RemoveCardParticle.Play();

                            _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitView.Model, false, _gameplayManager.CanDoDragActions);

                            _ranksController.UpdateRanksByElements(boardUnitView.Model.OwnerPlayer.BoardCards, boardUnitView.Model, RankBuffAction);

                            boardUnitView.PlayArrivalAnimation(playUniqueAnimation: true);
                            _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.CurrentPlayer,
                                () =>
                                {
                                    card.HandBoardCard.GameObject.SetActive(false);

                                    _abilitiesController.CallAbility(card, card.BoardUnitModel,
                                        Enumerators.CardKind.CREATURE, boardUnitView.Model, CallCardPlay, true, (status) =>
                                        {
                                            UpdateCardsStatusEvent?.Invoke(player);

                                            if (status)
                                            {
                                                player.ThrowPlayCardEvent(card.BoardUnitModel, card.FuturePositionOnBoard);
                                                OnPlayPlayerCard?.Invoke(new PlayCardOnBoard(boardUnitView, card.BoardUnitModel.Card.InstanceCard.Cost));
                                                if (card is UnitBoardCard)
                                                {
                                                    UnitBoardCard unitBoardCard = card as UnitBoardCard;
                                                    unitBoardCard.BoardUnitModel.Card.InstanceCard.Damage = boardUnitView.Model.MaxCurrentDamage;
                                                    unitBoardCard.BoardUnitModel.Card.InstanceCard.Defense = boardUnitView.Model.MaxCurrentDefense;
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
                            player.CardsInHand.Remove(card.BoardUnitModel);
                            _battlegroundController.PlayerHandCards.Remove(card);
                            _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                            card.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.BoardCards;
                            card.GameObject.GetComponent<SortingGroup>().sortingOrder = 1000;

                            BoardItem boardItem = new BoardItem(card.GameObject, card.BoardUnitModel);

                            card.RemoveCardParticle.Play();

                            InternalTools.DoActionDelayed(() =>
                            {
                                _abilitiesController.CallAbility(card, card.BoardUnitModel,
                                    Enumerators.CardKind.ITEM, boardItem, CallItemCardPlay, true, (status) =>
                                    {
                                        if(status)
                                        {
                                            player.ThrowPlayCardEvent(card.BoardUnitModel, 0);
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

        public void SummonUnitFromHand(Player player, BoardCardView card, bool activateAbility)
        {
            IReadOnlyCard prototype = card.BoardUnitModel.Card.Prototype;

            card.Transform.DORotate(Vector3.zero, .1f);

            if (card.HandBoardCard != null)
            {
                card.HandBoardCard.Enabled = false;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND,
                Constants.CardsMoveSoundVolume);

            GameObject board = player.IsLocalPlayer ? _playerBoard : _opponentBoard;

            BoardUnitView boardUnitView = new BoardUnitView(new BoardUnitModel(card.BoardUnitModel.Card), board.transform);
            boardUnitView.Transform.tag = player.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.parent = board.transform;
            boardUnitView.Transform.position = new Vector2(Constants.DefaultPositonOfUnitWhenSpawn * player.BoardCards.Count, 0);
            boardUnitView.Model.Card.Owner = card.BoardUnitModel.Card.Owner;
            boardUnitView.Model.Card.TutorialObjectId = card.BoardUnitModel.Card.TutorialObjectId;

            OpponentHandCard opponentHandCard = null;

            if(activateAbility)
            {
                _abilitiesController.ActivateAbilitiesOnCard(boardUnitView.Model, card.BoardUnitModel, player);
            }


            if (player.IsLocalPlayer)
            {
                _battlegroundController.PlayerHandCards.Remove(card);
                _battlegroundController.PlayerBoardCards.Insert(ItemPosition.End, boardUnitView);
            }
            else
            {
                opponentHandCard = _battlegroundController.OpponentHandCards.FirstOrDefault(cardOpponent => cardOpponent.BoardUnitModel.InstanceId == card.BoardUnitModel.Card.InstanceId);
                _battlegroundController.OpponentHandCards.Remove(opponentHandCard);
                _battlegroundController.OpponentBoardCards.Insert(ItemPosition.End, boardUnitView);
            }


            player.AddCardToBoard(card.BoardUnitModel, ItemPosition.End);
            player.RemoveCardFromHand(card.BoardUnitModel);
            player.BoardCards.Insert(ItemPosition.End, boardUnitView);

            InternalTools.DoActionDelayed(() =>
            {
                card.BoardUnitModel.Card.Owner.GraveyardCardsCount++;
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
            Action<BoardUnitModel> cardFoundCallback,
            Action<BoardUnitModel, BoardObject> completePlayCardCallback
            )
        {
            OpponentHandCard opponentHandCard;
            if(GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP || _gameplayManager.IsTutorial)
            {
                opponentHandCard =
                    _battlegroundController.OpponentHandCards.FirstOrDefault(x => x.BoardUnitModel.InstanceId == cardId);
            }
            else
            {
                if (_battlegroundController.OpponentHandCards.Count <= 0)
                    return;

                opponentHandCard = _battlegroundController.OpponentHandCards.FirstOrDefault(x => x.BoardUnitModel.InstanceId == cardId);
            }

            if(opponentHandCard is null)
            {
                Exception exception = new Exception($"[Out of sync] not found card in opponent hand! card Id: {cardId.Id}");
                Helpers.ExceptionReporter.SilentReportException(exception);
                Log.Error("", exception);
                return;
            }

            BoardUnitModel card = opponentHandCard.BoardUnitModel;

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

        public void DrawCardInfo(BoardUnitModel boardUnitModel)
        {
            GameObject go;
            BoardCardView boardCardView;
            switch (boardUnitModel.Prototype.CardKind)
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

        public void ReturnToHandBoardUnit(BoardUnitModel boardUnitModel, Player player, Vector3 cardPosition)
        {
            if (CheckIsMoreThanMaxCards(boardUnitModel, player))
                return;

            IView cardView = player.AddCardToHand(boardUnitModel, true);
            cardView.Transform.position = cardPosition;

            if (player.IsLocalPlayer)
            {
                cardView.Transform.localScale =
                    new Vector3(0.25f, 0.25f, 0.25f); // size of the cards in hand         
            }
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
                BoardCardView boardCardView = _battlegroundController.PlayerHandCards.First(x => x.BoardUnitModel.Card == boardUnitModel.Card);

                boardCardView.BoardUnitModel.Card.InstanceCard.Cost = Math.Max(boardCardView.BoardUnitModel.Card.InstanceCard.Cost + value, 0);
                boardCardView.UpdateCardCost();
            }
            else
            {
                boardUnitModel.Card.InstanceCard.Cost = Mathf.Clamp(boardUnitModel.Card.InstanceCard.Cost + value, 0, 99);
            }

            player.ThrowOnHandChanged();

            return boardUnitModel;
        }

        public void SetGooCostOfCardInHand(Player player, BoardUnitModel boardUnitModel, int value, BoardCardView boardCardView = null)
        {
            if (player.IsLocalPlayer)
            {
                if (boardCardView == null)
                {
                    boardCardView = _battlegroundController.PlayerHandCards.First(x => x.BoardUnitModel == boardUnitModel);
                }

                boardCardView.BoardUnitModel.Card.InstanceCard.Cost = value;
                boardCardView.UpdateCardCost();

                bool isActive = boardCardView.BoardUnitModel.Card.InstanceCard.Cost < boardCardView.BoardUnitModel.Card.Prototype.Cost;
                boardCardView.costHighlightObject.SetActive(isActive);
            }
            else
            {
                boardUnitModel.InstanceCard.Cost = Mathf.Clamp(value, 0, 99);
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
                    boardUnitModel.Prototype.CardKind,
                    boardUnitModel.Prototype.CardRank,
                    boardUnitModel.Prototype.CardType,
                    boardUnitModel.Prototype.Abilities
                        .Select(a => new AbilityData(a))
                        .ToList(),
                    new PictureTransform(boardUnitModel.Prototype.PictureTransform),
                    boardUnitModel.Prototype.UniqueAnimation,
                    boardUnitModel.Prototype.Hidden
                );
            }
        }

        public Enumerators.Faction GetSetOfCard(IReadOnlyCard card)
        {
            Faction set =
                _dataManager.CachedCardsLibraryData.Factions.Find(x => x.Cards.Find(y => y.Name.Equals(card.Name)) != null);

            return set.Name;
        }

        public BoardUnitModel CreateNewCardByNameAndAddToHand(Player player, string name)
        {
            float animationDuration = 1.5f;

            Card card = new Card(_dataManager.CachedCardsLibraryData.GetCardFromName(name));
            WorkingCard workingCard = new WorkingCard(card, card, player);
            if(_tutorialManager.IsTutorial)
            {
                workingCard.TutorialObjectId = DefaultIndexCustomCardForTutorial;
            }

            BoardUnitModel boardUnitModel = new BoardUnitModel(workingCard);

            if (CheckIsMoreThanMaxCards(boardUnitModel, player))
                return boardUnitModel;

            if (player.IsLocalPlayer)
            {
                BoardCardView boardCardView = CreateBoardCard(boardUnitModel);

                boardCardView.Transform.position = Vector3.zero;
                boardCardView.Transform.localScale = Vector3.zero;

                boardCardView.Transform.DOScale(Vector3.one * .3f, animationDuration);

                InternalTools.DoActionDelayed(() =>
                {
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerCreatedNewCardAndMovedToHand);
                }, animationDuration - Time.deltaTime);

                InternalTools.DoActionDelayed(() =>
                {
                    _battlegroundController.PlayerHandCards.Insert(ItemPosition.End, boardCardView);
                    player.CardsInHand.Insert(ItemPosition.End, boardUnitModel);

                    _battlegroundController.UpdatePositionOfCardsInPlayerHand(true);
                }, animationDuration);
            }
            else
            {
                OpponentHandCard handCard = AddCardToOpponentHand(boardUnitModel);
                handCard.Transform.position = Vector3.zero;
                handCard.Transform.localScale = Vector3.zero;

                handCard.Transform.DOScale(Vector3.one, animationDuration);

                _timerManager.AddTimer(
                    x =>
                    {
                        player.CardsInHand.Insert(ItemPosition.End, boardUnitModel);
                        _battlegroundController.UpdatePositionOfCardsInOpponentHand(true);
                    },
                    null,
                    animationDuration);
            }

            return boardUnitModel;
        }

        public BoardCardView GetBoardCard(BoardUnitModel boardUnitModel)
        {
            return CreateBoardCard(boardUnitModel);
        }

        public OpponentHandCard GetOpponentBoardCard(BoardUnitModel boardUnitModel)
        {
            return CreateOpponentHandCard(boardUnitModel);
        }

        public void ReturnCardToHand(BoardUnitView unit)
        {
            Player unitOwner = unit.Model.OwnerPlayer;
            BoardUnitModel returningCard = unit.Model;

            returningCard.Card.InstanceCard.Cost = returningCard.Card.Prototype.Cost;

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

            _playerBoard = GameObject.Find("PlayerBoard");
            _opponentBoard = GameObject.Find("OpponentBoard");
        }

        private void DirectlyEndCardDistribution(object[] param)
        {
            EndCardDistribution();
        }

        private BoardCardView CreateBoardCard(BoardUnitModel boardUnitModel)
        {
            GameObject go;
            BoardCardView boardCardView;
            switch (boardUnitModel.Card.Prototype.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    go = Object.Instantiate(CreatureCardViewPrefab);
                    boardCardView = new UnitBoardCard(go, boardUnitModel);
                    break;
                case Enumerators.CardKind.ITEM:
                    go = Object.Instantiate(ItemCardViewPrefab);
                    boardCardView = new ItemBoardCard(go, boardUnitModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            HandBoardCard handCard = new HandBoardCard(go, boardCardView);
            handCard.BoardZone = _playerBoard;
            boardCardView.HandBoardCard = handCard;
            handCard.CheckStatusOfHighlight();
            boardCardView.Transform.localScale = Vector3.one * .3f;

            _abilitiesController.CallAbilitiesInHand(boardCardView, boardUnitModel);

            return boardCardView;
        }

        private void CallCardPlay(BoardCardView card)
        {
        }

        private void CallItemCardPlay(BoardCardView card)
        {
        }

        private bool CheckIsMoreThanMaxCards(BoardUnitModel boardUnitModel, Player player)
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

            Card prototype = new Card(_dataManager.CachedCardsLibraryData.GetCardFromName(name));
            WorkingCard card = new WorkingCard(prototype, prototype, owner);
            BoardUnitModel boardUnitModel = new BoardUnitModel(card);

            return SpawnUnitOnBoard(owner, boardUnitModel, position, isPVPNetwork, onComplete);
        }

        public BoardUnitView SpawnUnitOnBoard(Player owner, BoardUnitModel boardUnitModel, ItemPosition position, bool isPVPNetwork = false, Action onComplete = null)
        {
            if (owner.BoardCards.Count >= owner.MaxCardsInPlay)
                return null;

            BoardUnitView unit = CreateBoardUnitForSpawn(boardUnitModel, owner);

            owner.AddCardToBoard(boardUnitModel, ItemPosition.End);

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

        private BoardUnitView CreateBoardUnitForSpawn(BoardUnitModel boardUnitModel, Player owner)
        {
            GameObject playerBoard = owner.IsLocalPlayer ? _battlegroundController.PlayerBoardObject : _battlegroundController.OpponentBoardObject;

            float unitYPositionOnBoard = owner.IsLocalPlayer ? -1.66f : 1.66f;

            if (boardUnitModel.Card.Owner != owner)
                throw new Exception("card.Owner != owner, shouldn't those be the same");

            BoardUnitView boardUnitView = new BoardUnitView(boardUnitModel, playerBoard.transform);
            boardUnitView.Transform.tag = owner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.parent = playerBoard.transform;
            boardUnitView.Transform.position = new Vector2(2f * owner.BoardCards.Count, unitYPositionOnBoard);

            boardUnitView.PlayArrivalAnimation();

            return boardUnitView;
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

            string prefabName = boardUnitModel.Prototype.CardKind == Enumerators.CardKind.CREATURE ? "Card_BoardUnit" : "Card_Item";

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
            string rarity = Enum.GetName(typeof(Enumerators.CardRank), boardUnitModel.Prototype.CardRank);
            string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

            if (!string.IsNullOrEmpty(boardUnitModel.Prototype.Frame))
            {
                frameName = "Images/Cards/Frames/" + boardUnitModel.Prototype.Frame;
            }

            _frame.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);
            _picture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>($"Images/Cards/Illustrations/{boardUnitModel.Prototype.Picture.ToLowerInvariant()}");

            _titleText.text = boardUnitModel.Prototype.Name;
            _descriptionText.text = choosableAbility.Description;
            _gooCostText.text = boardUnitModel.InstanceCard.Cost.ToString();

            if (boardUnitModel.Prototype.CardKind == Enumerators.CardKind.CREATURE)
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
