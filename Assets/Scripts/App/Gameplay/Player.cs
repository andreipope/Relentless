// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class Player
    {
        public event Action OnEndTurnEvent;
        public event Action OnStartTurnEvent;

        public event Action<int, int> PlayerHPChangedEvent;
        public event Action<int, int> PlayerManaChangedEvent;
        public event Action<int> DeckChangedEvent;
        public event Action<int> HandChangedEvent;
        public event Action<int> GraveyardChangedEvent;
        public event Action<int> BoardChangedEvent;

        private GameObject _playerObject;

        private IDataManager _dataManager;
        private IGameplayManager _gameplayManager;
        private ISoundManager _soundManager;
        private ITutorialManager _tutorialManager;

        private CardsController _cardsController;
        private BattlegroundController _battlegroundController;
        private SkillsController _skillsController;

        private int _mana;
        private int _manaOnCurrentTurn;
        private int _health;
        private int _graveyardCardsCount = 0;

        private bool _isDead;

        private Hero _selfHero;

        private List<WorkingCard> _cardsInDeck;
        private List<WorkingCard> _CardsInGraveyard;
        private List<WorkingCard> _cardsInHand;
        private List<WorkingCard> _cardsInBoard;

        private OnBehaviourHandler _avatarOnBehaviourHandler;

        private GameObject _avatarObject,
                           _avatarDeathObject,
                           _avatarHeroHighlight;

        private Animator _avatarAnimator, 
                         _deathAnimamtor;

        private FadeTool _manaBarFadeTool;


        public GameObject PlayerObject { get { return _playerObject; } }
        public GameObject AvatarObject { get { return _avatarObject.transform.parent.gameObject; } }
        public Transform Transform { get { return _playerObject.transform; } }
       
        public Hero SelfHero { get { return _selfHero; } }

        public int id;
        public int deckId;

        public int turn;

        public string nickname;

        public int ManaOnCurrentTurn
        {
            get { return _manaOnCurrentTurn; }
            set
            {
                _manaOnCurrentTurn = value;
                _manaOnCurrentTurn = Mathf.Clamp(_manaOnCurrentTurn, 0, Constants.MAXIMUM_PLAYER_MANA);
            }
        }

        public int Mana
        {
            get
            {
                return _mana;
            }
            set
            {
                var oldMana = _mana;
                _mana = value;

                //_mana = Mathf.Clamp(_mana, 0, Constants.MAXIMUM_PLAYER_MANA);

                PlayerManaChangedEvent?.Invoke(oldMana, _mana);
            }
        }

        public int HP
        {
            get
            {
                return _health;
            }
            set
            {
                var oldHealth = _health;
                _health = value;

                _health = Mathf.Clamp(_health, 0, 99);

                PlayerHPChangedEvent?.Invoke(oldHealth, _health);
            }
        }   

        public int GraveyardCardsCount
        {
            get { return _graveyardCardsCount; }
            set
            {
                _graveyardCardsCount = value;
                _battlegroundController.UpdateGraveyard(_graveyardCardsCount, this);
            }
        }

        public bool IsLocalPlayer { get; set; }
        public bool AlreadyAttackedInThisTurn { get; set; }

        public List<BoardUnit> BoardCards { get; set; }

        public List<WorkingCard> CardsInDeck { get; private set; }
        public List<WorkingCard> CardsInGraveyard { get; private set; }
        public List<WorkingCard> CardsInHand { get; private set; }
        public List<WorkingCard> CardsOnBoard { get; private set; }

        public Player(GameObject playerObject, bool isOpponent)
        {
            _playerObject = playerObject;
            IsLocalPlayer = !isOpponent;
            id = isOpponent ? 1 : 0;

            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();

            CardsInDeck = new List<WorkingCard>();
            CardsInGraveyard = new List<WorkingCard>();
            CardsInHand = new List<WorkingCard>();
            CardsOnBoard = new List<WorkingCard>();
            BoardCards = new List<BoardUnit>();

            int heroId = 0;

            if (!isOpponent)
                heroId = _dataManager.CachedDecksData.decks[_gameplayManager.PlayerDeckId].heroId;
            else
                heroId = _dataManager.CachedOpponentDecksData.decks[_gameplayManager.OpponentDeckId].heroId;

            _selfHero = _dataManager.CachedHeroesData.Heroes[heroId];


            nickname = _selfHero.name;
            deckId = _gameplayManager.PlayerDeckId;

            _health = Constants.DEFAULT_PLAYER_HP;
            _mana = Constants.DEFAULT_PLAYER_MANA;

            if (_gameplayManager.IsTutorial)
            {
                ManaOnCurrentTurn = 10;
                Mana = ManaOnCurrentTurn;
            }

            _avatarOnBehaviourHandler = playerObject.transform.Find("Avatar").GetComponent<OnBehaviourHandler>();

            _avatarObject = playerObject.transform.Find("Avatar/Hero_Object").gameObject;
            _avatarDeathObject = playerObject.transform.Find("HeroDeath").gameObject;
            _avatarHeroHighlight = playerObject.transform.Find("Avatar/HeroHighlight").gameObject;

            _avatarAnimator = playerObject.transform.Find("Avatar/Hero_Object").GetComponent<Animator>();
            _deathAnimamtor = playerObject.transform.Find("HeroDeath").GetComponent<Animator>();
            _manaBarFadeTool = playerObject.transform.Find("Avatar/Hero_Object").GetComponent<FadeTool>();


            _avatarAnimator.enabled = false;
            _deathAnimamtor.enabled = false;
            _deathAnimamtor.StopPlayback();

            _avatarOnBehaviourHandler.OnTriggerEnter2DEvent += OnTriggerEnter2DEventHandler;
            _avatarOnBehaviourHandler.OnTriggerExit2DEvent += OnTriggerExit2DEventHandler;

            PlayerHPChangedEvent += PlayerHPChangedEventHandler;
        }

        public void CallOnEndTurnEvent()
        {
            OnEndTurnEvent?.Invoke();
        }

        public void CallOnStartTurnEvent()
        {
            OnStartTurnEvent?.Invoke();

           if (_gameplayManager.CurrentTurnPlayer.Equals(this))
            {
                ManaOnCurrentTurn++;
                Mana = ManaOnCurrentTurn;

                if (((turn != 1 && IsLocalPlayer) || !IsLocalPlayer) && CardsInDeck.Count > 0)
                    _cardsController.AddCardToHand(this, CardsInDeck[0]);
            }
        }

        public void AddCardToDeck(WorkingCard card)
        {
            CardsInDeck.Add(card);

            DeckChangedEvent?.Invoke(CardsInDeck.Count);
        }

        public void RemoveCardFromDeck(WorkingCard card)
        {
            CardsInDeck.Remove(card);

            DeckChangedEvent?.Invoke(CardsInDeck.Count);
        }

        public void AddCardToHand(WorkingCard card)
        {
            CardsInHand.Add(card);

            if (IsLocalPlayer)
            {
                _cardsController.AddCardToHand(card);
                _battlegroundController.UpdatePositionOfCardsInPlayerHand();
            }
            else
            {
                _cardsController.AddCardToOpponentHand(card);
                _battlegroundController.UpdatePositionOfCardsInOpponentHand(true, true);
            }

            HandChangedEvent?.Invoke(CardsInHand.Count);
        }

        public void RemoveCardFromHand(WorkingCard card)
        {
            CardsInHand.Remove(card);

            if (IsLocalPlayer)
            {
                _battlegroundController.UpdatePositionOfCardsInPlayerHand();
            }
            else
            {
                //var randomIndex = UnityEngine.Random.Range(0, _battlegroundController.opponentHandCards.Count);
                //if (randomIndex < _battlegroundController.opponentHandCards.Count)
                //{
                //    var randomCard = _battlegroundController.opponentHandCards[randomIndex];
                //    _battlegroundController.opponentHandCards.Remove(randomCard);
                //    MonoBehaviour.Destroy(randomCard);
                //    _battlegroundController.RearrangeOpponentHand(true);
                //}
            }

            HandChangedEvent?.Invoke(CardsInHand.Count);
        }

        public void AddCardToBoard(WorkingCard card)
        {
            CardsOnBoard.Add(card);

            BoardChangedEvent?.Invoke(CardsOnBoard.Count);
        }

        public void RemoveCardFromBoard(WorkingCard card)
        {
            CardsOnBoard.Remove(card);

            if (IsLocalPlayer)
            {
                _battlegroundController.RemovePlayerCardFromBoardToGraveyard(card);
            }
            else
            {
                _battlegroundController.RemoveOpponentCardFromBoardToGraveyard(card);
            }

            BoardChangedEvent?.Invoke(CardsOnBoard.Count);
        }

        public void AddCardToGraveyard(WorkingCard card)
        {
            CardsInGraveyard.Add(card);

            GraveyardChangedEvent?.Invoke(CardsInGraveyard.Count);
        }

        public void RemoveCardFromGraveyard(WorkingCard card)
        {
            CardsInGraveyard.Remove(card);

            GraveyardChangedEvent?.Invoke(CardsInGraveyard.Count);
        }

        public void SetDeck(List<int> cards)
        {
            CardsInDeck = new List<WorkingCard>();

            if (!_gameplayManager.IsTutorial)
            {
                // shake
                var rnd = new System.Random();
                cards = cards.OrderBy(item => rnd.Next()).ToList();
            }

            foreach (var card in cards)
                CardsInDeck.Add(new WorkingCard(_dataManager.CachedCardsLibraryData.GetCard(card), this));

            DeckChangedEvent?.Invoke(CardsInDeck.Count);
        }

        public void SetFirstHand(bool isTutorial = false)
        {
            for (int i = 0; i < CardsInDeck.Count; i++)
            {
                if (i >= Constants.DEFAULT_CARDS_IN_HAND_AT_START_GAME || (isTutorial && !IsLocalPlayer))
                    break;

                _cardsController.AddCardToHand(this, CardsInDeck[0]);
            }
        }

        public void PlayerDie()
        {
            _manaBarFadeTool.FadeIn();

            _avatarAnimator.enabled = true;
            _deathAnimamtor.enabled = true;
            _avatarHeroHighlight.SetActive(false);
            _avatarAnimator.Play(0);
            _deathAnimamtor.Play(0);

            _soundManager.PlaySound(Enumerators.SoundType.HERO_DEATH, Constants.HERO_DEATH_SOUND_VOLUME, false, false);


            _gameplayManager.EndGame(IsLocalPlayer ? Enumerators.EndGameType.LOSE : Enumerators.EndGameType.WIN);
        }
  
        #region handlers


        private void PlayerHPChangedEventHandler(int was, int now)
        {
            if (now <= 0 && !_isDead)
            {
                PlayerDie();

                _isDead = true;
            }
        }

        private void OnTriggerEnter2DEventHandler(Collider2D collider)
        {
            if (collider.transform.parent != null)
            {
                var boardArrow = collider.transform.parent.parent.GetComponent<BoardArrow>();
                if (boardArrow != null)
                    boardArrow.OnPlayerSelected(this);
            }
        }

        private void OnTriggerExit2DEventHandler(Collider2D collider)
        {
            if (collider.transform.parent != null)
            {
                var boardArrow = collider.transform.parent.parent.GetComponent<BoardArrow>();
                if (boardArrow != null)
                    boardArrow.OnPlayerUnselected(this);
            }
        }

        #endregion
    }
}