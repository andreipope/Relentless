// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Gameplay;
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
        private ITimerManager _timerManager;

        private AbilitiesController _abilitiesController;
        private CardsController _cardsController;
        private BattlegroundController _battlegroundController;
        private SkillsController _skillsController;
        private BoardArrowController _boardArrowController;

        private bool _handCardPreviewTimerStarted;

        private bool _startedOnClickDelay = false;
        private bool _isPreviewHandCard = false;
        private float _delayTimerOfClick = 0f;
        private bool _cardsZooming = false;

        private BoardCard _topmostBoardCard;
        private BoardUnit _selectedBoardUnit;

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
            _timerManager = GameClient.Get<ITimerManager>();

            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();
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

        public void ResetAll()
        {
            StopHandTimer();
            _timerManager.StopTimer(SetStatusZoomingFalse);
        }

        public void InitializePlayer()
        {
            _gameplayManager.CurrentPlayer = new Player(GameObject.Find("Player"), false);

            var playerDeck = new List<string>();

            if (_gameplayManager.IsTutorial)
            {
                playerDeck.Add("GooZilla");
                playerDeck.Add("Burrrnn");
                playerDeck.Add("Burrrnn");
                playerDeck.Add("Burrrnn");
                playerDeck.Add("Azuraz");
            }
            else
            {
                var deckId = _gameplayManager.PlayerDeckId;
                foreach (var card in _dataManager.CachedDecksData.decks.First(d => d.id == deckId).cards)
                {
                    for (var i = 0; i < card.amount; i++)
                    {
                        if (Constants.DEV_MODE)
                        {
                           //    playerDeck.Add("Zhatterer");
                        }

                        playerDeck.Add(card.cardName);
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


        // OPTIMIZE THIS CODE(shit) --- comment by AS
        private void HandleInput()
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                // if (!IsCardSelected)
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
                                _startedOnClickDelay = true;
                                _isPreviewHandCard = true;
                                _topmostBoardCard = topmostBoardCard;
                            }
                        }
                    }
                    else if (hitBoardCard)
                    {
                        if (hitCards.Count > 0)
                        {
                            StopHandTimer();

                            hitCards = hitCards.OrderBy(x => x.GetComponent<SortingGroup>().sortingOrder).ToList();
                            var selectedBoardUnit = _battlegroundController.GetBoardUnitFromHisObject(hitCards[hitCards.Count - 1]);
                            if (selectedBoardUnit != null && (!_battlegroundController.isPreviewActive || selectedBoardUnit.Card.instanceId != _battlegroundController.currentPreviewedCardId))
                            {
                                _startedOnClickDelay = true;
                                _isPreviewHandCard = false;
                                _selectedBoardUnit = selectedBoardUnit;
                            }
                        }
                    }
                    else
                    {
                        StopHandTimer();
                        _battlegroundController.DestroyCardPreview();

                        _delayTimerOfClick = 0f;
                        _startedOnClickDelay = false;
                        _topmostBoardCard = null;
                        _selectedBoardUnit = null;
                    }
                }

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
                        else if (!_tutorialManager.IsTutorial)
                        {
                            _timerManager.StopTimer(SetStatusZoomingFalse);
                            _cardsZooming = true;
                            _timerManager.AddTimer(SetStatusZoomingFalse, null, 2f);

                            _battlegroundController.cardsZoomed = true;
                            _battlegroundController.UpdatePositionOfCardsInPlayerHand();
                        }
                    }
                    else
                    {
                        _timerManager.StopTimer(SetStatusZoomingFalse);
                        _cardsZooming = true;
                        _timerManager.AddTimer(SetStatusZoomingFalse, null, 2f);

                        _battlegroundController.cardsZoomed = false;
                        _battlegroundController.UpdatePositionOfCardsInPlayerHand();
                    }
                }
            }

            if (_startedOnClickDelay)
            {
                _delayTimerOfClick += Time.deltaTime;

                if (_delayTimerOfClick > Constants.TOOLTIP_APPEAR_ON_CLICK_DELAY)
                {

                    _delayTimerOfClick = 0f;
                    _startedOnClickDelay = false;
                    // _topmostBoardCard = null;
                    // _selectedBoardUnit = null;

                    //if (_selectedBoardUnit != null && !_selectedBoardUnit.IsAttacking)
                    //{
                    //    StopHandTimer();
                    //    _battlegroundController.DestroyCardPreview();

                    //    if (_boardArrowController.IsBoardArrowNowInTheBattle &&  (_boardArrowController.CurrentBoardArrow != null && !_boardArrowController.CurrentBoardArrow.IsDragging()))
                    //        HandCardPreview(new object[] { _selectedBoardUnit });
                    //}
                }
                else
                if (Input.GetMouseButtonUp(0) && _delayTimerOfClick <= Constants.TOOLTIP_APPEAR_ON_CLICK_DELAY)
                {
                    if (_isPreviewHandCard)
                    {
                        if (_topmostBoardCard != null && _battlegroundController.cardsZoomed && !_cardsZooming)
                        {
                            StopHandTimer();
                            _battlegroundController.DestroyCardPreview();


                            if (!_boardArrowController.IsBoardArrowNowInTheBattle)
                                HandCardPreview(new object[] { _topmostBoardCard });
                        }
                    }
                    else
                    {
                        if (_selectedBoardUnit != null && !_selectedBoardUnit.IsAttacking)
                        {
                            StopHandTimer();
                            _battlegroundController.DestroyCardPreview();

                            if (_boardArrowController.CurrentBoardArrow == null || (_boardArrowController.CurrentBoardArrow != null && !(_boardArrowController.CurrentBoardArrow is AbilityBoardArrow)))
                                HandCardPreview(new object[] { _selectedBoardUnit });
                        }
                    }

                    _delayTimerOfClick = 0f;
                    _startedOnClickDelay = false;
                    // _topmostBoardCard = null;
                    // _selectedBoardUnit = null;
                }
            }

            if(_boardArrowController.CurrentBoardArrow != null && _boardArrowController.CurrentBoardArrow.IsDragging())
            {
                _battlegroundController.DestroyCardPreview();
            }
        }


        public void HideCardPreview()
        {
            if (!_startedOnClickDelay)
                return;

            StopHandTimer();
            _battlegroundController.DestroyCardPreview();

            _delayTimerOfClick = 0f;
            _startedOnClickDelay = false;
            _topmostBoardCard = null;
            _selectedBoardUnit = null;
        }

        public void HandCardPreview(object[] param)
        {
            _battlegroundController.CreateCardPreview(param[0], new Vector3(-6f, -2.5f, 0.1f), false);
        }

        private void StopHandTimer()
        {
          //  if (_handCardPreviewTimerStarted)
            {
                GameClient.Get<ITimerManager>().StopTimer(HandCardPreview);
                _handCardPreviewTimerStarted = false;
            }
        }

        private void SetStatusZoomingFalse(object[] param)
        {
            _cardsZooming = false;
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
            //    if (PlayerInfo.Goo >= PlayerInfo.BoardSkills[0].manaCost)
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