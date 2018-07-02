using GrandDevs.CZB.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GrandDevs.CZB
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

        public Player PlayerInfo { get; protected set; }

        public bool AlreadyAttackedInThisTurn { get; set; }
        public bool IsPlayerStunned { get; set; }
        public bool IsCardSelected { get; set; }
        public bool IsActive { get; set; }

        public SpellCardView currentSpellCard;
        public GameObject currentBoardCreature;
        public BoardCreature currentCreature;


        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

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
            PlayerInfo = new Player(GameObject.Find("Player"), false);

            _gameplayManager.PlayersInGame.Add(PlayerInfo);

            _playerManager.PlayerInfo = PlayerInfo;

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
                            card.cardId = 51; 
                        }

                        playerDeck.Add(card.cardId);
                    }
                }
            }

            PlayerInfo.SetDeck(playerDeck);

            PlayerInfo.SetFirstHand();

            _battlegroundController.RearrangeHand();

            PlayerInfo.OnStartTurnEvent += OnTurnStartedEventHandler;
            PlayerInfo.OnEndTurnEvent += OnTurnEndedEventHandler;
        }


        public virtual void OnGameStartedEventHandler()
        {
           
        }


        public virtual void OnGameEndedEventHandler()
        {

        }

        private void HandleInput()
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                if (IsActive && currentSpellCard == null)
                {
                    var hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                    var hitCards = new List<GameObject>();
                    foreach (var hit in hits)
                    {
                        if (hit.collider != null &&
                            hit.collider.gameObject != null &&
                            hit.collider.gameObject.GetComponent<CardView>() != null &&
                            !hit.collider.gameObject.GetComponent<CardView>().isPreview &&
                            hit.collider.gameObject.GetComponent<CardView>().CanBePlayed(PlayerInfo))
                        {
                            hitCards.Add(hit.collider.gameObject);
                        }
                    }
                    if (hitCards.Count > 0)
                    {
                        _battlegroundController.DestroyCardPreview();
                        hitCards = hitCards.OrderByDescending(x => x.transform.position.z).ToList();
                        var topmostCardView = hitCards[hitCards.Count - 1].GetComponent<CardView>();
                        var topmostHandCard = topmostCardView.GetComponent<HandCard>();
                        if (topmostHandCard != null)
                        {
                            topmostCardView.GetComponent<HandCard>().OnSelected();

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
                        hit.collider.gameObject.GetComponent<CardView>() != null)
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
                        var topmostCardView = hitCards[hitCards.Count - 1].GetComponent<CardView>();
                        if (!topmostCardView.isPreview)
                        {
                            if (!_battlegroundController.isPreviewActive || topmostCardView.WorkingCard.instanceId != _battlegroundController.currentPreviewedCardId)
                            {
                                _battlegroundController.DestroyCardPreview();
                                _battlegroundController.CreateCardPreview(topmostCardView.WorkingCard, topmostCardView.transform.position, IsActive);
                            }
                        }
                    }
                }
                else if (hitBoardCard)
                {
                    if (hitCards.Count > 0)
                    {
                        hitCards = hitCards.OrderBy(x => x.GetComponent<SortingGroup>().sortingOrder).ToList();
                        var selectedBoardCreature = _battlegroundController.GetBoardCreatureFromHisObject(hitCards[hitCards.Count - 1]);
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
            if (_gameplayManager.IsLocalPlayerTurn())
            {
                if (PlayerInfo.turn != 1 && PlayerInfo.CardsInDeck.Count > 0)
                    _cardsController.AddCardToHand(PlayerInfo, PlayerInfo.CardsInDeck[0]);

                PlayerInfo.ManaOnCurrentTurn++;
                PlayerInfo.Mana = PlayerInfo.ManaOnCurrentTurn;
            }
        }

        public void UpdateHandCardsHighlight()
        {
            if (PlayerInfo.BoardSkills[0] != null && IsActive)
            {
                if (PlayerInfo.Mana >= PlayerInfo.BoardSkills[0].manaCost)
                    PlayerInfo.BoardSkills[0].SetHighlightingEnabled(true);
                else
                    PlayerInfo.BoardSkills[0].SetHighlightingEnabled(false);
            }

            foreach (var card in _battlegroundController.playerHandCards)
            {
                if (card.CanBePlayed(PlayerInfo) && card.CanBeBuyed(PlayerInfo))
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