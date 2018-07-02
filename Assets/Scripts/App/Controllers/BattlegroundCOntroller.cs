using DG.Tweening;
using GrandDevs.CZB.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace GrandDevs.CZB
{
    public class BattlegroundController : IController
    {
        public event Action<BoardCreature> OnBoardCardKilledEvent;
        public event Action<int> OnPlayerGraveyardUpdatedEvent;
        public event Action<int> OnOpponentGraveyardUpdatedEvent;
        public event Action OnTurnStartedEvent;
        public event Action OnTurnEndeddEvent;

        private IGameplayManager _gameplayManager;
        private ITimerManager _timerManager;
        private ISoundManager _soundManager;
        private IDataManager _dataManager;
        private ITutorialManager _tutorialManager;
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IPlayerManager _playerManager;


        private CardsController _cardsController;
        private PlayerController _playerController;
        private AIController _aiController;

        private bool _battleDynamic = false;

        private Coroutine _turnCoroutine;

        public int TurnDuration { get; private set; }
        public int currentPlayerIndex;
        public int currentTurn;
        public bool gameFinished;

        public GameObject currentCardPreview;
        public int currentPreviewedCardId;


        public bool _rearrangingBottomBoard,
                    _rearrangingTopBoard,
                    isPreviewActive;

        public Coroutine createPreviewCoroutine;

        public List<CardView> playerHandCards = new List<CardView>();
        public List<GameObject> opponentHandCards = new List<GameObject>();

        public List<BoardCreature> playerBoardCards = new List<BoardCreature>();
        public List<BoardCreature> opponentBoardCards = new List<BoardCreature>();

        public List<BoardCreature> playerGraveyardCards = new List<BoardCreature>();
        public List<BoardCreature> opponentGraveyardCards = new List<BoardCreature>();


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
            _aiController = _gameplayManager.GetController<AIController>();

            LoadGameConfiguration();

            _gameplayManager.OnGameEndedEvent += OnGameEndedEventHandler;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            if(_gameplayManager.GameStarted && !_gameplayManager.GameEnded)
            {
                CheckGameDynamic();
            }
        }

        private void LoadGameConfiguration()
        {
            TurnDuration = Constants.DEFAULT_TURN_DURATION;

            if (_gameplayManager.IsTutorial)
                TurnDuration = 100000;
        }

        public void KillBoardCard(BoardCreature card)
        {
            OnBoardCardKilledEvent?.Invoke(card);
        }

        public void CheckGameDynamic()
        {
            if (_gameplayManager.GetOpponentPlayer().HP > 9 && _gameplayManager.GetLocalPlayer().HP > 9)
            {
                if (_battleDynamic)
                    _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);
                _battleDynamic = false;
            }
            else
            {
                if (!_battleDynamic)
                    _soundManager.CrossfaidSound(Enumerators.SoundType.BATTLEGROUND, null, true);
                _battleDynamic = true;
            }
        }


        public void UpdateGraveyard(int index, Player player)
        {
            if (player.IsLocalPlayer)
                OnPlayerGraveyardUpdatedEvent?.Invoke(index);
            else
                OnOpponentGraveyardUpdatedEvent?.Invoke(index);
        }

        public void ClearBattleground()
        {
            playerHandCards.Clear();
            opponentHandCards.Clear();

            playerBoardCards.Clear();
            opponentBoardCards.Clear();

            playerGraveyardCards.Clear();
            opponentGraveyardCards.Clear();
        }

        public void InitializeBattleground()
        {
            // Start with turn 1.
            currentTurn = 1;

            gameFinished = false;

            var players = _gameplayManager.PlayersInGame;

            // Create an array with all the player nicknames.
            var playerNicknames = new List<string>(players.Count);
            foreach (var player in players)
                playerNicknames.Add(player.nickname);

            if (_gameplayManager.IsTutorial)
                players.Find(x => !x.IsLocalPlayer).HP = 8;

            if (Constants.DEV_MODE)
                players.Find(x => !x.IsLocalPlayer).HP = 99;


            _turnCoroutine = MainApp.Instance.StartCoroutine(RunTurn());


            _playerManager.PlayerGraveyardCards = playerGraveyardCards;
            _playerManager.OpponentGraveyardCards = opponentGraveyardCards;
        }
    
        public void OnGameEndedEventHandler()
        {
            if (_gameplayManager.IsTutorial)
                return;

            gameFinished = true;

            ClearBattleground();
        }

        private IEnumerator RunTurn()
        {
            while (!gameFinished)
            {
                StartTurn();
                yield return new WaitForSeconds(TurnDuration);
                EndTurn();
            }
        }

        public void StartTurn()
        {
            if (_gameplayManager.GameEnded)
                return;

            var players = _gameplayManager.PlayersInGame;


            _gameplayManager.WhoseTurn.turn++;


            if (_dataManager.CachedUserLocalData.tutorial && !_tutorialManager.IsTutorial)
                _tutorialManager.StartTutorial();

            _uiManager.GetPage<GameplayPage>().SetEndTurnButtonStatus(_gameplayManager.IsLocalPlayerTurn());


            //foreach (var card in opponentHandCards)
            //    MonoBehaviour.Destroy(card);

            //opponentHandCards.Clear();

            //for (var i = 0; i < _gameplayManager.GetOpponentPlayer().CardsInHand.Count; i++)
            //{
            //    //if (i == _gameplayManager.GetOpponentPlayer().CardsInHand.Count - 1)
            //    //    RearrangeOpponentHand();

            //    _cardsController.AddCardToOpponentHand(null);
            //}
            RearrangeOpponentHand();
            //RearrangeOpponentHand(!_gameplayManager.IsLocalPlayerTurn(), true);

            _playerController.IsActive = _gameplayManager.IsLocalPlayerTurn();

            if (_gameplayManager.IsLocalPlayerTurn())
            {
                _playerController.UpdateHandCardsHighlight();

                List<BoardCreature> creatures = new List<BoardCreature>();

                foreach (var card in playerBoardCards)
                {
                    if (_playerController == null || !card.gameObject)
                    {
                        creatures.Add(card);
                        continue;
                    }

                    card.OnStartTurn();
                }

                foreach (var item in creatures)
                    playerBoardCards.Remove(item);
                creatures.Clear();
                creatures = null;

                if (_playerController.PlayerInfo.CurrentBoardWeapon != null && !_playerController.IsPlayerStunned)
                {
                    _playerController.AlreadyAttackedInThisTurn = false;
                    _playerController.PlayerInfo.CurrentBoardWeapon.ActivateWeapon(false);
                }

                _uiManager.DrawPopup<YourTurnPopup>();

                //   StartTurnCountdown(Constants.DEFAULT_TURN_DURATION);
            }
            else
            {
                foreach (var card in opponentBoardCards)
                {
                    card.OnStartTurn();
                }

                foreach (var card in playerHandCards)
                {
                    card.SetHighlightingEnabled(false);
                }
                foreach (var card in playerBoardCards)
                {
                    card.SetHighlightingEnabled(false);
                }

                //HideTurnCountdown();
            }

            foreach (var player in players)
                player.CallOnStartTurnEvent();

            OnTurnStartedEvent?.Invoke();
        }


        //public void StartTurnCountdown(int time)
        //{
        //    MainApp.Instance.StartCoroutine(StartCountdown(time));
        //}

        //public void HideTurnCountdown()
        //{
        //}

        //private IEnumerator StartCountdown(int time)
        //{
        //    while (time >= 0)
        //    {
        //        yield return new WaitForSeconds(1.0f);
        //        time -= 1;
        //    }
        //}

        //public void StopCountdown()
        //{
        //    MainApp.Instance.StopAllCoroutines();
        //}

        public void EndTurn()
        {
            if (_gameplayManager.GameEnded)
                return;

            var players = _gameplayManager.PlayersInGame;

            // Switch to next player.
            currentPlayerIndex += 1;
            if (currentPlayerIndex == players.Count)
            {
                currentPlayerIndex = 0;
                // Increase turn count.
                currentTurn += 1;
            }



            if (_gameplayManager.IsLocalPlayerTurn())
            {
                _uiManager.GetPage<GameplayPage>().SetEndTurnButtonStatus(false);

                foreach (var card in playerBoardCards)
                    card.OnEndTurn();

                GameObject.Find("Player/Spell").GetComponent<BoardSkill>().OnEndTurn();

                if (_playerController.currentCreature != null)
                {
                    playerBoardCards.Remove(_playerController.currentCreature);
                    RearrangeBottomBoard();

                    _playerController.PlayerInfo.AddCardToHand(_playerController.currentCreature.Card);
                    _playerController.PlayerInfo.RemoveCardFromBoard(_playerController.currentCreature.Card);

                    MonoBehaviour.Destroy(_playerController.currentCreature.gameObject);
                    _playerController.currentCreature = null;
                }

                if (_playerController.currentSpellCard != null)
                {
                    MonoBehaviour.Destroy(_playerController.currentSpellCard);
                    _playerController.currentSpellCard = null;
                    RearrangeHand();
                }
            }
            else
            {
                foreach (var card in opponentBoardCards)
                    card.OnEndTurn();
            }


            foreach (var player in players)
                player.CallOnEndTurnEvent();

            //todo move it from here !!!!!!!!!!!!!! 
            _gameplayManager.WhoseTurn = _gameplayManager.IsLocalPlayerTurn() ? _gameplayManager.GetOpponentPlayer() : _gameplayManager.GetLocalPlayer();

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.END_TURN);

            OnTurnEndeddEvent?.Invoke();
        }

        public void StopTurn()
        {
            if (_turnCoroutine != null)
                MainApp.Instance.StopCoroutine(_turnCoroutine);

            EndTurn();

            _turnCoroutine = MainApp.Instance.StartCoroutine(RunTurn());
        }

        public void RemovePlayerCardFromBoardToGraveyard(WorkingCard card)
        {
            var graveyardPos = GameObject.Find("GraveyardPlayer").transform.position + new Vector3(0.0f, -0.2f, 0.0f);
            var boardCard = playerBoardCards.Find(x => x.Card == card);
            if (boardCard != null)
            {

                boardCard.transform.localPosition = new Vector3(boardCard.transform.localPosition.x, boardCard.transform.localPosition.y, -0.2f);

                playerBoardCards.Remove(boardCard);
                playerGraveyardCards.Add(boardCard);

                boardCard.SetHighlightingEnabled(false);
                boardCard.StopSleepingParticles();
                boardCard.gameObject.GetComponent<SortingGroup>().sortingLayerName = "BoardCards";

                MonoBehaviour.Destroy(boardCard.gameObject.GetComponent<BoxCollider2D>());
            }
            else if (_playerController.currentSpellCard != null && card == _playerController.currentSpellCard.WorkingCard)
            {
                _playerController.currentSpellCard.SetHighlightingEnabled(false);
                _playerController.currentSpellCard.GetComponent<SortingGroup>().sortingLayerName = "BoardCards";

                MonoBehaviour.Destroy(_playerController.currentSpellCard.GetComponent<BoxCollider2D>());

                _playerController.currentSpellCard.GetComponent<HandCard>().enabled = false;
                _playerController.currentSpellCard = null;
            }
        }

        public void RemoveOpponentCardFromBoardToGraveyard(WorkingCard card)
        {
            var graveyardPos = GameObject.Find("GraveyardOpponent").transform.position + new Vector3(0.0f, -0.2f, 0.0f);
            var boardCard = opponentBoardCards.Find(x => x.Card == card);
            if (boardCard != null)
            {

                boardCard.transform.localPosition = new Vector3(boardCard.transform.localPosition.x, boardCard.transform.localPosition.y, -0.2f);

                opponentBoardCards.Remove(boardCard);
                opponentGraveyardCards.Add(boardCard);
               

                boardCard.SetHighlightingEnabled(false);
                boardCard.StopSleepingParticles();
                boardCard.gameObject.GetComponent<SortingGroup>().sortingLayerName = "BoardCards";
                MonoBehaviour.Destroy(boardCard.gameObject.GetComponent<BoxCollider2D>());
            }
            else if (_aiController.currentSpellCard != null && card == _aiController.currentSpellCard.WorkingCard)
            {
                _aiController.currentSpellCard.SetHighlightingEnabled(false);
                _aiController.currentSpellCard.GetComponent<SortingGroup>().sortingLayerName = "BoardCards";
                MonoBehaviour.Destroy(_aiController.currentSpellCard.GetComponent<BoxCollider2D>());
                var sequence = DOTween.Sequence();
                sequence.PrependInterval(2.0f);
                sequence.Append(_aiController.currentSpellCard.transform.DOMove(graveyardPos, 0.5f));
                sequence.Append(_aiController.currentSpellCard.transform.DOScale(new Vector2(0.6f, 0.6f), 0.5f));
                sequence.OnComplete(() =>
                {
                    _aiController.currentSpellCard = null;
                });
            }
        }

        public void RearrangeBottomBoard(Action onComplete = null)
        {
            if (_rearrangingBottomBoard)
            {
                _timerManager.AddTimer((x) =>
                {
                    RearrangeBottomBoard(onComplete);
                }, null, 1f, false);
                return;
            }


            var playerBoardCards = _gameplayManager.GetLocalPlayer().BoardCards;

            _rearrangingBottomBoard = true;

            var boardWidth = 0.0f;
            var spacing = 0.2f; // -0.2
            var cardWidth = 0.0f;
            foreach (var card in playerBoardCards)
            {
                cardWidth = card.transform.GetComponent<SpriteRenderer>().bounds.size.x;
                boardWidth += cardWidth;
                boardWidth += spacing;
            }
            boardWidth -= spacing;

            var newPositions = new List<Vector2>(playerBoardCards.Count);
            var pivot = GameObject.Find("PlayerBoard").transform.position;

            for (var i = 0; i < playerBoardCards.Count; i++)
            {
                var card = playerBoardCards[i];
                newPositions.Add(new Vector2(pivot.x - boardWidth / 2 + cardWidth / 2, pivot.y - 1.7f));
                pivot.x += boardWidth / playerBoardCards.Count;
            }

            var sequence = DOTween.Sequence();
            for (var i = 0; i < playerBoardCards.Count; i++)
            {
                var card = playerBoardCards[i];
                sequence.Insert(0, card.transform.DOMove(newPositions[i], 0.4f).SetEase(Ease.OutSine));
            }
            sequence.OnComplete(() =>
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            });

            _timerManager.AddTimer((x) =>
            {
                _rearrangingBottomBoard = false;
            }, null, 1.5f, false);
        }


        public void RearrangeTopBoard(Action onComplete = null)
        {
            if (_rearrangingTopBoard)
            {
                _timerManager.AddTimer((x) =>
                {
                    RearrangeTopBoard(onComplete);
                }, null, 1f, false);

                return;
            }

            var opponentBoardCards = _gameplayManager.GetOpponentPlayer().BoardCards;


            _rearrangingTopBoard = true;

            var boardWidth = 0.0f;
            var spacing = 0.2f;
            var cardWidth = 0.0f;
            foreach (var card in opponentBoardCards)
            {
                // warning!
                if (card == null)
                    continue;

                cardWidth = card.transform.GetComponent<SpriteRenderer>().bounds.size.x;
                boardWidth += cardWidth;
                boardWidth += spacing;
            }
            boardWidth -= spacing;

            var newPositions = new List<Vector2>(opponentBoardCards.Count);
            var pivot = GameObject.Find("OpponentBoard").transform.position;

            for (var i = 0; i < opponentBoardCards.Count; i++)
            {
                var card = opponentBoardCards[i];
                newPositions.Add(new Vector2(pivot.x - boardWidth / 2 + cardWidth / 2, pivot.y + 0.0f));
                pivot.x += boardWidth / opponentBoardCards.Count;
            }

            var sequence = DOTween.Sequence();
            for (var i = 0; i < opponentBoardCards.Count; i++)
            {
                var card = opponentBoardCards[i];
                sequence.Insert(0, card.transform.DOMove(newPositions[i], 0.4f).SetEase(Ease.OutSine));
            }
            sequence.OnComplete(() =>
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            });

            _timerManager.AddTimer((x) =>
            {
                _rearrangingTopBoard = false;
            }, null, 1.5f, false);
        }


        public void CreateCardPreview(WorkingCard card, Vector3 pos, bool highlight = true)
        {
            isPreviewActive = true;
            currentPreviewedCardId = card.instanceId;
            createPreviewCoroutine = MainApp.Instance.StartCoroutine(CreateCardPreviewAsync(card, pos, highlight));
        }

        public IEnumerator CreateCardPreviewAsync(WorkingCard card, Vector3 pos, bool highlight)
        {
            yield return new WaitForSeconds(0.3f);

            string cardSetName = string.Empty;
            foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(card.libraryCard) > -1)
                    cardSetName = cardSet.name;
            }

            if (card.libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                currentCardPreview = MonoBehaviour.Instantiate(_cardsController.creatureCardViewPrefab);
            }
            else if (card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                currentCardPreview = MonoBehaviour.Instantiate(_cardsController.spellCardViewPrefab);
            }

            var cardView = currentCardPreview.GetComponent<CardView>();
            cardView.PopulateWithInfo(card, cardSetName);
            if (highlight)
                highlight = cardView.CanBePlayed(card.owner) && cardView.CanBeBuyed(card.owner);
            cardView.SetHighlightingEnabled(highlight);
            cardView.isPreview = true;

            var newPos = pos;
            newPos.y += 2.0f;
            currentCardPreview.transform.position = newPos;
            currentCardPreview.transform.localRotation = Quaternion.Euler(Vector3.zero);
            currentCardPreview.transform.localScale = new Vector2(.4f, .4f);
            currentCardPreview.GetComponent<SortingGroup>().sortingOrder = 1000;
            currentCardPreview.layer = LayerMask.NameToLayer("Ignore Raycast");
            currentCardPreview.transform.DOMoveY(newPos.y + 1.0f, 0.1f);
        }

        public void DestroyCardPreview()
        {
            MainApp.Instance.StartCoroutine(DestroyCardPreviewAsync());
            if (createPreviewCoroutine != null)
            {
                MainApp.Instance.StopCoroutine(createPreviewCoroutine);
            }
            isPreviewActive = false;
        }

        public IEnumerator DestroyCardPreviewAsync()
        {
            if (currentCardPreview != null)
            {
                var oldCardPreview = currentCardPreview;
                foreach (var renderer in oldCardPreview.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.DOFade(0.0f, 0.2f);
                }
                foreach (var text in oldCardPreview.GetComponentsInChildren<TextMeshPro>())
                {
                    text.DOFade(0.0f, 0.2f);
                }
                yield return new WaitForSeconds(0.5f);
                MonoBehaviour.Destroy(oldCardPreview.gameObject);
            }
        }

        public void RearrangeHand(bool isMove = false)
        {
            var handWidth = 0.0f;
            var spacing = -1.5f; // -1
            foreach (var card in playerHandCards)
            {
                handWidth += spacing;
            }
            handWidth -= spacing;

            var pivot = new Vector3(6f, -7.5f, 0f); //1.115f, -8.05f, 0f
            var twistPerCard = -5;

            if (playerHandCards.Count == 1)
            {
                twistPerCard = 0;
            }

            var totalTwist = twistPerCard * playerHandCards.Count;
            float startTwist = ((totalTwist - twistPerCard) / 2f);
            var scalingFactor = 0.04f;
            Vector3 moveToPosition = Vector3.zero;
            for (var i = 0; i < playerHandCards.Count; i++)
            {
                var card = playerHandCards[i];
                var twist = startTwist - (i * twistPerCard);
                var nudge = Mathf.Abs(twist);
                nudge *= scalingFactor;
                moveToPosition = new Vector3(pivot.x - handWidth / 2, pivot.y - nudge, (playerHandCards.Count - i) * 0.1f);

                if (isMove)
                    card.isNewCard = false;

                card.RearrangeHand(moveToPosition, Vector3.forward * twist);

                pivot.x += handWidth / playerHandCards.Count;
                card.GetComponent<SortingGroup>().sortingLayerName = "HandCards";
                card.GetComponent<SortingGroup>().sortingOrder = i;
            }
        }

        public void RearrangeOpponentHand(bool isMove = false, bool isNewCard = false)
        {
            var handWidth = 0.0f;
            var spacing = -1.0f;
            foreach (var card in opponentHandCards)
            {
                handWidth += spacing;
            }
            handWidth -= spacing;

            var pivot = new Vector3(-3.2f, 8.5f, 0f);
            var twistPerCard = 5;

            if (opponentHandCards.Count == 1)
            {
                twistPerCard = 0;
            }

            var totalTwist = twistPerCard * opponentHandCards.Count;
            float startTwist = ((totalTwist - twistPerCard) / 2f);
            var scalingFactor = 0.04f;
            Vector3 movePosition = Vector3.zero;
            Vector3 rotatePosition = Vector3.zero;
            for (var i = 0; i < opponentHandCards.Count; i++)
            {
                var card = opponentHandCards[i];
                var twist = startTwist - (i * twistPerCard);
                var nudge = Mathf.Abs(twist);
                nudge *= scalingFactor;
                movePosition = new Vector2(pivot.x - handWidth / 2, pivot.y);
                rotatePosition = new Vector3(0, 0, twist); // added multiplier, was: 0,0, twist

                if (isMove)
                {
                    if (i == opponentHandCards.Count - 1 && isNewCard)
                    {
                        card.transform.position = new Vector3(-8.2f, 5.7f, 0); // OPPONENT DECK START POINT
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
                pivot.x += handWidth / opponentHandCards.Count;
                card.GetComponent<SortingGroup>().sortingOrder = i;
            }
        }

        public BoardCreature GetBoardCreatureFromHisObject(GameObject creatureObject)
        {
           var creature = playerBoardCards.Find(x => x.gameObject.Equals(creatureObject));

            if(creature == null)
                creature = opponentBoardCards.Find(x => x.gameObject.Equals(creatureObject));


            return creature;
        }
    }
}