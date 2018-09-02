using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Gameplay;
using LoomNetwork.CZB.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class BattlegroundController : IController
    {
        public bool RearrangingTopBoard, IsPreviewActive;

        public bool CardsZoomed = false;

        public Coroutine CreatePreviewCoroutine;

        public GameObject CurrentBoardCard;

        public int CurrentPreviewedCardId;

        // public int TurnDuration { get; private set; }
        public int CurrentTurn;

        public bool GameFinished;

        public List<BoardUnit> OpponentBoardCards = new List<BoardUnit>();

        public List<BoardUnit> OpponentGraveyardCards = new List<BoardUnit>();

        public List<GameObject> OpponentHandCards = new List<GameObject>();

        public List<BoardUnit> PlayerBoardCards = new List<BoardUnit>();

        public GameObject PlayerBoardObject, OpponentBoardObject, PlayerGraveyardObject, OpponentGraveyardObject;

        public List<BoardUnit> PlayerGraveyardCards = new List<BoardUnit>();

        public List<BoardCard> PlayerHandCards = new List<BoardCard>();

        private AiController _aiController;

        private bool _battleDynamic;

        private CardsController _cardsController;

        private List<BoardUnit> _cardsInDestroy;

        private IDataManager _dataManager;

        private IGameplayManager _gameplayManager;

        private BoardUnit _lastBoardUntilOnPreview;

        private ILoadObjectsManager _loadObjectsManager;

        private PlayerController _playerController;

        private IPlayerManager _playerManager;

        private RanksController _ranksController;

        private ISoundManager _soundManager;

        private ITimerManager _timerManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private Sequence _rearrangingRealTimeSequence;

        public event Action<int> OnPlayerGraveyardUpdatedEvent;

        public event Action<int> OnOpponentGraveyardUpdatedEvent;

        public event Action OnTurnStartedEvent;

        public event Action OnTurnEndeddEvent;

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _playerManager = GameClient.Get<IPlayerManager>();

            _playerController = _gameplayManager.GetController<PlayerController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _aiController = _gameplayManager.GetController<AiController>();

            _ranksController = _gameplayManager.GetController<RanksController>();

            _cardsInDestroy = new List<BoardUnit>();

            LoadGameConfiguration();

            _gameplayManager.OnGameEndedEvent += OnGameEndedEventHandler;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            if (_gameplayManager.GameStarted && !_gameplayManager.GameEnded)
            {
                CheckGameDynamic();

                foreach (BoardUnit item in PlayerBoardCards)
                {
                    item.Update();
                }

                foreach (BoardUnit item in OpponentBoardCards)
                {
                    item.Update();
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

            if ((CurrentBoardCard != null) && CurrentBoardCard)
            {
                Object.Destroy(CurrentBoardCard);
            }

            CurrentBoardCard = null;

            ClearBattleground();
        }

        public void KillBoardCard(BoardUnit cardToDestroy)
        {
            if (cardToDestroy == null)

                return;

            if ((_lastBoardUntilOnPreview != null) && (cardToDestroy == _lastBoardUntilOnPreview))
            {
                DestroyCardPreview();
            }

            bool isOpponentCard = cardToDestroy.OwnerPlayer == _gameplayManager.CurrentPlayer?false:true;

            cardToDestroy.Transform.position = new Vector3(cardToDestroy.Transform.position.x, cardToDestroy.Transform.position.y, cardToDestroy.Transform.position.z + 0.2f);

            _timerManager.AddTimer(
                x =>
                {
                    cardToDestroy.Transform.DOShakePosition(.7f, 0.25f, 10, 90, false, false);

                    string cardDeathSoundName = cardToDestroy.Card.LibraryCard.Name.ToLower() + "_" + Constants.KCardSoundDeath;
                    float soundLength = 0f;

                    if (!cardToDestroy.OwnerPlayer.Equals(_gameplayManager.CurrentTurnPlayer))
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.Cards, cardDeathSoundName, Constants.ZombieDeathVoDelayBeforeFadeout, Constants.ZombiesSoundVolume, Enumerators.CardSoundType.Death);
                        soundLength = _soundManager.GetSoundLength(Enumerators.SoundType.Cards, cardDeathSoundName);
                    }

                    _timerManager.AddTimer(
                        t =>
                        {
                            cardToDestroy.OwnerPlayer.BoardCards.Remove(cardToDestroy);
                            cardToDestroy.OwnerPlayer.RemoveCardFromBoard(cardToDestroy.Card);
                            cardToDestroy.OwnerPlayer.AddCardToGraveyard(cardToDestroy.Card);

                            // _ranksController.UpdateRanksBuffs(cardToDestroy.ownerPlayer);
                            cardToDestroy.ThrowOnDieEvent();
                            cardToDestroy.Transform.DOKill();
                            Object.Destroy(cardToDestroy.GameObject);

                            _timerManager.AddTimer(
                                f =>
                                {
                                    UpdatePositionOfBoardUnitsOfOpponent();
                                    UpdatePositionOfBoardUnitsOfPlayer(_gameplayManager.CurrentPlayer.BoardCards);
                                },
                                null,
                                Time.deltaTime,
                                false);
                        },
                        null,
                        soundLength);
                },
                null,
                1f);
        }

        public void CheckGameDynamic()
        {
            // if (_gameplayManager.OpponentPlayer.HP > 9 && _gameplayManager.CurrentPlayer.HP > 9)
            // {
            // if (_battleDynamic)
            // _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);
            // _battleDynamic = false;
            // }
            // else
            // {
            if (!_battleDynamic)
            {
                _soundManager.CrossfaidSound(Enumerators.SoundType.Battleground, null, true);
            }

            _battleDynamic = true;

            // }
        }

        public void UpdateGraveyard(int index, Player player)
        {
            if (player.IsLocalPlayer)
            {
                OnPlayerGraveyardUpdatedEvent?.Invoke(index);
            } else
            {
                OnOpponentGraveyardUpdatedEvent?.Invoke(index);
            }
        }

        public void ClearBattleground()
        {
            PlayerHandCards.Clear();
            OpponentHandCards.Clear();

            PlayerBoardCards.Clear();
            OpponentBoardCards.Clear();

            PlayerGraveyardCards.Clear();
            OpponentGraveyardCards.Clear();

            _cardsInDestroy.Clear();
        }

        public void InitializeBattleground()
        {
            CurrentTurn = Constants.FirstGameTurnIndex;

            GameFinished = false;

            // _timerManager.StopTimer(RunTurnAsync);
            if (_gameplayManager.IsTutorial)
            {
                _gameplayManager.OpponentPlayer.Hp = 12;
                _gameplayManager.OpponentPlayer.GooOnCurrentTurn = 10;
                _gameplayManager.OpponentPlayer.Goo = 10;
                _gameplayManager.CurrentPlayer.GooOnCurrentTurn = 7;
                _gameplayManager.CurrentPlayer.Goo = 7;
            }

#if DEV_MODE
            _gameplayManager.OpponentPlayer.HP = 99;
            _gameplayManager.CurrentPlayer.HP = 99;
#endif

            _playerManager.OpponentGraveyardCards = OpponentGraveyardCards;

            PlayerBoardObject = GameObject.Find("PlayerBoard");
            OpponentBoardObject = GameObject.Find("OpponentBoard");
            PlayerGraveyardObject = GameObject.Find("GraveyardPlayer");
            OpponentGraveyardObject = GameObject.Find("GraveyardOpponent");
        }

        public void StartGameplayTurns()
        {
            StartTurn();

            if (!_gameplayManager.IsTutorial)
            {
                Player player = _gameplayManager.CurrentTurnPlayer.IsLocalPlayer?_gameplayManager.OpponentPlayer:_gameplayManager.CurrentPlayer;
                _cardsController.AddCardToHand(player);
            }

            // if (!_gameplayManager.IsTutorial)
            // _timerManager.AddTimer(RunTurnAsync, null, TurnDuration, true, false);
        }

        public void OnGameEndedEventHandler(Enumerators.EndGameType endGameType)
        {
            // _timerManager.StopTimer(RunTurnAsync);
            GameFinished = true;
            CurrentTurn = 0;

            ClearBattleground();
        }

        /* private void RunTurnAsync(object[] param)
         {
             EndTurn();

             if (!gameFinished)
                 StartTurn();
             else
                 _timerManager.StopTimer(RunTurnAsync);
         } */
        public void StartTurn()
        {
            if (_gameplayManager.GameEnded)

                return;

            CurrentTurn++;

            _gameplayManager.CurrentTurnPlayer.Turn++;

            if (_dataManager.CachedUserLocalData.Tutorial && !_tutorialManager.IsTutorial)
            {
                _tutorialManager.StartTutorial();
            }

            _uiManager.GetPage<GameplayPage>().SetEndTurnButtonStatus(_gameplayManager.IsLocalPlayerTurn());

            UpdatePositionOfCardsInOpponentHand();

            // RearrangeOpponentHand(!_gameplayManager.IsLocalPlayerTurn(), true);
            _playerController.IsActive = _gameplayManager.IsLocalPlayerTurn();

            if (_gameplayManager.IsLocalPlayerTurn())
            {
                List<BoardUnit> creatures = new List<BoardUnit>();

                foreach (BoardUnit card in PlayerBoardCards)
                {
                    if ((_playerController == null) || !card.GameObject)
                    {
                        creatures.Add(card);
                        continue;
                    }

                    card.OnStartTurn();
                }

                foreach (BoardUnit item in creatures)
                {
                    PlayerBoardCards.Remove(item);
                }

                creatures.Clear();
                creatures = null;

                foreach (BoardUnit card in PlayerBoardCards)
                {
                    card.SetHighlightingEnabled(true);
                }

                _uiManager.DrawPopup<YourTurnPopup>();
            } else
            {
                foreach (BoardUnit card in OpponentBoardCards)
                {
                    card.OnStartTurn();
                }

                foreach (BoardCard card in PlayerHandCards)
                {
                    card.SetHighlightingEnabled(false);
                }

                foreach (BoardUnit card in PlayerBoardCards)
                {
                    card.SetHighlightingEnabled(false);
                }
            }

            _gameplayManager.CurrentPlayer.CallOnStartTurnEvent();
            _gameplayManager.OpponentPlayer.CallOnStartTurnEvent();

            _playerController.UpdateHandCardsHighlight();

            OnTurnStartedEvent?.Invoke();
        }

        public void EndTurn()
        {
            if (_gameplayManager.GameEnded)

                return;

            if (_gameplayManager.IsLocalPlayerTurn())
            {
                _uiManager.GetPage<GameplayPage>().SetEndTurnButtonStatus(false);

                foreach (BoardUnit card in PlayerBoardCards)
                {
                    card.OnEndTurn();
                }
            } else
            {
                foreach (BoardUnit card in OpponentBoardCards)
                {
                    card.OnEndTurn();
                }
            }

            _gameplayManager.CurrentPlayer.CallOnEndTurnEvent();
            _gameplayManager.OpponentPlayer.CallOnEndTurnEvent();

            _gameplayManager.CurrentTurnPlayer = _gameplayManager.IsLocalPlayerTurn()?_gameplayManager.OpponentPlayer:_gameplayManager.CurrentPlayer;

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.EndTurn);

            OnTurnEndeddEvent?.Invoke();
        }

        public void StopTurn()
        {
            // _timerManager.StopTimer(RunTurnAsync);
            EndTurn();
            StartTurn();

            // if (!_gameplayManager.IsTutorial)
            // _timerManager.AddTimer(RunTurnAsync, null, TurnDuration, true, false);
        }

        public void RemovePlayerCardFromBoardToGraveyard(WorkingCard card)
        {
            Vector3 graveyardPos = PlayerGraveyardObject.transform.position + new Vector3(0.0f, -0.2f, 0.0f);
            BoardUnit boardCard = PlayerBoardCards.Find(x => x.Card == card);
            if (boardCard != null)
            {
                boardCard.Transform.localPosition = new Vector3(boardCard.Transform.localPosition.x, boardCard.Transform.localPosition.y, -0.2f);

                PlayerBoardCards.Remove(boardCard);
                PlayerGraveyardCards.Add(boardCard);

                boardCard.SetHighlightingEnabled(false);
                boardCard.StopSleepingParticles();
                boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.KLayerBoardCards;

                Object.Destroy(boardCard.GameObject.GetComponent<BoxCollider2D>());
            }
        }

        public void RemoveOpponentCardFromBoardToGraveyard(WorkingCard card)
        {
            Vector3 graveyardPos = OpponentGraveyardObject.transform.position + new Vector3(0.0f, -0.2f, 0.0f);
            BoardUnit boardCard = OpponentBoardCards.Find(x => x.Card == card);
            if (boardCard != null)
            {
                if (boardCard.Transform != null)
                {
                    boardCard.Transform.localPosition = new Vector3(boardCard.Transform.localPosition.x, boardCard.Transform.localPosition.y, -0.2f);
                }

                OpponentBoardCards.Remove(boardCard);

                boardCard.SetHighlightingEnabled(false);
                boardCard.StopSleepingParticles();
                if (boardCard.GameObject != null)
                {
                    boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.KLayerBoardCards;
                    Object.Destroy(boardCard.GameObject.GetComponent<BoxCollider2D>());
                }

                Debug.Log("Destroy = " + boardCard.CurrentHp + "_" + boardCard.Card.LibraryCard.Name);
            } else if ((_aiController.CurrentSpellCard != null) && (card == _aiController.CurrentSpellCard.WorkingCard))
            {
                _aiController.CurrentSpellCard.SetHighlightingEnabled(false);
                _aiController.CurrentSpellCard.GameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.KLayerBoardCards;
                Object.Destroy(_aiController.CurrentSpellCard.GameObject.GetComponent<BoxCollider2D>());
                Sequence sequence = DOTween.Sequence();
                sequence.PrependInterval(2.0f);
                sequence.Append(_aiController.CurrentSpellCard.Transform.DOMove(graveyardPos, 0.5f));
                sequence.Append(_aiController.CurrentSpellCard.Transform.DOScale(new Vector2(0.6f, 0.6f), 0.5f));
                sequence.OnComplete(
                    () =>
                    {
                        _aiController.CurrentSpellCard = null;
                    });
            }
        }

        public void UpdatePositionOfBoardUnitsOfPlayer(List<BoardUnit> cardsList, Action onComplete = null)
        {
            if (_gameplayManager.GameEnded)

                return;

            if (_rearrangingRealTimeSequence != null)
            {
                _rearrangingRealTimeSequence.Kill();
                _rearrangingRealTimeSequence = null;
            }

            float boardWidth = 0.0f;
            float spacing = 0.2f; // -0.2
            float cardWidth = 0.0f;
            foreach (BoardUnit card in cardsList)
            {
                cardWidth = 2.5f;
                boardWidth += cardWidth;
                boardWidth += spacing;
            }

            boardWidth -= spacing;

            List<Vector2> newPositions = new List<Vector2>(cardsList.Count);
            Vector3 pivot = PlayerBoardObject.transform.position;

            for (int i = 0; i < cardsList.Count; i++)
            {
                BoardUnit card = cardsList[i];
                newPositions.Add(new Vector2((pivot.x - (boardWidth / 2)) + (cardWidth / 2), pivot.y - 1.7f));
                pivot.x += boardWidth / cardsList.Count;
            }

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < cardsList.Count; i++)
            {
                BoardUnit card = cardsList[i];
                sequence.Insert(0, card.Transform.DOMove(newPositions[i], 0.4f).SetEase(Ease.OutSine));
            }

            _rearrangingRealTimeSequence = sequence;
            sequence.OnComplete(
                () =>
                {
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                });
        }

        public void UpdatePositionOfBoardUnitsOfOpponent(Action onComplete = null)
        {
            if (RearrangingTopBoard)
            {
                _timerManager.AddTimer(
                    x =>
                    {
                        UpdatePositionOfBoardUnitsOfOpponent(onComplete);
                    },
                    null,
                    .1f,
                    false);

                return;
            }

            if (_gameplayManager.GameEnded)

                return;

            List<BoardUnit> opponentBoardCards = _gameplayManager.OpponentPlayer.BoardCards;

            RearrangingTopBoard = true;

            float boardWidth = 0.0f;
            float spacing = 0.2f;
            float cardWidth = 0.0f;

            foreach (BoardUnit card in opponentBoardCards)
            {
                cardWidth = 2.5f;
                boardWidth += cardWidth;
                boardWidth += spacing;
            }

            boardWidth -= spacing;

            List<Vector2> newPositions = new List<Vector2>(opponentBoardCards.Count);
            Vector3 pivot = OpponentBoardObject.transform.position;

            for (int i = 0; i < opponentBoardCards.Count; i++)
            {
                BoardUnit card = opponentBoardCards[i];
                newPositions.Add(new Vector2((pivot.x - (boardWidth / 2)) + (cardWidth / 2), pivot.y + 0.0f));
                pivot.x += boardWidth / opponentBoardCards.Count;
            }

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < opponentBoardCards.Count; i++)
            {
                BoardUnit card = opponentBoardCards[i];
                sequence.Insert(0, card.Transform.DOMove(newPositions[i], 0.4f).SetEase(Ease.OutSine));
            }

            sequence.OnComplete(
                () =>
                {
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                });

            _timerManager.AddTimer(
                x =>
                {
                    RearrangingTopBoard = false;
                },
                null,
                1.5f,
                false);
        }

        // rewrite
        public void CreateCardPreview(object target, Vector3 pos, bool highlight = true)
        {
            IsPreviewActive = true;

            if (target is BoardCard)
            {
                CurrentPreviewedCardId = (target as BoardCard).WorkingCard.InstanceId;
            } else if (target is BoardUnit)
            {
                _lastBoardUntilOnPreview = target as BoardUnit;
                CurrentPreviewedCardId = (target as BoardUnit).Card.InstanceId;
            }

            CreatePreviewCoroutine = MainApp.Instance.StartCoroutine(CreateCardPreviewAsync(target, pos, highlight));
        }

        // rewrite
        public IEnumerator CreateCardPreviewAsync(object target, Vector3 pos, bool highlight)
        {
            yield return new WaitForSeconds(0.3f);

            WorkingCard card = null;

            if (target is BoardCard)
            {
                card = (target as BoardCard).WorkingCard;
            } else if (target is BoardUnit)
            {
                card = (target as BoardUnit).Card;
            }

            string cardSetName = _cardsController.GetSetOfCard(card.LibraryCard);

            BoardCard boardCard = null;
            if (card.LibraryCard.CardKind == Enumerators.CardKind.Creature)
            {
                CurrentBoardCard = Object.Instantiate(_cardsController.CreatureCardViewPrefab);
                boardCard = new UnitBoardCard(CurrentBoardCard);
            } else if (card.LibraryCard.CardKind == Enumerators.CardKind.Spell)
            {
                CurrentBoardCard = Object.Instantiate(_cardsController.SpellCardViewPrefab);
                boardCard = new SpellBoardCard(CurrentBoardCard);
            }

            boardCard.Init(card);
            if (highlight)
            {
                highlight = boardCard.CanBePlayed(card.Owner) && boardCard.CanBeBuyed(card.Owner);
            }

            boardCard.SetHighlightingEnabled(highlight);
            boardCard.IsPreview = true;

            InternalTools.SetLayerRecursively(boardCard.GameObject, 0);

            if (target is BoardUnit)
            {
                boardCard.DrawTooltipInfoOfUnit(target as BoardUnit);
            } else if (target is BoardCard)
            {
                boardCard.DrawTooltipInfoOfCard(target as BoardCard);
            }

            Vector3 newPos = pos;
            newPos.y += 2.0f;
            CurrentBoardCard.transform.position = newPos;
            CurrentBoardCard.transform.localRotation = Quaternion.Euler(Vector3.zero);

            Vector3 sizeOfCard = Vector3.one;

            if (!InternalTools.IsTabletScreen())
            {
                sizeOfCard = new Vector3(.8f, .8f, .8f);
            } else
            {
                sizeOfCard = new Vector3(.4f, .4f, .4f);
            }

            CurrentBoardCard.transform.localScale = sizeOfCard;

            CurrentBoardCard.GetComponent<SortingGroup>().sortingLayerName = Constants.KLayerGameUI3;
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

            foreach (BoardCard card in PlayerHandCards)
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
                BoardCard card = PlayerHandCards[i];
                float twist = startTwist - (i * twistPerCard);
                float nudge = Mathf.Abs(twist);

                nudge *= scalingFactor;
                moveToPosition = new Vector3(pivot.x - (handWidth / 2), pivot.y - nudge, (PlayerHandCards.Count - i) * 0.1f);

                if (isMove)
                {
                    card.IsNewCard = false;
                }

                card.UpdateCardPositionInHand(moveToPosition, Vector3.forward * twist, Vector3.one * scaling);

                pivot.x += handWidth / PlayerHandCards.Count;

                card.GameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.KLayerHandCards;
                card.GameObject.GetComponent<SortingGroup>().sortingOrder = i;
            }
        }

        public void UpdatePositionOfCardsInOpponentHand(bool isMove = false, bool isNewCard = false)
        {
            float handWidth = 0.0f;
            float spacing = -1.0f;

            foreach (GameObject card in OpponentHandCards)
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
            float scalingFactor = 0.04f;
            Vector3 movePosition = Vector3.zero;
            Vector3 rotatePosition = Vector3.zero;

            for (int i = 0; i < OpponentHandCards.Count; i++)
            {
                GameObject card = OpponentHandCards[i];
                float twist = startTwist - (i * twistPerCard);
                float nudge = Mathf.Abs(twist);

                nudge *= scalingFactor;
                movePosition = new Vector2(pivot.x - (handWidth / 2), pivot.y);
                rotatePosition = new Vector3(0, 0, twist);

                if (isMove)
                {
                    if ((i == OpponentHandCards.Count - 1) && isNewCard)
                    {
                        card.transform.position = new Vector3(-8.2f, 5.7f, 0);
                        card.transform.eulerAngles = Vector3.forward * 90f;
                    }

                    card.transform.DOMove(movePosition, 0.5f);
                    card.transform.DORotate(rotatePosition, 0.5f);
                } else
                {
                    card.transform.position = movePosition;
                    card.transform.rotation = Quaternion.Euler(rotatePosition);
                }

                pivot.x += handWidth / OpponentHandCards.Count;

                card.GetComponent<SortingGroup>().sortingOrder = i;
            }
        }

        public BoardUnit GetBoardUnitFromHisObject(GameObject unitObject)
        {
            BoardUnit unit = PlayerBoardCards.Find(x => x.GameObject.Equals(unitObject));

            if (unit == null)
            {
                unit = OpponentBoardCards.Find(x => x.GameObject.Equals(unitObject));
            }

            return unit;
        }

        public BoardCard GetBoardCardFromHisObject(GameObject cardObject)
        {
            BoardCard card = PlayerHandCards.Find(x => x.GameObject.Equals(cardObject));

            return card;
        }

        public void DestroyBoardUnit(BoardUnit unit)
        {
            if (unit != null)
            {
                unit.Die();
            }
        }

        public void TakeControlUnit(Player to, BoardUnit unit)
        {
            // implement functionality of the take control
        }

        public BoardUnit CreateBoardUnit(Player owner, WorkingCard card)
        {
            GameObject playerBoard = owner.IsLocalPlayer?PlayerBoardObject:OpponentBoardObject;

            BoardUnit boardUnit = new BoardUnit(playerBoard.transform);
            boardUnit.Transform.tag = owner.IsLocalPlayer?Constants.KTagPlayerOwned:Constants.KTagOpponentOwned;
            boardUnit.Transform.SetParent(playerBoard.transform);
            boardUnit.Transform.position = new Vector2(1.9f * owner.BoardCards.Count, 0);
            boardUnit.OwnerPlayer = owner;
            boardUnit.SetObjectInfo(card);

            boardUnit.PlayArrivalAnimation();

            return boardUnit;
        }

        private void LoadGameConfiguration()
        {
            // TurnDuration = Constants.DEFAULT_TURN_DURATION;

            // if (_gameplayManager.IsTutorial)
            // TurnDuration = 10000000;
        }
    }
}
