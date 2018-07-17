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
        private SkillsController _skillsController;

        private bool _handCardPreviewTimerStarted;

      //  private HeroController _heroController;

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
            _skillsController = _gameplayManager.GetController<SkillsController>();
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
                playerDeck.Add( _dataManager.CachedCardsLibraryData.GetCardIdFromName("Vindrom") );
                playerDeck.Add( _dataManager.CachedCardsLibraryData.GetCardIdFromName("Burrrnn") );
                playerDeck.Add( _dataManager.CachedCardsLibraryData.GetCardIdFromName("Burrrnn") );
                playerDeck.Add( _dataManager.CachedCardsLibraryData.GetCardIdFromName("Burrrnn") );
                playerDeck.Add( _dataManager.CachedCardsLibraryData.GetCardIdFromName("Azuraz") ); 
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

                       // playerDeck.Add(_dataManager.CachedCardsLibraryData.GetCardIdFromName("Fire-Maw"));
                    }
                }
            }

            _gameplayManager.CurrentPlayer.SetDeck(playerDeck);

            _gameplayManager.CurrentPlayer.OnStartTurnEvent += OnTurnStartedEventHandler;
            _gameplayManager.CurrentPlayer.OnEndTurnEvent += OnTurnEndedEventHandler;
        }


        public void SetHand()
        {
            //if (_gameplayManager.IsTutorial)
           //     _cardsController.AddCardToHand(_gameplayManager.CurrentPlayer, _gameplayManager.CurrentPlayer.CardsInDeck[0]);

            _gameplayManager.CurrentPlayer.SetFirstHand(_gameplayManager.IsTutorial);

            GameClient.Get<ITimerManager>().AddTimer((x) =>
            {
                _cardsController.UpdatePositionOfCardsForDistribution(_gameplayManager.CurrentPlayer);
            }, null, 1f);
           

            _battlegroundController.UpdatePositionOfCardsInPlayerHand();
        }

        public virtual void OnGameStartedEventHandler()
        {
           
        }


        public virtual void OnGameEndedEventHandler(Enumerators.EndGameType endGameType)
        {

            IsActive = false;
            IsPlayerStunned = false;
            IsCardSelected = false;

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
                        if (_battlegroundController.cardsZoomed || _tutorialManager.IsTutorial)
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
                        else if(!_tutorialManager.IsTutorial)
                        {
                            _battlegroundController.cardsZoomed = true;
                            _battlegroundController.UpdatePositionOfCardsInPlayerHand();
                        }
                    }
                    else
                    {
                        _battlegroundController.cardsZoomed = false;
                        _battlegroundController.UpdatePositionOfCardsInPlayerHand();
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
                                if (!_handCardPreviewTimerStarted)
                                {
                                    GameClient.Get<ITimerManager>().AddTimer(HandCardPreview, new object[] { topmostBoardCard }, 3f);
                                    _handCardPreviewTimerStarted = true;
                                }
                            }
                            if (_battlegroundController.isPreviewActive && _handCardPreviewTimerStarted && topmostBoardCard.WorkingCard.instanceId != _battlegroundController.currentPreviewedCardId)
                                StopHandTimer();
                        }
                    }
                }
                else if (hitBoardCard)
                {
                    if (hitCards.Count > 0)
                    {
                        StopHandTimer();

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
                    StopHandTimer();
                    _battlegroundController.DestroyCardPreview();
                }
            }

        }

        public void HandCardPreview(object[] param)
        {
            BoardCard card = param[0] as BoardCard;
            _battlegroundController.CreateCardPreview(card.WorkingCard, card.transform.position, IsActive);
        }

        private void StopHandTimer()
        {
            if (_handCardPreviewTimerStarted)
            {
                GameClient.Get<ITimerManager>().StopTimer(HandCardPreview);
                _handCardPreviewTimerStarted = false;
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
            //if (PlayerInfo.BoardSkills[0] != null && IsActive)
            //{
            //    if (PlayerInfo.Mana >= PlayerInfo.BoardSkills[0].manaCost)
            //        PlayerInfo.BoardSkills[0].SetHighlightingEnabled(true);
            //    else
            //        PlayerInfo.BoardSkills[0].SetHighlightingEnabled(false);
            //}

            if (_gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.CurrentPlayer))
            {
                foreach (var card in _battlegroundController.playerHandCards)
                {
                    if (card.CanBeBuyed(_gameplayManager.CurrentPlayer))
                        card.SetHighlightingEnabled(true);
                    else
                        card.SetHighlightingEnabled(false);
                }
            }
        }
    }
}