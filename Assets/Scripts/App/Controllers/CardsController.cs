using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
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

        private IGameplayManager _gameplayManager;

        private ITimerManager _timerManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private ISoundManager _soundManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private IMatchManager _matchManager;

        private BattlegroundController _battlegroundController;

        private VfxController _vfxController;

        private AbilitiesController _abilitiesController;

        private ActionsQueueController _actionsQueueController;

        private AnimationsController _animationsController;

        private RanksController _ranksController;

        private GameObject _playerBoard;

        private GameObject _opponentBoard;

        private BoardUnitView _fakeBoardCard;

        private int _cardInstanceId;

        private int _indexOfCard;

        public event Action<Player> UpdateCardsStatusEvent;

        public bool CardDistribution { get; set; }

        public List<WorkingCard> MulliganCards;

        public GameAction<object> PlayCardAction;
        public GameAction<object> RankBuffAction;
        public GameAction<object> CallAbilityAction;

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

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _vfxController = _gameplayManager.GetController<VfxController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _animationsController = _gameplayManager.GetController<AnimationsController>();
            _ranksController = _gameplayManager.GetController<RanksController>();

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

        public int GetNewCardInstanceId()
        {
            return _cardInstanceId++;
        }

        public int GetCardInstanceId()
        {
            return _cardInstanceId;
        }

        public void SetNewCardInstanceId(int id)
        {
            _cardInstanceId = id;
        }

        public void StartCardDistribution()
        {
            CardDistribution = true;

            GameClient.Get<ICameraManager>().FadeIn(0.8f, 0, false);

            if (_gameplayManager.IsTutorial || _gameplayManager.IsSpecificGameplayBattleground)
            {
                EndCardDistribution();
            }
            else
            {
                _uiManager.DrawPopup<MulliganPopup>();
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

            if (!_gameplayManager.IsTutorial)
            {
                _gameplayManager.CurrentPlayer.CardsInDeck =
                    _gameplayManager.CurrentPlayer.ShuffleCardsList(_gameplayManager.CurrentPlayer.CardsInDeck);
                _battlegroundController.StartGameplayTurns();
            }
            else
            {
                _battlegroundController.StartGameplayTurns();
            }

            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
            {
                SetNewCardInstanceId(Constants.MinDeckSize * 2);// 2 is players count
            }
        }

        public void CardsDistribution(List<WorkingCard> mulliganCards)
        {
            Player player = _gameplayManager.CurrentPlayer;
            List<WorkingCard> randomCards = InternalTools.GetRandomElementsFromList(
                player.CardsInDeck.Except(player.CardsPreparingToHand).ToList(),
                mulliganCards.Count);
            player.CardsPreparingToHand = player.CardsPreparingToHand.Except(mulliganCards).ToList();
            player.CardsPreparingToHand.AddRange(randomCards);

            EndCardDistribution();
        }

        public void AddCardToDistributionState(Player player, WorkingCard card)
        {
            player.CardsPreparingToHand.Add(card);
        }

        public void AddCardToHand(Player player, WorkingCard card = null, bool removeCardsFromDeck = true)
        {
            if (card == null)
            {
                if (player.CardsInDeck.Count == 0)
                {
                    if (!_tutorialManager.IsTutorial)
                    {
                        player.DamageByNoMoreCardsInDeck++;
                        player.Defense -= player.DamageByNoMoreCardsInDeck;
                        _vfxController.SpawnGotDamageEffect(player, -player.DamageByNoMoreCardsInDeck);
                    }
                    return;
                }

                card = player.CardsInDeck[0];
            }

            if (CheckIsMoreThanMaxCards(card, player))
                return;

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

            player.AddCardToHand(card);
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
                //await _gameplayManager.GetController<OpponentController>().ActionDrawCard(player, otherPlayer, player, Enumerators.AffectObjectType.PLAYER, card.LibraryCard.Name);
                MulliganCards?.Add(card);
            }
        }

        public GameObject AddCardToHand(WorkingCard card, bool silent = false)
        {
            BoardCard boardCard = CreateBoardCard(card);

            if (_battlegroundController.CurrentTurn == 0)
            {
                boardCard.SetDefaultAnimation(boardCard.WorkingCard.Owner.CardsInHand.Count);
            }

            _battlegroundController.PlayerHandCards.Add(boardCard);

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

            return boardCard.GameObject;
        }

        public GameObject AddCardToOpponentHand(WorkingCard card, bool silent = false)
        {
            GameObject go = CreateOpponentBoardCard();

            _battlegroundController.OpponentHandCards.Add(go);

            _abilitiesController.CallAbilitiesInHand(null, card);

            return go;
        }

        public GameObject CreateOpponentBoardCard()
        {
            Player opponent = _gameplayManager.OpponentPlayer;
            GameObject go = Object.Instantiate(OpponentCardPrefab);
            go.GetComponent<SortingGroup>().sortingOrder = opponent.CardsInHand.Count;

            return go;
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

            GameObject go = (GameObject) param[0];
            SortingGroup sortingGroup = go.GetComponent<SortingGroup>();

            Sequence animationSequence3 = DOTween.Sequence();

            animationSequence3.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0, -30f), .4f));
            go.transform.DOScale(new Vector3(1, 1, 1), .2f);

            Sequence animationSequence2 = DOTween.Sequence();
            animationSequence2.Append(go.transform.DOMove(new Vector3(7.7f, 14f, 0), .6f));

            animationSequence2.OnComplete(
                () =>
                {
                    go.layer = 0;
                    for (int i = 0; i < go.transform.childCount; i++)
                    {
                        go.transform.GetChild(i).gameObject.layer = 0;
                    }

                    sortingGroup.sortingOrder = 7; // Foreground layer

                    Sequence animationSequence4 = DOTween.Sequence();
                    animationSequence4.Append(go.transform.DORotate(new Vector3(go.transform.eulerAngles.x, 0f, 0f), .2f));

                    Sequence animationSequence5 = DOTween.Sequence();
                    animationSequence5.Append(go.transform.DOMove(new Vector3(7.7f, 6.306f, 0), .5f));
                    animationSequence5.OnComplete(
                        () =>
                        {
                            Object.Destroy(go);
                        });
                });
        }

        public void HoverPlayerCardOnBattleground(Player player, BoardCard card, HandBoardCard handCard)
        {
            Card libraryCard = card.WorkingCard.LibraryCard;
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

                    List<BoardUnitView> playerCards = _gameplayManager.CurrentPlayer.BoardCards;
                    List<BoardUnitView> toArrangeList = new List<BoardUnitView>();

                    for (int i = 0; i < playerCards.Count; i++)
                    {
                        toArrangeList.Add(playerCards[i]);
                    }

                    if (_fakeBoardCard != null)
                    {
                        Object.Destroy(_fakeBoardCard.GameObject);
                        _fakeBoardCard = null;
                    }

                    _fakeBoardCard = new BoardUnitView(new BoardUnitModel(), _playerBoard.transform);
                    toArrangeList.Insert(_indexOfCard, _fakeBoardCard);

                    _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(toArrangeList);
                }
            }
        }

        public void ResetPlayerCardsOnBattlegroundPosition()
        {
            if (_indexOfCard != -1)
            {
                _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(_gameplayManager.CurrentPlayer.BoardCards);
                _indexOfCard = -1;
                if (_fakeBoardCard != null)
                {
                    Object.Destroy(_fakeBoardCard.GameObject);
                    _fakeBoardCard = null;
                }
            }
        }

        public void PlayPlayerCard(Player player, BoardCard card, HandBoardCard handCard, Action<PlayCardOnBoard> OnPlayPlayerCard)
        {
            if (card.CanBePlayed(card.WorkingCard.Owner))
            {
                Card libraryCard = card.WorkingCard.LibraryCard;

                card.Transform.DORotate(Vector3.zero, .1f);
                card.HandBoardCard.Enabled = false;

                _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND,
                    Constants.CardsMoveSoundVolume);


                GameAction<object> waiterAction = _actionsQueueController.AddNewActionInToQueue(null);
                RankBuffAction = _actionsQueueController.AddNewActionInToQueue(null);
                CallAbilityAction = _actionsQueueController.AddNewActionInToQueue(null);

                switch (libraryCard.CardKind)
                {
                    case Enumerators.CardKind.CREATURE:
                        {
                            int indexOfCard = 0;
                            float newCreatureCardPosition = card.Transform.position.x;

                            // set correct position on board depends from card view position
                            for (int i = 0; i < player.BoardCards.Count; i++)
                            {
                                if (newCreatureCardPosition > player.BoardCards[i].Transform.position.x)
                                {
                                    indexOfCard = i + 1;
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

                            _battlegroundController.PlayerHandCards.Remove(card);
                            _battlegroundController.PlayerBoardCards.Add(boardUnitView);
                            player.AddCardToBoard(card.WorkingCard);
                            player.RemoveCardFromHand(card.WorkingCard);

                            player.BoardCards.Insert(indexOfCard, boardUnitView);

                            InternalTools.DoActionDelayed(
                                     () =>
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

                            UpdateCardsStatusEvent?.Invoke(player);

                            _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitView.Model, false, _gameplayManager.CanDoDragActions);

                            _ranksController.UpdateRanksByElements(boardUnitView.Model.OwnerPlayer.BoardCards, boardUnitView.Model.Card, RankBuffAction);

                            player.ThrowPlayCardEvent(card.WorkingCard, player.BoardCards.Count - 1 - indexOfCard);

                            Sequence animationSequence = DOTween.Sequence();
                            animationSequence.Append(card.Transform.DOScale(new Vector3(.27f, .27f, .27f), 1f));
                            animationSequence.OnComplete(
                                () =>
                                {
                                    RemoveCard(new object[]
                                    {
                                                    card
                                    });

                                    InternalTools.DoActionDelayed(
                                        () =>
                                        {
                                            boardUnitView.PlayArrivalAnimation();
                                            _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(
                                                _gameplayManager.CurrentPlayer.BoardCards,
                                                () =>
                                                {
                                                    _abilitiesController.CallAbility(libraryCard, card, card.WorkingCard,
                                                        Enumerators.CardKind.CREATURE, boardUnitView.Model, CallCardPlay, true, null, CallAbilityAction);

                                                    waiterAction.ForceActionDone();
                                                });
                                        }, 0.1f);
                                });

                            OnPlayPlayerCard?.Invoke(new PlayCardOnBoard(boardUnitView, card.ManaCost));

                            player.CurrentGoo -= card.ManaCost;
                            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.MOVE_CARD);
                            GameClient.Get<IOverlordManager>().ReportExperienceAction(player.SelfHero, Common.Enumerators.ExperienceActionType.PlayCard);
                            break;
                        }
                    case Enumerators.CardKind.SPELL:
                        {
                            player.CardsInHand.Remove(card.WorkingCard);
                            _battlegroundController.PlayerHandCards.Remove(card);
                            _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                            card.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.BoardCards;
                            card.GameObject.GetComponent<SortingGroup>().sortingOrder = 1000;

                            BoardSpell boardSpell = new BoardSpell(card.GameObject, card.WorkingCard);
                            boardSpell.Transform.position = Vector3.zero;

                            _abilitiesController.CallAbility(libraryCard, card, card.WorkingCard,
                                Enumerators.CardKind.SPELL, boardSpell, CallSpellCardPlay, true, null, CallAbilityAction, handCard: handCard);

                            waiterAction.ForceActionDone();
                            RankBuffAction.ForceActionDone();
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

        public void SummonUnitFromHand(Player player, BoardCard card)
        {
            Card libraryCard = card.WorkingCard.LibraryCard;

            card.Transform.DORotate(Vector3.zero, .1f);

            if (card.HandBoardCard != null)
            {
                card.HandBoardCard.Enabled = false;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND,
                Constants.CardsMoveSoundVolume);

            int indexOfCard = 0;
            float newCreatureCardPosition = card.Transform.position.x;

            // set correct position on board depends from card view position
            for (int i = 0; i < player.BoardCards.Count; i++)
            {
                if (newCreatureCardPosition > player.BoardCards[i].Transform.position.x)
                {
                    indexOfCard = i + 1;
                }
                else
                {
                    break;
                }
            }

            GameObject board = player.IsLocalPlayer ? _playerBoard : _opponentBoard;

            BoardUnitView boardUnitView = new BoardUnitView(new BoardUnitModel(), board.transform);
            boardUnitView.Transform.tag = player.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.parent = board.transform;
            boardUnitView.Transform.position = new Vector2(Constants.DefaultPositonOfUnitWhenSpawn * player.BoardCards.Count, 0);
            boardUnitView.Model.OwnerPlayer = card.WorkingCard.Owner;
            boardUnitView.SetObjectInfo(card.WorkingCard);

            if (player.IsLocalPlayer)
            {
                _battlegroundController.PlayerHandCards.Remove(card);
                _battlegroundController.PlayerBoardCards.Add(boardUnitView);
            }
            else
            {
                _battlegroundController.OpponentBoardCards.Add(boardUnitView);
            }


            player.AddCardToBoard(card.WorkingCard);
            player.RemoveCardFromHand(card.WorkingCard);
            player.BoardCards.Insert(indexOfCard, boardUnitView);

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

            Sequence animationSequence = DOTween.Sequence();
            animationSequence.Append(card.Transform.DOScale(new Vector3(.27f, .27f, .27f), 1f));
            animationSequence.OnComplete(() =>
            {
                RemoveCard(new object[]
                {
                        card
                });

                InternalTools.DoActionDelayed(() =>
                {
                    boardUnitView.PlayArrivalAnimation();

                    if (player.IsLocalPlayer)
                    {
                        _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(player.BoardCards);
                    }
                    else
                    {
                        _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
                    }
                }, 0.1f);
            });
        }


        public void ShuffleCardToPlayerHand(WorkingCard card, Player player)
        {
            player.AddCardToDeck(card, true);
        }

        public void PlayOpponentCard(
            Player player, WorkingCard card, BoardObject target, Action<WorkingCard, BoardObject> completePlayCardCallback)
        {
            GameObject randomCard;

            if (_battlegroundController.OpponentHandCards.Count > 0)
            {
                randomCard = _battlegroundController.OpponentHandCards[
                    Random.Range(0, _battlegroundController.OpponentHandCards.Count)];

                _battlegroundController.OpponentHandCards.Remove(randomCard);
            }
            else
            {
			    #warning hot fix - visual bug will appear! temp solution!
                if(GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
                {
                    randomCard = CreateOpponentBoardCard();

                    _battlegroundController.UpdatePositionOfCardsInOpponentHand();
                }
                else return;
            }

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.MOVE_CARD);

            _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND_TO_BATTLEGROUND,
                Constants.CardsMoveSoundVolume);

            randomCard.transform.DOMove(Vector3.up * 2.5f, 0.6f).OnComplete(
                () =>
                {
                    randomCard.transform.Find("RemoveCardParticle").GetComponent<ParticleSystem>().Play();

                    randomCard.transform.DOScale(Vector3.one * 1.2f, 0.6f).OnComplete(
                        () =>
                        {
                            RemoveOpponentCard(new object[]
                            {
                                randomCard
                            });

                            _timerManager.AddTimer(
                                x =>
                                {
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

            randomCard.transform.DORotate(Vector3.zero, 0.5f);

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
                case Enumerators.CardKind.SPELL:
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

            GameObject cardObject = player.AddCardToHand(workingCard, true);
            cardObject.transform.position = cardPosition;

            if (player.IsLocalPlayer)
            {
                cardObject.transform.localScale =
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
                BoardCard boardCard = _battlegroundController.PlayerHandCards.Find(x => x.WorkingCard.Equals(card));

                boardCard.ChangeCardCostOn(value, true);
            }
            else
            {
                card.RealCost += value;
            }

            return card;
        }

        public void SetGooCostOfCardInHand(Player player, WorkingCard card, int value, BoardCard boardCard = null)
        {
            if (player.IsLocalPlayer)
            {
                if (boardCard == null)
                {
                    boardCard = _battlegroundController.PlayerHandCards.Find(x => x.WorkingCard.Equals(card));
                }

                boardCard.SetCardCost(value);

                bool isActive = boardCard.WorkingCard.RealCost < boardCard.WorkingCard.InitialCost;
                boardCard.costHighlightObject.SetActive(isActive);
            }
            else
            {
                card.RealCost = Mathf.Clamp(value, 0, 99);
                card.LibraryCard.Cost = card.RealCost;
            }
        }

        public string GetSetOfCard(Card card)
        {
            CardSet set =
                _dataManager.CachedCardsLibraryData.Sets.Find(x => x.Cards.Find(y => y.Name.Equals(card.Name)) != null);

            if (set != null)
            {
                return set.Name;
            }

            return string.Empty;
        }

        public WorkingCard CreateNewCardByNameAndAddToHand(Player player, string name)
        {
            float animationDuration = 1.5f;

            Card card = _dataManager.CachedCardsLibraryData.GetCardFromName(name).Clone();
            WorkingCard workingCard = new WorkingCard(card, player);

            if (CheckIsMoreThanMaxCards(workingCard, player))
                return workingCard;

            if (player.IsLocalPlayer)
            {
                BoardCard boardCard = CreateBoardCard(workingCard);

                boardCard.Transform.position = Vector3.zero;
                boardCard.Transform.localScale = Vector3.zero;

                boardCard.Transform.DOScale(Vector3.one * .3f, animationDuration);

                _timerManager.AddTimer(
                    x =>
                    {
                        _battlegroundController.PlayerHandCards.Add(boardCard);

                        player.CardsInHand.Add(workingCard);

                        _battlegroundController.UpdatePositionOfCardsInPlayerHand(true);
                    },
                    null,
                    animationDuration);
            }
            else
            {
                GameObject boardCard = AddCardToOpponentHand(workingCard);
                boardCard.transform.position = Vector3.zero;
                boardCard.transform.localScale = Vector3.zero;

                boardCard.transform.DOScale(Vector3.one, animationDuration);

                _timerManager.AddTimer(
                    x =>
                    {
                        player.CardsInHand.Add(workingCard);
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

        public GameObject GetOpponentBoardCard(WorkingCard card)
        {
            return CreateOpponentBoardCard();
        }

        public void ReturnCardToHand(BoardUnitView unit)
        {
            Player unitOwner = unit.Model.OwnerPlayer;
            WorkingCard returningCard = unit.Model.Card;

            returningCard.InitialCost = returningCard.LibraryCard.Cost;
            returningCard.RealCost = returningCard.InitialCost;

            Vector3 unitPosition = unit.Transform.position;

            unit.Model.InvokeUnitPrepairingToDie();

            _timerManager.AddTimer(
                x =>
                {
                    // STEP 1 - REMOVE UNIT FROM BOARD
                    unitOwner.BoardCards.Remove(unit);

                    // STEP 2 - DESTROY UNIT ON THE BOARD OR ANIMATE
                    unit.Model.Die(true);
                    Object.Destroy(unit.GameObject);

                    // STEP 3 - REMOVE WORKING CARD FROM BOARD
                    unitOwner.RemoveCardFromBoard(returningCard);

                    // STEP 4 - RETURN CARD TO HAND
                    ReturnToHandBoardUnit(returningCard, unitOwner, unitPosition);

                    // STEP 4 - REARRANGE HANDS
                    _gameplayManager.RearrangeHands();
                },
                null,
                2f);
        }

        public WorkingCard GetWorkingCardFromName(Player owner, string cardName)
        {
            return new WorkingCard(_dataManager.CachedCardsLibraryData.GetCardFromName(cardName), owner);
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
                case Enumerators.CardKind.SPELL:
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

        public BoardUnitView SpawnUnitOnBoard(Player owner, string name, bool isPVPNetwork = false, Action onComplete = null, int position = 9999)
        {
            if (owner.BoardCards.Count >= owner.MaxCardsInPlay)
                return null;

            Card libraryCard = _dataManager.CachedCardsLibraryData.GetCardFromName(name).Clone();

            WorkingCard card = new WorkingCard(libraryCard, owner);
            BoardUnitView unit = CreateBoardUnitForSpawn(card, owner);

            owner.AddCardToBoard(card);

            if (isPVPNetwork)
            {
                owner.BoardCards.Insert(0, unit);
            }
            else
            {
                if (position <= Constants.MaxBoardUnits)
                {
                    owner.BoardCards.Insert(position, unit);
                }
                else
                {
                    owner.BoardCards.Add(unit);
                }
            }

            _abilitiesController.ResolveAllAbilitiesOnUnit(unit.Model);

            if (!owner.IsLocalPlayer)
            {
                _battlegroundController.OpponentBoardCards.Add(unit);
                _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent(onComplete);
            }
            else
            {
                _battlegroundController.PlayerBoardCards.Add(unit);
                _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(owner.BoardCards, onComplete);
            }

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

            if (!owner.Equals(_gameplayManager.CurrentTurnPlayer))
            {
                boardUnitView.Model.IsPlayable = true;
            }

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

            string setName = card.LibraryCard.CardSetType.ToString();
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
            _gooCostText.text = card.Damage.ToString();

            if (card.LibraryCard.CardKind == Enumerators.CardKind.CREATURE)
            {
                _unitType = SelfObject.transform.Find("Image_Type").GetComponent<SpriteRenderer>();

                _attackText = SelfObject.transform.Find("Text_Attack").GetComponent<TextMeshPro>();
                _defenseText = SelfObject.transform.Find("Text_Defense").GetComponent<TextMeshPro>();

                _attackText.text = card.Damage.ToString();
                _defenseText.text = card.Health.ToString();

                _unitType.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/{0}", card.Type + "_icon"));
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
