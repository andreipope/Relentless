using System;
using System.Collections.Generic;
using System.Linq;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LoomNetwork.CZB
{
    public class Player
    {
        public int Turn;

        public int InitialHp;

        public int CurrentGooModificator;

        public int DamageByNoMoreCardsInDeck;

        private readonly GameObject _freezedHighlightObject;

        private readonly IDataManager _dataManager;

        private readonly IGameplayManager _gameplayManager;

        private readonly ISoundManager _soundManager;

        private readonly CardsController _cardsController;

        private readonly BattlegroundController _battlegroundController;

        private readonly SkillsController _skillsController;

        private readonly AnimationsController _animationsController;

        private readonly GameObject _avatarObject;

        private readonly GameObject _overlordRegularObject;

        private readonly GameObject _overlordDeathObject;

        private readonly GameObject _avatarHeroHighlight;

        private readonly GameObject _avatarSelectedHighlight;

        private readonly Animator _avatarAnimator;

        private readonly Animator _deathAnimamtor;

        private readonly FadeTool _gooBarFadeTool;

        private int _goo;

        private int _gooOnCurrentTurn;

        private int _health;

        private int _graveyardCardsCount;

        private bool _isDead;

        private int _turnsLeftToFreeFromStun;

        private List<WorkingCard> _cardsInDeck;

        private List<WorkingCard> _cardsInGraveyard;

        private List<WorkingCard> _cardsInHand;

        private List<WorkingCard> _cardsInBoard;

        public Player(GameObject playerObject, bool isOpponent)
        {
            PlayerObject = playerObject;
            IsLocalPlayer = !isOpponent;

            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();

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

            int heroId = 0;

            if (!isOpponent)
            {
                if (!_gameplayManager.IsTutorial)
                {
                    heroId = _dataManager.CachedDecksData.Decks.First(d => d.Id == _gameplayManager.PlayerDeckId).HeroId;
                }
                else
                {
                    heroId = Constants.TutorialPlayerHeroId;
                }
            }
            else
            {
                heroId = _dataManager.CachedOpponentDecksData.Decks.First(d => d.Id == _gameplayManager.OpponentDeckId).HeroId;
            }

            SelfHero = _dataManager.CachedHeroesData.HeroesParsed[heroId];

            _health = Constants.DefaultPlayerHp;
            InitialHp = _health;
            BuffedHp = 0;
            _goo = Constants.DefaultPlayerGoo;

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

            PlayerHpChanged += PlayerHpChangedHandler;

            DamageByNoMoreCardsInDeck = 0;
        }

        public event Action TurnStarted;

        public event Action TurnEnded;

        public event Action<int> PlayerHpChanged;

        public event Action<int> PlayerGooChanged;

        public event Action<int> PlayerVialGooChanged;

        public event Action<int> DeckChanged;

        public event Action<int> HandChanged;

        public event Action<int> GraveyardChanged;

        public event Action<int> BoardChanged;

        public event Action<WorkingCard> CardPlayed;

        public GameObject PlayerObject { get; }

        public GameObject AvatarObject => _avatarObject.transform.parent.gameObject;

        public Transform Transform => PlayerObject.transform;

        public Hero SelfHero { get; }

        public int GooOnCurrentTurn
        {
            get => _gooOnCurrentTurn;
            set
            {
                _gooOnCurrentTurn = value;
                _gooOnCurrentTurn = Mathf.Clamp(_gooOnCurrentTurn, 0, Constants.MaximumPlayerGoo);

                PlayerVialGooChanged?.Invoke(_gooOnCurrentTurn);
            }
        }

        public int Goo
        {
            get => _goo;
            set
            {
                _goo = Mathf.Clamp(value, 0, 999999);

                PlayerGooChanged?.Invoke(_goo);
            }
        }

        public int Health
        {
            get => _health;
            set
            {
                int oldHealth = _health;
                _health = value;

                _health = Mathf.Clamp(_health, 0, 99);

                PlayerHpChanged?.Invoke(_health);
            }
        }

        public int GraveyardCardsCount
        {
            get => _graveyardCardsCount;
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

        public List<WorkingCard> CardsInGraveyard { get; }

        public List<WorkingCard> CardsInHand { get; }

        public List<WorkingCard> CardsOnBoard { get; }

        public List<BoardCard> CardsPreparingToHand { get; set; }

        public bool IsStunned { get; private set; }

        public int BuffedHp { get; set; }

        public int MaxCurrentHp => InitialHp + BuffedHp;

        public void InvokeTurnEnded()
        {
            TurnEnded?.Invoke();
            if (Goo > GooOnCurrentTurn)
            {
                Goo = GooOnCurrentTurn;
            }
        }

        public void InvokeTurnStarted()
        {
            TurnStarted?.Invoke();

            if (_gameplayManager.CurrentTurnPlayer.Equals(this))
            {
                GooOnCurrentTurn++;
                Goo = GooOnCurrentTurn + CurrentGooModificator;
                CurrentGooModificator = 0;

                if (_turnsLeftToFreeFromStun > 0 && IsStunned)
                {
                    _turnsLeftToFreeFromStun--;

                    if (_turnsLeftToFreeFromStun <= 0)
                    {
                        IsStunned = false;

                        _freezedHighlightObject.SetActive(false);
                    }
                }

                _cardsController.AddCardToHand(this);
            }
        }

        public void AddCardToDeck(WorkingCard card)
        {
            CardsInDeck.Add(card);

            DeckChanged?.Invoke(CardsInDeck.Count);
        }

        public void RemoveCardFromDeck(WorkingCard card)
        {
            CardsInDeck.Remove(card);

            DeckChanged?.Invoke(CardsInDeck.Count);
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

            HandChanged?.Invoke(CardsInHand.Count);

            return cardObject;
        }

        public void AddCardToHandFromOpponentDeck(Player opponent, WorkingCard card)
        {
            card.Owner = this;

            CardsInHand.Add(card);

            if (IsLocalPlayer)
            {
                _animationsController.MoveCardFromPlayerDeckToPlayerHandAnimation(opponent, this, _cardsController.GetBoardCard(card));
            }
            else
            {
                _animationsController.MoveCardFromPlayerDeckToOpponentHandAnimation(opponent, this, _cardsController.GetOpponentBoardCard(card));
            }

            HandChanged?.Invoke(CardsInHand.Count);
        }

        public void RemoveCardFromHand(WorkingCard card, bool silent = false)
        {
            CardsInHand.Remove(card);

            if (IsLocalPlayer)
            {
                if (!silent)
                {
                    _battlegroundController.UpdatePositionOfCardsInPlayerHand();
                }
            }

            HandChanged?.Invoke(CardsInHand.Count);
        }

        public void AddCardToBoard(WorkingCard card)
        {
            CardsOnBoard.Add(card);

            BoardChanged?.Invoke(CardsOnBoard.Count);
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

            BoardChanged?.Invoke(CardsOnBoard.Count);
        }

        public void AddCardToGraveyard(WorkingCard card)
        {
            CardsInGraveyard.Add(card);

            GraveyardChanged?.Invoke(CardsInGraveyard.Count);
        }

        public void RemoveCardFromGraveyard(WorkingCard card)
        {
            CardsInGraveyard.Remove(card);

            GraveyardChanged?.Invoke(CardsInGraveyard.Count);
        }

        public void SetDeck(List<string> cards)
        {
            CardsInDeck = new List<WorkingCard>();

            cards = ShuffleCardsList(cards);

            foreach (string card in cards)
            {
#if DEV_MODE
                if (IsLocalPlayer)
                {
                    CardsInDeck.Add(new WorkingCard(_dataManager.CachedCardsLibraryData.GetCardFromName(card /* 15 */), this)); // special card id
                }
#endif
                CardsInDeck.Add(new WorkingCard(_dataManager.CachedCardsLibraryData.GetCardFromName(card), this));
            }

            DeckChanged?.Invoke(CardsInDeck.Count);
        }

        public List<T> ShuffleCardsList<T>(List<T> cards)
        {
            List<T> array = cards;

            if (!_gameplayManager.IsTutorial)
            {
                InternalTools.ShakeList(ref array); // shake
            }

            return array;
        }

        public void SetFirstHand(bool isTutorial = false)
        {
            if (isTutorial)
                return;

            for (int i = 0; i < Constants.DefaultCardsInHandAtStartGame; i++)
            {
                if (IsLocalPlayer && !_gameplayManager.IsTutorial)
                {
                    _cardsController.AddCardToDistributionState(this, CardsInDeck[i]);
                }
                else
                {
                    _cardsController.AddCardToHand(this, CardsInDeck[0]);
                }
            }
        }

        public void DistributeCard()
        {
            if (IsLocalPlayer)
            {
                _cardsController.AddCardToDistributionState(this, GetCardThatNotInDistribution()); // CardsInDeck[UnityEngine.Random.Range(0, CardsInDeck.Count)]);
            }
            else
            {
                _cardsController.AddCardToHand(this, CardsInDeck[Random.Range(0, CardsInDeck.Count)]);
            }
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

            _soundManager.PlaySound(Enumerators.SoundType.HERO_DEATH, Constants.HeroDeathSoundVolume);

            if (!_gameplayManager.IsTutorial)
            {
                _gameplayManager.EndGame(IsLocalPlayer ? Enumerators.EndGameType.LOSE : Enumerators.EndGameType.WIN);
            }
        }

        public void SetGlowStatus(bool status)
        {
            _avatarSelectedHighlight.SetActive(status);
        }

        public void Stun(Enumerators.StunType stunType, int turnsCount)
        {
            // todo implement logic
            _freezedHighlightObject.SetActive(true);
            IsStunned = true;
            _turnsLeftToFreeFromStun = turnsCount;

            _skillsController.BlockSkill(this, Enumerators.SkillType.PRIMARY);
            _skillsController.BlockSkill(this, Enumerators.SkillType.SECONDARY);
        }

        public void ThrowPlayCardEvent(WorkingCard card)
        {
            CardPlayed?.Invoke(card);
        }

        public void ThrowOnHandChanged()
        {
            HandChanged?.Invoke(CardsInHand.Count);
        }

        private WorkingCard GetCardThatNotInDistribution()
        {
            List<WorkingCard> usedCards = CardsPreparingToHand.Select(x => x.WorkingCard).ToList();
            List<WorkingCard> cards = CardsInDeck.FindAll(x => !usedCards.Contains(x)).ToList();

            return cards[0];
        }

        #region handlers

        private void PlayerHpChangedHandler(int now)
        {
            if (now <= 0 && !_isDead)
            {
                PlayerDie();

                _isDead = true;
            }
        }

        #endregion
    }
}
