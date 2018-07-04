// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace LoomNetwork.CZB
{
    public class PlayerController : IController
    {
        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;
        private IPlayerManager _playerManager;
        private ITutorialManager _tutorialManager;

        private AbilitiesController _abilitiesController;
        private CardsController _cardsController;
        private BattlegroundController _battlegroundController;
      //  private HeroController _heroController;

        public bool AlreadyAttackedInThisTurn { get; set; }
        public bool IsPlayerStunned { get; set; }
        public bool IsCardSelected { get; set; }
        public bool IsActive { get; set; }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            //_heroController = _gameplayManager.GetController<HeroController>();

            _gameplayManager.OnGameStartedEvent += OnGameStartedEventHandler;
            _gameplayManager.OnGameEndedEvent += OnGameEndedEventHandler;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            if (!_gameplayManager.GameStarted || _gameplayManager.GameEnded)
                return;

            if (_tutorialManager.IsTutorial && (_tutorialManager.CurrentStep != 8 &&
                                                _tutorialManager.CurrentStep != 17 &&
                                                _tutorialManager.CurrentStep != 19 &&
                                                _tutorialManager.CurrentStep != 27))
                return;

            HandleInput();
        }

        public void InitializePlayer()
        {
            _gameplayManager.CurrentPlayer = new Player(GameObject.Find("Player"), false);

            var playerDeck = new List<int>();

            if (_gameplayManager.IsTutorial)
            {
                playerDeck.Add(21);
                playerDeck.Add(1);
                playerDeck.Add(1);
                playerDeck.Add(1);
                playerDeck.Add(18);
            }
            else
            {
                var deckId = _gameplayManager.PlayerDeckId;
                foreach (var card in _dataManager.CachedDecksData.decks[deckId].cards)
                {
                    for (var i = 0; i < card.amount; i++)
                    {
                        if (Constants.DEV_MODE)
                        {
                           // card.cardId = 51; 
                        }

                        playerDeck.Add(card.cardId);
                    }
                }
            }

            _gameplayManager.CurrentPlayer.SetDeck(playerDeck);
            _gameplayManager.CurrentPlayer.SetFirstHand();

            _battlegroundController.UpdatePositionOfCardsInPlayerHand();

            _gameplayManager.CurrentPlayer.OnStartTurnEvent += OnTurnStartedEventHandler;
            _gameplayManager.CurrentPlayer.OnEndTurnEvent += OnTurnEndedEventHandler;
        }

        public virtual void OnGameStartedEventHandler()
        {
           
        }


        public virtual void OnGameEndedEventHandler(Enumerators.EndGameType endGameType)
        {
            //Write code for analize amount of experience getted in the battle
            //_heroController.ChangeExperience(endGameType == Enumerators.EndGameType.WIN ? 100 : -50);
        }

        private void HandleInput()
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                if (IsActive)
                {
                    var hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                    var hitCards = new List<GameObject>();
                    foreach (var hit in hits)
                    {
                        if (hit.collider != null && hit.collider.gameObject != null)
                        {
                            var boardCardObject = _battlegroundController.GetBoardCardFromHisObject(hit.collider.gameObject);
                            if (boardCardObject != null && !boardCardObject.isPreview && boardCardObject.CanBePlayed(_gameplayManager.CurrentPlayer))
                            {
                                hitCards.Add(hit.collider.gameObject);
                            }
                        }
                    }
                    if (hitCards.Count > 0)
                    {
                        _battlegroundController.DestroyCardPreview();
                        hitCards = hitCards.OrderByDescending(x => x.transform.position.z).ToList();

                        var topmostBoardCard = _battlegroundController.GetBoardCardFromHisObject(hitCards[hitCards.Count - 1]);
                        var topmostHandCard = topmostBoardCard.HandBoardCard;
                        if (topmostHandCard != null)
                        {
                            topmostBoardCard.HandBoardCard.OnSelected();

                            if (_tutorialManager.IsTutorial)
                                _tutorialManager.DeactivateSelectTarget();
                        }
                    }
                }
            }
            else if (!IsCardSelected)
            {
                var hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                var hitCards = new List<GameObject>();
                var hitHandCard = false;
                var hitBoardCard = false;
                foreach (var hit in hits)
                {
                    if (hit.collider != null &&
                        hit.collider.gameObject != null &&
                        _battlegroundController.GetBoardCardFromHisObject(hit.collider.gameObject) != null)
                    {
                        hitCards.Add(hit.collider.gameObject);
                        hitHandCard = true;
                    }
                }
                if (!hitHandCard)
                {
                    foreach (var hit in hits)
                    {
                        if (hit.collider != null && hit.collider.name.Contains("BoardCreature"))
                        {
                            hitCards.Add(hit.collider.gameObject);
                            hitBoardCard = true;
                        }
                    }
                }

                if (hitHandCard)
                {
                    if (hitCards.Count > 0)
                    {
                        hitCards = hitCards.OrderBy(x => x.GetComponent<SortingGroup>().sortingOrder).ToList();

                        var topmostBoardCard = _battlegroundController.GetBoardCardFromHisObject(hitCards[hitCards.Count - 1]);
                        if (topmostBoardCard != null && !topmostBoardCard.isPreview)
                        {
                            if (!_battlegroundController.isPreviewActive || topmostBoardCard.WorkingCard.instanceId != _battlegroundController.currentPreviewedCardId)
                            {
                                _battlegroundController.DestroyCardPreview();
                                _battlegroundController.CreateCardPreview(topmostBoardCard.WorkingCard, topmostBoardCard.transform.position, IsActive);
                            }
                        }
                    }
                }
                else if (hitBoardCard)
                {
                    if (hitCards.Count > 0)
                    {
                        hitCards = hitCards.OrderBy(x => x.GetComponent<SortingGroup>().sortingOrder).ToList();
                        var selectedBoardCreature = _battlegroundController.GetBoardUnitFromHisObject(hitCards[hitCards.Count - 1]);
                        if (selectedBoardCreature != null && (!_battlegroundController.isPreviewActive || selectedBoardCreature.Card.instanceId != _battlegroundController.currentPreviewedCardId))
                        {
                            _battlegroundController.DestroyCardPreview();
                            _battlegroundController.CreateCardPreview(selectedBoardCreature.Card, selectedBoardCreature.transform.position, false);
                        }
                    }
                }
                else
                {
                    _battlegroundController.DestroyCardPreview();
                }
            }

        }

        public void OnTurnEndedEventHandler()
        {
        }

        public void OnTurnStartedEventHandler()
        {
        }

        public void UpdateHandCardsHighlight()
        {
            if (_gameplayManager.CurrentPlayer.BoardSkills[0] != null && IsActive)
            {
                if (_gameplayManager.CurrentPlayer.Mana >= _gameplayManager.CurrentPlayer.BoardSkills[0].manaCost)
                    _gameplayManager.CurrentPlayer.BoardSkills[0].SetHighlightingEnabled(true);
                else
                    _gameplayManager.CurrentPlayer.BoardSkills[0].SetHighlightingEnabled(false);
            }

            foreach (var card in _battlegroundController.playerHandCards)
            {
                if (card.CanBePlayed(_gameplayManager.CurrentPlayer) && card.CanBeBuyed(_gameplayManager.CurrentPlayer))
                {
                    card.SetHighlightingEnabled(true);
                }
                else
                {
                    card.SetHighlightingEnabled(false);
                }
            }
        }
    }
}