using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;
using Loom.ZombieBattleground.View;
using UnityEngine;
using Hero = Loom.ZombieBattleground.Data.Hero;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class Player : BoardObject, IView
    {
        public int Turn;

        public int InitialHp;

        public int CurrentGooModificator;

        public int DamageByNoMoreCardsInDeck;

        private readonly GameObject _freezedHighlightObject;

        private readonly IDataManager _dataManager;

        private readonly BackendFacade _backendFacade;

        private readonly BackendDataControlMediator _backendDataControlMediator;

        private readonly IGameplayManager _gameplayManager;

        private readonly ISoundManager _soundManager;

        private readonly CardsController _cardsController;

        private readonly BattlegroundController _battlegroundController;

        private readonly SkillsController _skillsController;

        private readonly AnimationsController _animationsController;

        private readonly GameObject _avatarObject;

        private readonly GameObject _avatarAfterDeadObject;

        private readonly GameObject _overlordRegularObject;

        private readonly GameObject _overlordDeathObject;

        private readonly GameObject _avatarHeroHighlight;

        private readonly GameObject _avatarSelectedHighlight;

        private readonly Animator _avatarAnimator;

        private readonly Animator _deathAnimator;

        private readonly Animator _regularAnimator;

        private readonly FadeTool _gooBarFadeTool;

        private int _goo;

        private int _gooOnCurrentTurn;

        private int _health;

        private int _graveyardCardsCount;

        private bool _isDead;

        private int _turnsLeftToFreeFromStun;

        private IMatchManager _matchManager;

        private IPvPManager _pvpManager;

        public Player(int id, GameObject playerObject, bool isOpponent)
        {
            Id = id;
            PlayerObject = playerObject;
            IsLocalPlayer = !isOpponent;

            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _matchManager = GameClient.Get<IMatchManager>();
            _pvpManager = GameClient.Get<IPvPManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _animationsController = _gameplayManager.GetController<AnimationsController>();

            CardsInDeck = new List<WorkingCard>();
            CardsInGraveyard = new List<WorkingCard>();
            CardsInHand = new List<WorkingCard>();
            CardsOnBoard = new List<WorkingCard>();
            BoardCards = new List<BoardUnitView>();
            BoardSpellsInUse = new List<BoardSpell>();

            CardsPreparingToHand = new List<BoardCard>();

            int heroId;

            if (!isOpponent)
            {
                if (!_gameplayManager.IsTutorial)
                {
                    heroId = _dataManager.CachedDecksData.Decks.First(d => d.Id == _gameplayManager.PlayerDeckId)
                        .HeroId;
                }
                else
                {
                    heroId = Constants.TutorialPlayerHeroId;
                }
            }
            else
            {
                switch (_matchManager.MatchType)
                {
                    case Enumerators.MatchType.LOCAL:
                        heroId = _dataManager.CachedOpponentDecksData.Decks.First(d => d.Id == _gameplayManager.OpponentDeckId).HeroId;
                        break;
                    case Enumerators.MatchType.PVP:
                        heroId = _pvpManager.OpponentDeck.HeroId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            SelfHero = _dataManager.CachedHeroesData.HeroesParsed[heroId];

            _health = Constants.DefaultPlayerHp;
            InitialHp = _health;
            BuffedHp = 0;
            _goo = Constants.DefaultPlayerGoo;

            _overlordRegularObject = playerObject.transform.Find("OverlordArea/RegularModel").gameObject;
            _avatarAfterDeadObject = _overlordRegularObject.transform.Find("RegularPosition/FrameShuttering (1)/FrameExplosion").gameObject;
            _avatarObject = playerObject.transform.Find("Avatar/Hero_Object").gameObject;
            _overlordDeathObject = playerObject.transform.Find("OverlordArea/OverlordDeath").gameObject;
            _avatarHeroHighlight = playerObject.transform.Find("Avatar/HeroHighlight").gameObject;
            _avatarSelectedHighlight = playerObject.transform.Find("Avatar/SelectedHighlight").gameObject;

            _avatarAnimator = _avatarObject.GetComponent<Animator>();
            _deathAnimator = _overlordDeathObject.GetComponent<Animator>();
            _regularAnimator = _overlordRegularObject.GetComponent<Animator>();
            _gooBarFadeTool = _avatarObject.GetComponent<FadeTool>();

            _freezedHighlightObject = playerObject.transform.Find("Avatar/FreezedHighlight").gameObject;

            _avatarAnimator.enabled = false;
            _deathAnimator.enabled = false;
            _regularAnimator.enabled = false;
            _deathAnimator.StopPlayback();

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

        public event Action<WorkingCard, AffectObjectType, int> CardAttacked;

        public event Action LeaveMatch;

        public event Action<List<WorkingCard>> Mulligan;

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
                _health = Mathf.Clamp(value, 0, 99);

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

        public List<BoardUnitView> BoardCards { get; set; }

        public List<BoardSpell> BoardSpellsInUse { get; set; }

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
            GameObject cardObject;
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
                _animationsController.MoveCardFromPlayerDeckToPlayerHandAnimation(opponent, this,
                    _cardsController.GetBoardCard(card));
            }
            else
            {
                _animationsController.MoveCardFromPlayerDeckToOpponentHandAnimation(opponent, this,
                    _cardsController.GetOpponentBoardCard(card));
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
            ThrowPlayCardEvent(card);
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

        public void SetDeck(List<string> cards, bool isMainTurnSecond)
        {
            CardsInDeck = new List<WorkingCard>();

            cards = ShuffleCardsList(cards);

            if(isMainTurnSecond)
            {
                _cardsController.SetNewCardInstanceId(Constants.MinDeckSize);
            }
            else
            {
                _cardsController.SetNewCardInstanceId(0);
            }
           
            foreach (string card in cards)
            {
                CardsInDeck.Add(new WorkingCard(_dataManager.CachedCardsLibraryData.GetCardFromName(card), this));
            }

            DeckChanged?.Invoke(CardsInDeck.Count);
        }

        public List<T> ShuffleCardsList<T>(List<T> cards)
        {
            if (cards.Count == 0)
                return cards;

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

            ThrowMulliganCardsEvent(_cardsController.MulliganCards);
        }

        public void DistributeCard()
        {
            if (IsLocalPlayer)
            {
                _cardsController.AddCardToDistributionState(this,
                    GetCardThatNotInDistribution()); // CardsInDeck[UnityEngine.Random.Range(0, CardsInDeck.Count)]);
            }
            else
            {
                _cardsController.AddCardToHand(this, CardsInDeck[Random.Range(0, CardsInDeck.Count)]);
            }
        }

        public async Task PlayerDie()
        {
            _gooBarFadeTool.FadeIn();

            Material heroAvatarMaterial = _avatarObject.transform.GetChild(1).GetComponent<Renderer>().material;

            MeshRenderer renderer;
            for (int i = 0; i < _avatarAfterDeadObject.transform.childCount; i++)
            {
                renderer = _avatarAfterDeadObject.transform.GetChild(i).GetComponent<MeshRenderer>();

                if (renderer  != null)
                {
                    renderer.material.mainTexture = heroAvatarMaterial.mainTexture;
                }
            }

            _overlordDeathObject.SetActive(true);
            _avatarObject.SetActive(false);
            _avatarHeroHighlight.SetActive(false);
            _avatarAfterDeadObject.SetActive(true);
            _deathAnimator.enabled = true;
            _regularAnimator.enabled = true;
            _deathAnimator.Play(0);
            _regularAnimator.Play(0);

            _skillsController.DisableSkillsContent(this);

            _soundManager.PlaySound(Enumerators.SoundType.HERO_DEATH, Constants.HeroDeathSoundVolume);

            if (!_gameplayManager.IsTutorial)
            {
                _gameplayManager.EndGame(IsLocalPlayer ? Enumerators.EndGameType.LOSE : Enumerators.EndGameType.WIN);

                await _backendFacade.EndMatch(_backendDataControlMediator.UserDataModel.UserId,
                                                (int)_pvpManager.MatchResponse.Match.Id,
                                                IsLocalPlayer ? _pvpManager.GetOpponentUserId() : _backendDataControlMediator.UserDataModel.UserId);

            }
            else
            {
                GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.HERO_DEATH);
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

        public void ThrowCardAttacked(WorkingCard card, AffectObjectType type, int instanceId)
        {
            CardAttacked?.Invoke(card, type, instanceId);
        }

        public void ThrowLeaveMatch()
        {
            LeaveMatch?.Invoke();
        }

        public void ThrowOnHandChanged()
        {
            HandChanged?.Invoke(CardsInHand.Count);
        }

        private void ThrowMulliganCardsEvent(List<WorkingCard> cards)
        {
            Mulligan?.Invoke(cards);
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
                if (!IsLocalPlayer)
                {
                    GameClient.Get<IOverlordManager>().ReportExperienceAction(_gameplayManager.CurrentPlayer.SelfHero, Common.Enumerators.ExperienceActionType.KillOverlord);
                }

                PlayerDie();

                _isDead = true;
            }
        }

        #endregion

    }
}
