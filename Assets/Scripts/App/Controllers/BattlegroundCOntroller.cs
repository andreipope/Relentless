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
        private IGameplayManager _gameplayManager;
        private ITimerManager _timerManager;
        private ISoundManager _soundManager;

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

        public GameObject creatureCardViewPrefab,
                          spellCardViewPrefab;


        public List<CardView> playerHandCards = new List<CardView>();
        public List<GameObject> opponentHandCards = new List<GameObject>();

        public List<BoardCreature> playerBoardCards = new List<BoardCreature>();
        public List<BoardCreature> opponentBoardCards = new List<BoardCreature>();

        public List<BoardCreature> playerGraveyardCards = new List<BoardCreature>();
        public List<BoardCreature> opponentGraveyardCards = new List<BoardCreature>();


        public BattlegroundController()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _soundManager = GameClient.Get<ISoundManager>();

            LoadGameConfiguration();
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        private void LoadGameConfiguration()
        {
            TurnDuration = Constants.DEFAULT_TURN_DURATION;

            if (_gameplayManager.IsTutorial)
                TurnDuration = 100000;
        }

        private void CheckGameDynamic()
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


        public void StartGame()
        {
            Debug.Log("Game has started.");

            // Start with turn 1.
            currentTurn = 1;

            var players = _gameplayManager.PlayersInGame;

            // Create an array with all the player nicknames.
            var playerNicknames = new List<string>(players.Count);
            foreach (var player in players)
                playerNicknames.Add(player.nickname);

            if (!_gameplayManager.IsTutorial)
            {
                //// Execute the game start actions.
                //foreach (var action in GameManager.Instance.config.properties.gameStartActions)
                //{
                //    ExecuteGameAction(action);
                //}
            }
            else
                players.Find(x => !x.IsLocalPlayer).HP = 8;

            if (Constants.DEV_MODE)
                players.Find(x => !x.IsLocalPlayer).HP = 100;


            _turnCoroutine = MainApp.Instance.StartCoroutine(RunTurn());
        }

        public void EndGame(Player player, Enumerators.EndGameType type)
        {
            if (_gameplayManager.IsTutorial)
                return;

            gameFinished = true;

            switch (type)
            {
                case Enumerators.EndGameType.WIN:       
                    break;

                case Enumerators.EndGameType.LOSE:
                    break;
            }
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

        protected virtual void StartTurn()
        {
            var players = _gameplayManager.PlayersInGame;


            _gameplayManager.PlayersInGame.Find(x => x.IsLocalPlayer).turn++;

            //// Execute the turn start actions.
            //foreach (var action in GameManager.Instance.config.properties.turnStartActions)
            //{
            //    ExecuteGameAction(action);
            //}

        }

        protected virtual void EndTurn()
        {
            var players = _gameplayManager.PlayersInGame;

            // Switch to next player.
            currentPlayerIndex += 1;
            if (currentPlayerIndex == players.Count)
            {
                currentPlayerIndex = 0;
                // Increase turn count.
                currentTurn += 1;
            }
        }

        public void StopTurn()
        {
            if (_turnCoroutine != null)
                MainApp.Instance.StopCoroutine(_turnCoroutine);

            EndTurn();

            _turnCoroutine = MainApp.Instance.StartCoroutine(RunTurn());
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
                currentCardPreview = MonoBehaviour.Instantiate(creatureCardViewPrefab);
            }
            else if (card.libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                currentCardPreview = MonoBehaviour.Instantiate(spellCardViewPrefab);
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
    }
}