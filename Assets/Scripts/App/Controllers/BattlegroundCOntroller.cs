using GrandDevs.CZB.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class BattlegrdController : IController
    {
        private IGameplayManager _gameplayManager;
        private ITimerManager _timerManager;

        private Coroutine _turnCoroutine;

        public int TurnDuration { get; private set; }
        public int currentPlayerIndex;
        public int currentTurn;
        public bool gameFinished;





        public BattlegrdController()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();

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

        public virtual void StartGame()
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

        public virtual void EndGame(Player player, Enumerators.EndGameType type)
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

        public virtual void StopTurn()
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

            _rearrangingBottomBoard = true;

            var boardWidth = 0.0f;
            var spacing = 0.2f; // -0.2
            var cardWidth = 0.0f;
            foreach (var card in playerBoardCards)
            {
                cardWidth = card.GetComponent<SpriteRenderer>().bounds.size.x;
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


        public void RearrangeTopBoard()
        {
            if (_rearrangingTopBoard)
            {
                _timerManager.AddTimer((x) =>
                {
                    RearrangeTopBoard(onComplete);
                }, null, 1f, false);

                return;
            }

            _rearrangingTopBoard = true;

            var boardWidth = 0.0f;
            var spacing = 0.2f;
            var cardWidth = 0.0f;
            foreach (var card in opponentBoardCards)
            {
                // warning!
                if (card == null && !card)
                    continue;

                cardWidth = card.GetComponent<SpriteRenderer>().bounds.size.x;
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


        public void CreateCardPreview(RuntimeCard card, Vector3 pos, bool highlight = true)
        {
            isPreviewActive = true;
            currentPreviewedCardId = card.instanceId;
            createPreviewCoroutine = StartCoroutine(CreateCardPreviewAsync(card, pos, highlight));
        }

        public IEnumerator CreateCardPreviewAsync(RuntimeCard card, Vector3 pos, bool highlight)
        {
            yield return new WaitForSeconds(0.3f);

            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(card.cardId);

            string cardSetName = string.Empty;
            foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(libraryCard) > -1)
                    cardSetName = cardSet.name;
            }

            if ((Enumerators.CardKind)libraryCard.cardKind == Enumerators.CardKind.CREATURE)
            {
                currentCardPreview = MonoBehaviour.Instantiate(creatureCardViewPrefab as GameObject);
            }
            else if ((Enumerators.CardKind)libraryCard.cardKind == Enumerators.CardKind.SPELL)
            {
                currentCardPreview = MonoBehaviour.Instantiate(spellCardViewPrefab as GameObject);
            }

            var cardView = currentCardPreview.GetComponent<CardView>();
            cardView.PopulateWithInfo(card, cardSetName);
            if (highlight)
                highlight = cardView.CanBePlayed(this) && cardView.CanBeBuyed(this);
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
            StartCoroutine(DestroyCardPreviewAsync());
            if (createPreviewCoroutine != null)
            {
                StopCoroutine(createPreviewCoroutine);
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
                Destroy(oldCardPreview.gameObject);
            }
        }
    }
}