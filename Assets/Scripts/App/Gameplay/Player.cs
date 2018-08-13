// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Helpers;
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

        public event Action<int> PlayerHPChangedEvent;
        public event Action<int> PlayerGooChangedEvent;
        public event Action<int> PlayerVialGooChangedEvent;
        public event Action<int> DeckChangedEvent;
        public event Action<int> HandChangedEvent;
        public event Action<int> GraveyardChangedEvent;
        public event Action<int> BoardChangedEvent;
        public event Action<WorkingCard> CardPlayedEvent;

        private GameObject _playerObject;

        private GameObject _freezedHighlightObject;

        private IDataManager _dataManager;
        private IGameplayManager _gameplayManager;
        private ISoundManager _soundManager;
        private ITutorialManager _tutorialManager;

        private CardsController _cardsController;
        private BattlegroundController _battlegroundController;
        private SkillsController _skillsController;
        private AnimationsController _animationsController;

        private int _goo;
        private int _gooOnCurrentTurn;
        private int _health;
        private int _graveyardCardsCount = 0;

        private bool _isDead;

        private int _turnsLeftToFreeFromStun = 0;

        private Hero _selfHero;

        private List<WorkingCard> _cardsInDeck;
        private List<WorkingCard> _CardsInGraveyard;
        private List<WorkingCard> _cardsInHand;
        private List<WorkingCard> _cardsInBoard;

        private OnBehaviourHandler _avatarOnBehaviourHandler;

        private GameObject _avatarObject,
                           _overlordRegularObject,
                           _overlordDeathObject,
                           _avatarHeroHighlight,
                           _avatarSelectedHighlight;

        private Animator _avatarAnimator, 
                         _deathAnimamtor;

        private FadeTool _gooBarFadeTool;


        public GameObject PlayerObject { get { return _playerObject; } }
        public GameObject AvatarObject { get { return _avatarObject.transform.parent.gameObject; } }
        public Transform Transform { get { return _playerObject.transform; } }
       
        public Hero SelfHero { get { return _selfHero; } }

        public int id;
        public int deckId;

        public int turn;

        public string nickname;

        public int initialHP;

        public int currentGooModificator;

        public int GooOnCurrentTurn
        {
            get { return _gooOnCurrentTurn; }
            set
            {
                _gooOnCurrentTurn = value;
                _gooOnCurrentTurn = Mathf.Clamp(_gooOnCurrentTurn, 0, Constants.MAXIMUM_PLAYER_GOO);

                PlayerVialGooChangedEvent?.Invoke(_gooOnCurrentTurn);
            }
        }

        public int Goo
        {
            get
            {
                return _goo;
            }
            set
            {
                var oldGoo = _goo;
              // _goo = value;

                _goo = Mathf.Clamp(value, 0,999999);

                PlayerGooChangedEvent?.Invoke(_goo);
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

                PlayerHPChangedEvent?.Invoke(_health);
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

        public List<WorkingCard> CardsInDeck { get; set; }
        public List<WorkingCard> CardsInGraveyard { get; private set; }
        public List<WorkingCard> CardsInHand { get; private set; }
        public List<WorkingCard> CardsOnBoard { get; private set; }

        public List<BoardCard> CardsPreparingToHand { get; set; }

        public bool IsStunned { get; private set; }

    
        public int BuffedHP { get; set; }
        public int MaxCurrentHP { get { return initialHP + BuffedHP; } }


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
            _animationsController = _gameplayManager.GetController<AnimationsController>();

            CardsInDeck = new List<WorkingCard>();
            CardsInGraveyard = new List<WorkingCard>();
            CardsInHand = new List<WorkingCard>();
            CardsOnBoard = new List<WorkingCard>();
            BoardCards = new List<BoardUnit>();

            CardsPreparingToHand = new List<BoardCard>();

			Deck _currentDeck = null;

			if (!_gameplayManager.IsTutorial) {
				_currentDeck = _dataManager.CachedDecksData.decks[_gameplayManager.PlayerDeckId];
			} else {
				_currentDeck = new Deck();
				_currentDeck.heroId = 4;
			}

            int heroId = 0;

            if (!isOpponent)
				heroId = _currentDeck.heroId;
            else
                heroId = _dataManager.CachedOpponentDecksData.decks[_gameplayManager.OpponentDeckId].heroId;

            _selfHero = _dataManager.CachedHeroesData.Heroes[heroId];

            nickname = _selfHero.FullName;
            deckId = _gameplayManager.PlayerDeckId;

            _health = Constants.DEFAULT_PLAYER_HP;
            initialHP = _health;
            BuffedHP = 0;
            _goo = Constants.DEFAULT_PLAYER_GOO;

            _avatarOnBehaviourHandler = playerObject.transform.Find("Avatar").GetComponent<OnBehaviourHandler>();

            _avatarObject = playerObject.transform.Find("Avatar/Hero_Object").gameObject;
            _overlordRegularObject = playerObject.transform.Find("OverlordArea/RegularModel").gameObject;
            _overlordDeathObject = playerObject.transform.Find("OverlordArea/OverlordDeath").gameObject;
            _avatarHeroHighlight = playerObject.transform.Find("Avatar/HeroHighlight").gameObject;
            _avatarSelectedHighlight = playerObject.transform.Find("Avatar/SelectedHighlight").gameObject;

            _avatarAnimator = playerObject.transform.Find("Avatar/Hero_Object").GetComponent<Animator>();
            _deathAnimamtor = _overlordDeathObject.GetComponent<Animator>();
            _gooBarFadeTool = playerObject.transform.Find("Avatar/Hero_Object").GetComponent<FadeTool>();

            _freezedHighlightObject = playerObject.transform.Find("Avatar/FreezedHighlight").gameObject; 

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
            if (Goo > GooOnCurrentTurn)
                Goo = GooOnCurrentTurn;
        }

        public void CallOnStartTurnEvent()
        {
            OnStartTurnEvent?.Invoke();

           if (_gameplayManager.CurrentTurnPlayer.Equals(this))
            {
                GooOnCurrentTurn++;
                Goo = GooOnCurrentTurn + currentGooModificator;
                currentGooModificator = 0;

                if (_turnsLeftToFreeFromStun > 0 && IsStunned)
                {
                    _turnsLeftToFreeFromStun--;

                    if (_turnsLeftToFreeFromStun <= 0)
                    {
                        IsStunned = false;

                        _freezedHighlightObject.SetActive(false);
                    }
                }

                if (/*((turn != 1 && IsLocalPlayer) || !IsLocalPlayer) && */CardsInDeck.Count > 0)
                {
                    _cardsController.AddCardToHand(this, CardsInDeck[0]);
                }

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

        public GameObject AddCardToHand(WorkingCard card, bool silent = false)
        {
            GameObject cardObject = null;
            CardsInHand.Add(card);

            if (IsLocalPlayer)
            {
                cardObject = _cardsController.AddCardToHand(card, silent);
                _battlegroundController.UpdatePositionOfCardsInPlayerHand(silent);
            }
            else
            {
                cardObject = _cardsController.AddCardToOpponentHand(card, silent);

                _battlegroundController.UpdatePositionOfCardsInOpponentHand(true, !silent);
            }

            HandChangedEvent?.Invoke(CardsInHand.Count);

            return cardObject;
        }

        public void AddCardToHandFromOpponentDeck(Player opponent, WorkingCard card)
        {
            card.owner = this;

            CardsInHand.Add(card);

            if(IsLocalPlayer)
                _animationsController.MoveCardFromPlayerDeckToPlayerHandAnimation(opponent, this, _cardsController.GetBoardCard(card));
            else
                _animationsController.MoveCardFromPlayerDeckToOpponentHandAnimation(opponent, this, _cardsController.GetOpponentBoardCard(card));

            HandChangedEvent?.Invoke(CardsInHand.Count);
        }

        public void RemoveCardFromHand(WorkingCard card, bool silent = false)
        {
            CardsInHand.Remove(card);

            if (IsLocalPlayer)
            {
                if (!silent)
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

        public void SetDeck(List<string> cards)
        {
            CardsInDeck = new List<WorkingCard>();

            cards = ShuffleCardsList(cards);

            foreach (var card in cards)
            {
#if UNITY_EDITOR
                if (IsLocalPlayer && Constants.DEV_MODE)
                    CardsInDeck.Add(new WorkingCard(_dataManager.CachedCardsLibraryData.GetCardFromName(card /* 15 */), this)); // special card id
                else
#endif
                    CardsInDeck.Add(new WorkingCard(_dataManager.CachedCardsLibraryData.GetCardFromName(card), this));
            }

            DeckChangedEvent?.Invoke(CardsInDeck.Count);
        }

        public List<T> ShuffleCardsList<T>(List<T> cards)
        {
            var array = cards;

            if (!_gameplayManager.IsTutorial)
                InternalTools.ShakeList<T>(ref array);// shake

            return array;
        }

        public void SetFirstHand(bool isTutorial = false)
        {
            if (isTutorial)
                return;
            for (int i = 0; i < Constants.DEFAULT_CARDS_IN_HAND_AT_START_GAME; i++)
            {
                if (IsLocalPlayer && !_gameplayManager.IsTutorial)
                    _cardsController.AddCardToDistributionState(this, CardsInDeck[i]);
                else
                    _cardsController.AddCardToHand(this, CardsInDeck[0]);
            }
        }

        public void DistributeCard()
        {
            if (IsLocalPlayer)
                _cardsController.AddCardToDistributionState(this, CardsInDeck[UnityEngine.Random.Range(0, CardsInDeck.Count)]);
            else
                _cardsController.AddCardToHand(this, CardsInDeck[UnityEngine.Random.Range(0, CardsInDeck.Count)]);
        }

        public void PlayerDie()
        {
            _gooBarFadeTool.FadeIn();

            _overlordRegularObject.SetActive(false);
            _overlordDeathObject.SetActive(true);

            _avatarAnimator.enabled = true;
            _deathAnimamtor.enabled = true;
            _avatarHeroHighlight.SetActive(false);
            _avatarAnimator.Play(0);
            _deathAnimamtor.Play(0);

            _skillsController.DisableSkillsContent(this);

            _soundManager.PlaySound(Enumerators.SoundType.HERO_DEATH, Constants.HERO_DEATH_SOUND_VOLUME, false, false);

            if(!_gameplayManager.IsTutorial)
                _gameplayManager.EndGame(IsLocalPlayer ? Enumerators.EndGameType.LOSE : Enumerators.EndGameType.WIN);
        }

        public void SetGlowStatus(bool status)
        {
            _avatarSelectedHighlight.SetActive(status);
        }

        public void Stun(Enumerators.StunType stunType, int turnsCount)
        {
            //todo implement logic

            _freezedHighlightObject.SetActive(true);
            IsStunned = true;
            _turnsLeftToFreeFromStun = turnsCount;

            _skillsController.BlockSkill(this, Enumerators.SkillType.PRIMARY);
            _skillsController.BlockSkill(this, Enumerators.SkillType.SECONDARY);

        }

        public void ThrowPlayCardEvent(WorkingCard card)
        {
            CardPlayedEvent?.Invoke(card);
        }

        public void ThrowOnHandChanged()
        {
            HandChangedEvent?.Invoke(CardsInHand.Count);
        }

        #region handlers


        private void PlayerHPChangedEventHandler(int now)
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
                var boardArrow = collider.transform.parent.GetComponent<BoardArrow>();
                if (boardArrow != null)
                    boardArrow.OnPlayerSelected(this);
            }
        }

        private void OnTriggerExit2DEventHandler(Collider2D collider)
        {
            if (collider.transform.parent != null)
            {
                var boardArrow = collider.transform.parent.GetComponent<BoardArrow>();
                if (boardArrow != null)
                    boardArrow.OnPlayerUnselected(this);
            }
        }

        #endregion
    }
}