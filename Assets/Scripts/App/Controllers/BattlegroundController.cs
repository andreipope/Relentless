using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class BattlegroundController : IController
    {
        public bool IsPreviewActive;

        public bool CardsZoomed = false;

        public Coroutine CreatePreviewCoroutine;

        public GameObject CurrentBoardCard;

        public int CurrentPreviewedCardId;

        public int CurrentTurn;

        public List<BoardUnit> OpponentBoardCards = new List<BoardUnit>();

        public List<BoardUnit> OpponentGraveyardCards = new List<BoardUnit>();

        public List<GameObject> OpponentHandCards = new List<GameObject>();

        public List<BoardUnit> PlayerBoardCards = new List<BoardUnit>();

        public GameObject PlayerBoardObject, OpponentBoardObject, PlayerGraveyardObject, OpponentGraveyardObject;

        public List<BoardUnit> PlayerGraveyardCards = new List<BoardUnit>();

        public List<BoardCard> PlayerHandCards = new List<BoardCard>();

        private AIController _aiController;

        private bool _battleDynamic;

        private CardsController _cardsController;

        private List<BoardUnit> _cardsInDestroy;

        private IDataManager _dataManager;

        private IGameplayManager _gameplayManager;

        private BoardUnit _lastBoardUntilOnPreview;

        private PlayerController _playerController;

        private IPlayerManager _playerManager;

        private ISoundManager _soundManager;

        private ITimerManager _timerManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private Sequence _rearrangingTopRealTimeSequence, _rearrangingBottomRealTimeSequence;

        public event Action<int> PlayerGraveyardUpdated;

        public event Action<int> OpponentGraveyardUpdated;

        public event Action TurnStarted;

        public event Action TurnEnded;

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _playerManager = GameClient.Get<IPlayerManager>();

            _playerController = _gameplayManager.GetController<PlayerController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _aiController = _gameplayManager.GetController<AIController>();

            _cardsInDestroy = new List<BoardUnit>();

            _gameplayManager.GameEnded += GameEndedHandler;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            if (_gameplayManager.IsGameStarted && !_gameplayManager.IsGameEnded)
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

            if (CurrentBoardCard != null && CurrentBoardCard)
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

            if (_lastBoardUntilOnPreview != null && cardToDestroy == _lastBoardUntilOnPreview)
            {
                DestroyCardPreview();
            }

            cardToDestroy.Transform.position = new Vector3(cardToDestroy.Transform.position.x,
                cardToDestroy.Transform.position.y, cardToDestroy.Transform.position.z + 0.2f);

            _timerManager.AddTimer(
                x =>
                {
                    cardToDestroy.Transform.DOShakePosition(.7f, 0.25f, 10, 90, false, false);

                    string cardDeathSoundName =
                        cardToDestroy.Card.LibraryCard.Name.ToLower() + "_" + Constants.CardSoundDeath;
                    float soundLength = 0f;

                    if (!cardToDestroy.OwnerPlayer.Equals(_gameplayManager.CurrentTurnPlayer))
                    {
                        _soundManager.PlaySound(Enumerators.SoundType.CARDS, cardDeathSoundName,
                            Constants.ZombieDeathVoDelayBeforeFadeout, Constants.ZombiesSoundVolume,
                            Enumerators.CardSoundType.DEATH);
                        soundLength = _soundManager.GetSoundLength(Enumerators.SoundType.CARDS, cardDeathSoundName);
                    }

                    _timerManager.AddTimer(
                        t =>
                        {
                            cardToDestroy.OwnerPlayer.BoardCards.Remove(cardToDestroy);
                            cardToDestroy.OwnerPlayer.RemoveCardFromBoard(cardToDestroy.Card);
                            cardToDestroy.OwnerPlayer.AddCardToGraveyard(cardToDestroy.Card);

                            cardToDestroy.InvokeUnitDied();
                            cardToDestroy.Transform.DOKill();
                            Object.Destroy(cardToDestroy.GameObject);

                            _timerManager.AddTimer(
                                f =>
                                {
                                    UpdatePositionOfBoardUnitsOfOpponent();
                                    UpdatePositionOfBoardUnitsOfPlayer(_gameplayManager.CurrentPlayer.BoardCards);
                                },
                                null,
                                Time.deltaTime);
                        },
                        null,
                        soundLength);
                });
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

            PlayerBoardCards.Clear();
            OpponentBoardCards.Clear();

            PlayerGraveyardCards.Clear();
            OpponentGraveyardCards.Clear();

            _cardsInDestroy.Clear();
        }

        public void InitializeBattleground()
        {
            CurrentTurn = Constants.FirstGameTurnIndex;

            if (_gameplayManager.IsTutorial)
            {
                _gameplayManager.OpponentPlayer.Health = 12;
                _gameplayManager.OpponentPlayer.GooOnCurrentTurn = 10;
                _gameplayManager.OpponentPlayer.Goo = 10;
                _gameplayManager.CurrentPlayer.GooOnCurrentTurn = 7;
                _gameplayManager.CurrentPlayer.Goo = 7;
            }

#if DEV_MODE
            _gameplayManager.OpponentPlayer.Health = 99;
            _gameplayManager.CurrentPlayer.Health = 99;
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
                Player player = _gameplayManager.CurrentTurnPlayer.IsLocalPlayer ?
                    _gameplayManager.OpponentPlayer :
                    _gameplayManager.CurrentPlayer;
                _cardsController.AddCardToHand(player);
            }
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

            CurrentTurn++;

            _gameplayManager.CurrentTurnPlayer.Turn++;

            if (_dataManager.CachedUserLocalData.Tutorial && !_tutorialManager.IsTutorial)
            {
                _tutorialManager.StartTutorial();
            }

            _uiManager.GetPage<GameplayPage>().SetEndTurnButtonStatus(_gameplayManager.IsLocalPlayerTurn());

            UpdatePositionOfCardsInOpponentHand();
            _playerController.IsActive = _gameplayManager.IsLocalPlayerTurn();

            if (_gameplayManager.IsLocalPlayerTurn())
            {
                List<BoardUnit> creatures = new List<BoardUnit>();

                foreach (BoardUnit card in PlayerBoardCards)
                {
                    if (_playerController == null || !card.GameObject)
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

                foreach (BoardUnit card in PlayerBoardCards)
                {
                    card.SetHighlightingEnabled(true);
                }
            }
            else
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

            _gameplayManager.CurrentPlayer.InvokeTurnStarted();
            _gameplayManager.OpponentPlayer.InvokeTurnStarted();

            _playerController.UpdateHandCardsHighlight();

            TurnStarted?.Invoke();
        }

        public void EndTurn()
        {
            if (_gameplayManager.IsGameEnded)
                return;

            if (_gameplayManager.IsLocalPlayerTurn())
            {
                _uiManager.GetPage<GameplayPage>().SetEndTurnButtonStatus(false);

                foreach (BoardUnit card in PlayerBoardCards)
                {
                    card.OnEndTurn();
                }
            }
            else
            {
                foreach (BoardUnit card in OpponentBoardCards)
                {
                    card.OnEndTurn();
                }
            }

            _gameplayManager.CurrentPlayer.InvokeTurnEnded();
            _gameplayManager.OpponentPlayer.InvokeTurnEnded();

            _gameplayManager.CurrentTurnPlayer = _gameplayManager.IsLocalPlayerTurn() ?
                _gameplayManager.OpponentPlayer :
                _gameplayManager.CurrentPlayer;

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.END_TURN);

            TurnEnded?.Invoke();
        }

        public void StopTurn()
        {
            EndTurn();


            if (_gameplayManager.IsLocalPlayerTurn())
            {
                _uiManager.DrawPopup<YourTurnPopup>();

                _timerManager.AddTimer((x) =>
                {
                    StartTurn();
                }, null, 4f);
            }
            else
			{
                StartTurn();
			}
        }

        public void RemovePlayerCardFromBoardToGraveyard(WorkingCard card)
        {
            BoardUnit boardCard = PlayerBoardCards.Find(x => x.Card == card);
            if (boardCard == null)
                return;

            boardCard.Transform.localPosition = new Vector3(boardCard.Transform.localPosition.x,
                boardCard.Transform.localPosition.y, -0.2f);

            PlayerBoardCards.Remove(boardCard);
            PlayerGraveyardCards.Add(boardCard);

            boardCard.SetHighlightingEnabled(false);
            boardCard.StopSleepingParticles();
            boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LayerBoardCards;

            Object.Destroy(boardCard.GameObject.GetComponent<BoxCollider2D>());
        }

        public void RemoveOpponentCardFromBoardToGraveyard(WorkingCard card)
        {
            Vector3 graveyardPos = OpponentGraveyardObject.transform.position + new Vector3(0.0f, -0.2f, 0.0f);
            BoardUnit boardCard = OpponentBoardCards.Find(x => x.Card == card);
            if (boardCard != null)
            {
                if (boardCard.Transform != null)
                {
                    boardCard.Transform.localPosition = new Vector3(boardCard.Transform.localPosition.x,
                        boardCard.Transform.localPosition.y, -0.2f);
                }

                OpponentBoardCards.Remove(boardCard);

                boardCard.SetHighlightingEnabled(false);
                boardCard.StopSleepingParticles();
                if (boardCard.GameObject != null)
                {
                    boardCard.GameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LayerBoardCards;
                    Object.Destroy(boardCard.GameObject.GetComponent<BoxCollider2D>());
                }

                Debug.Log("Destroy = " + boardCard.CurrentHp + "_" + boardCard.Card.LibraryCard.Name);
            }
            else if (_aiController.CurrentSpellCard != null && card == _aiController.CurrentSpellCard.WorkingCard)
            {
                _aiController.CurrentSpellCard.SetHighlightingEnabled(false);
                _aiController.CurrentSpellCard.GameObject.GetComponent<SortingGroup>().sortingLayerName =
                    Constants.LayerBoardCards;
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
            if (_gameplayManager.IsGameEnded)
                return;

            if (_rearrangingBottomRealTimeSequence != null)
            {
                _rearrangingBottomRealTimeSequence.Kill();
                _rearrangingBottomRealTimeSequence = null;
            }

            float boardWidth = 0.0f;
            float spacing = 0.2f;
            float cardWidth = 0.0f;
            for (int i = 0; i < cardsList.Count; i++)
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
                newPositions.Add(new Vector2(pivot.x - boardWidth / 2 + cardWidth / 2, pivot.y - 1.7f));
                pivot.x += boardWidth / cardsList.Count;
            }

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < cardsList.Count; i++)
            {
                BoardUnit card = cardsList[i];
                sequence.Insert(0, card.Transform.DOMove(newPositions[i], 0.4f).SetEase(Ease.OutSine));
            }

            _rearrangingBottomRealTimeSequence = sequence;
            sequence.OnComplete(
                () =>
                {
                    onComplete?.Invoke();
                });
        }

        public void UpdatePositionOfBoardUnitsOfOpponent(Action onComplete = null)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            if (_rearrangingTopRealTimeSequence != null)
            {
                _rearrangingTopRealTimeSequence.Kill();
                _rearrangingTopRealTimeSequence = null;
            }

            List<BoardUnit> opponentBoardCards = _gameplayManager.OpponentPlayer.BoardCards;

            float boardWidth = 0.0f;
            float spacing = 0.2f;
            float cardWidth = 0.0f;

            for (int i = 0; i < opponentBoardCards.Count; i++)
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
                newPositions.Add(new Vector2(pivot.x - boardWidth / 2 + cardWidth / 2, pivot.y + 0.0f));
                pivot.x += boardWidth / opponentBoardCards.Count;
            }

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < opponentBoardCards.Count; i++)
            {
                BoardUnit card = opponentBoardCards[i];
                sequence.Insert(0, card.Transform.DOMove(newPositions[i], 0.4f).SetEase(Ease.OutSine));
            }

            _rearrangingTopRealTimeSequence = sequence;
            sequence.OnComplete(
                () =>
                {
                    onComplete?.Invoke();
                });
        }

        // rewrite
        public void CreateCardPreview(object target, Vector3 pos, bool highlight = true)
        {
            IsPreviewActive = true;

            switch (target)
            {
                case BoardCard card:
                    CurrentPreviewedCardId = card.WorkingCard.InstanceId;
                    break;
                case BoardUnit unit:
                    _lastBoardUntilOnPreview = unit;
                    CurrentPreviewedCardId = unit.Card.InstanceId;
                    break;
            }

            CreatePreviewCoroutine = MainApp.Instance.StartCoroutine(CreateCardPreviewAsync(target, pos, highlight));
        }

        // rewrite
        public IEnumerator CreateCardPreviewAsync(object target, Vector3 pos, bool highlight)
        {
            yield return new WaitForSeconds(0.3f);

            WorkingCard card = null;

            switch (target)
            {
                case BoardCard card1:
                    card = card1.WorkingCard;
                    break;
                case BoardUnit unit:
                    card = unit.Card;
                    break;
            }

            BoardCard boardCard;
            switch (card.LibraryCard.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    CurrentBoardCard = Object.Instantiate(_cardsController.CreatureCardViewPrefab);
                    boardCard = new UnitBoardCard(CurrentBoardCard);
                    break;
                case Enumerators.CardKind.SPELL:
                    CurrentBoardCard = Object.Instantiate(_cardsController.SpellCardViewPrefab);
                    boardCard = new SpellBoardCard(CurrentBoardCard);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            boardCard.Init(card);
            if (highlight)
            {
                highlight = boardCard.CanBePlayed(card.Owner) && boardCard.CanBeBuyed(card.Owner);
            }

            boardCard.SetHighlightingEnabled(highlight);
            boardCard.IsPreview = true;

            InternalTools.SetLayerRecursively(boardCard.GameObject, 0);

            switch (target)
            {
                case BoardUnit boardUnit:
                    boardCard.DrawTooltipInfoOfUnit(boardUnit);
                    break;
                case BoardCard tooltipCard:
                    tooltipCard.DrawTooltipInfoOfCard(tooltipCard);
                    break;
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
                BoardCard card = PlayerHandCards[i];
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

                card.GameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LayerHandCards;
                card.GameObject.GetComponent<SortingGroup>().sortingOrder = i;
            }
        }

        public void UpdatePositionOfCardsInOpponentHand(bool isMove = false, bool isNewCard = false)
        {
            float handWidth = 0.0f;
            float spacing = -1.0f;

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
                GameObject card = OpponentHandCards[i];
                float twist = startTwist - i * twistPerCard;

                Vector3 movePosition = new Vector2(pivot.x - handWidth / 2, pivot.y);
                Vector3 rotatePosition = new Vector3(0, 0, twist);

                if (isMove)
                {
                    if (i == OpponentHandCards.Count - 1 && isNewCard)
                    {
                        card.transform.position = new Vector3(-8.2f, 5.7f, 0);
                        card.transform.eulerAngles = Vector3.forward * 90f;
                    }

                    card.transform.DOMove(movePosition, 0.5f);
                    card.transform.DORotate(rotatePosition, 0.5f);
                }
                else
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
            unit?.Die();
        }

        public void TakeControlUnit(Player to, BoardUnit unit)
        {
            // implement functionality of the take control
        }

        public BoardUnit CreateBoardUnit(Player owner, WorkingCard card)
        {
            GameObject playerBoard = owner.IsLocalPlayer ? PlayerBoardObject : OpponentBoardObject;

            BoardUnit boardUnit = new BoardUnit(playerBoard.transform);
            boardUnit.Transform.tag = owner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnit.Transform.SetParent(playerBoard.transform);
            boardUnit.Transform.position = new Vector2(1.9f * owner.BoardCards.Count, 0);
            boardUnit.OwnerPlayer = owner;
            boardUnit.SetObjectInfo(card);

            boardUnit.PlayArrivalAnimation();

            return boardUnit;
        }
    }
}
