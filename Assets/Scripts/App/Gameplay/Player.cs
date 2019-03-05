using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;
using Loom.ZombieBattleground.View;
using DG.Tweening;
using log4net;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Hero = Loom.ZombieBattleground.Data.Hero;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using ZombieBattleground.Editor.Runtime;
#endif

namespace Loom.ZombieBattleground
{
    public class Player : BoardObject, IView, IInstanceIdOwner
    {
        private static readonly ILog Log = Logging.GetLog(nameof(Player));

        public int Turn { get; set; }

        public int InitialHp { get; private set; }

        public int CurrentGooModificator { get; set; }

        public int DamageByNoMoreCardsInDeck { get; set; }

        public int ExtraGoo { get; set; }

        public uint InitialCardsInHandCount { get; private set; }

        public uint MaxCardsInPlay { get; private set; }

        public uint MaxCardsInHand { get; private set; }

        public uint MaxGooVials { get; private set; }

        public uint TurnTime { get; private set; }

        public PlayerState InitialPvPPlayerState { get; }

        public Data.InstanceId InstanceId { get; }

        public bool MulliganWasStarted { get; set; }

        private readonly GameObject _freezedHighlightObject;

        private readonly IDataManager _dataManager;

        private readonly IQueueManager _queueManager;

        private readonly BackendDataControlMediator _backendDataControlMediator;

        private readonly IGameplayManager _gameplayManager;

        private readonly ISoundManager _soundManager;

        private readonly IMatchManager _matchManager;

        private readonly IPvPManager _pvpManager;

        private readonly ITutorialManager _tutorialManager;

        private readonly CardsController _cardsController;

        private readonly BattlegroundController _battlegroundController;

        private readonly SkillsController _skillsController;

        private readonly AnimationsController _animationsController;

        private readonly ActionsQueueController _actionsQueueController;

        private readonly GameObject _avatarObject;

        private readonly Animator _overlordFactionFrameAnimator;

        private readonly GameObject _overlordRegularObject;

        private readonly GameObject _overlordDeathObject;

        private readonly GameObject _avatarSelectedHighlight;

        private readonly Animator _avatarAnimator;

        private readonly Animator _deathAnimator;

        private readonly Animator _regularAnimator;

        private readonly ParticleSystem _drawCradParticle;

        private int _currentGoo;

        private int _gooVials;

        private int _defense;

        private int _graveyardCardsCount;

        private bool _isDead;

        private int _turnsLeftToFreeFromStun;

        public Player(Data.InstanceId instanceId, GameObject playerObject, bool isOpponent)
        {
            InstanceId = instanceId;
            PlayerObject = playerObject;
            IsLocalPlayer = !isOpponent;

            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _matchManager = GameClient.Get<IMatchManager>();
            _pvpManager = GameClient.Get<IPvPManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _queueManager = GameClient.Get<IQueueManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _animationsController = _gameplayManager.GetController<AnimationsController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();

            CardsInDeck = new UniquePositionedList<WorkingCard>(new PositionedList<WorkingCard>());
            CardsInGraveyard = new UniquePositionedList<WorkingCard>(new PositionedList<WorkingCard>());
            CardsInHand = new UniquePositionedList<WorkingCard>(new PositionedList<WorkingCard>());
            CardsOnBoard = new UniquePositionedList<WorkingCard>(new PositionedList<WorkingCard>());
            BoardCards = new UniquePositionedList<BoardUnitView>(new PositionedList<BoardUnitView>());
            BoardSpellsInUse = new UniquePositionedList<BoardSpell>(new PositionedList<BoardSpell>());
            CardsPreparingToHand = new UniquePositionedList<WorkingCard>(new PositionedList<WorkingCard>());

            switch (_matchManager.MatchType)
            {
                case Enumerators.MatchType.PVP:
                    InitialPvPPlayerState =
                        _pvpManager.InitialGameState.PlayerStates
                        .First(state =>
                                isOpponent ?
                                    state.Id != _backendDataControlMediator.UserDataModel.UserId :
                                    state.Id == _backendDataControlMediator.UserDataModel.UserId
                                    );

                    InitialCardsInHandCount = (uint) InitialPvPPlayerState.InitialCardsInHandCount;
                    MaxCardsInHand = (uint) InitialPvPPlayerState.MaxCardsInHand;
                    MaxCardsInPlay = (uint) InitialPvPPlayerState.MaxCardsInPlay;
                    MaxGooVials = (uint) InitialPvPPlayerState.MaxGooVials;

#if USE_REBALANCE_BACKEND
                    Defense = Constants.DefaultPlayerHp;
#else
                    Defense = InitialPvPPlayerState.Defense;
#endif
                    CurrentGoo = InitialPvPPlayerState.CurrentGoo;
                    GooVials = InitialPvPPlayerState.GooVials;
                    TurnTime = (uint) InitialPvPPlayerState.TurnTime;
                    break;
                default:
                    InitialCardsInHandCount = Constants.DefaultCardsInHandAtStartGame;
                    MaxCardsInHand = Constants.MaxCardsInHand;
                    MaxCardsInPlay = Constants.MaxBoardUnits;
                    MaxGooVials = Constants.MaximumPlayerGoo;

                    Defense = Constants.DefaultPlayerHp;
                    CurrentGoo = Constants.DefaultPlayerGoo;
                    GooVials = _currentGoo;
                    TurnTime = (uint) Constants.TurnTime;
                    break;
            }

            int heroId = -1;

            if (!isOpponent)
            {
                if (!_gameplayManager.IsTutorial)
                {
                    if(_matchManager.MatchType == Enumerators.MatchType.PVP)
                    {
                        foreach (PlayerState playerState in _pvpManager.InitialGameState.PlayerStates)
                        {
                            if (playerState.Id == _backendDataControlMediator.UserDataModel.UserId)
                            {
                                heroId = (int) playerState.Deck.HeroId;
                            }
                        }
                    }
                    else
                    {
                        heroId = _dataManager.CachedDecksData.Decks.First(d => d.Id == _gameplayManager.PlayerDeckId).HeroId;
                    }
                }
                else
                {
                    heroId = _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.PlayerInfo.OverlordId;
                }
            }
            else
            {
                switch (_matchManager.MatchType)
                {
                    case Enumerators.MatchType.LOCAL:
                        if (_gameplayManager.IsTutorial && 
                            !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().
                            SpecificBattlegroundInfo.DisabledInitialization)
                        {
                            heroId = _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.OpponentInfo.OverlordId;
                        }
                        else
                        {
                            heroId = _dataManager.CachedAiDecksData.Decks.First(d => d.Deck.Id == _gameplayManager.OpponentDeckId).Deck.HeroId;
                        }
                        break;
                    case Enumerators.MatchType.PVP:
                        heroId = (int) InitialPvPPlayerState.Deck.HeroId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            SelfHero = _dataManager.CachedHeroesData.Heroes[heroId];

            InitialHp = _defense;
            BuffedHp = 0;

            _overlordDeathObject = playerObject.transform.Find("OverlordArea/OverlordDeath").gameObject;
            _overlordRegularObject = playerObject.transform.Find("OverlordArea/RegularModel").gameObject;
            _avatarObject = _overlordRegularObject.transform.Find("RegularPosition/Avatar/OverlordImage").gameObject;
            _avatarSelectedHighlight = _overlordRegularObject.transform.Find("RegularPosition/Avatar/SelectedHighlight").gameObject;
            _freezedHighlightObject = _overlordRegularObject.transform.Find("RegularPosition/Avatar/FreezedHighlight").gameObject;
            _drawCradParticle = playerObject.transform.Find("Deck_Illustration/DrawCardVFX").GetComponent<ParticleSystem>();

            string name = SelfHero.HeroElement.ToString() + "HeroFrame";
            GameObject prefab = GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>("Prefabs/Gameplay/OverlordFrames/" + name);
            Transform frameObjectTransform = MonoBehaviour.Instantiate(prefab,
                        _overlordRegularObject.transform.Find("RegularPosition/Avatar/FactionFrame"),
                        false).transform;
            frameObjectTransform.name = name;
            _overlordFactionFrameAnimator = frameObjectTransform.Find("Anim").GetComponent<Animator>();
            _overlordFactionFrameAnimator.speed = 0;

            _avatarAnimator = _avatarObject.GetComponent<Animator>();
            _deathAnimator = _overlordDeathObject.GetComponent<Animator>();
            _regularAnimator = _overlordRegularObject.GetComponent<Animator>();

            _avatarAnimator.enabled = false;
            _deathAnimator.enabled = false;
            _regularAnimator.enabled = false;
            _deathAnimator.StopPlayback();

            PlayerDefenseChanged += PlayerDefenseChangedHandler;

            DamageByNoMoreCardsInDeck = 0;

#if UNITY_EDITOR
            MainApp.Instance.OnDrawGizmosCalled += OnDrawGizmos;
#endif

            _gameplayManager.GetController<InputController>().PlayerSelectedEvent += PlayerSelectedEventHandler;

        }

        public event Action TurnStarted;

        public event Action TurnEnded;

        public event Action<int> PlayerDefenseChanged;

        public event Action<int> PlayerCurrentGooChanged;

        public event Action<int> PlayerGooVialsChanged;

        public event Action<int> DeckChanged;

        public event Action<int> HandChanged;

        public event Action<int> GraveyardChanged;

        public event Action<int> BoardChanged;

        public event Action<WorkingCard> DrawCard;

        public event Action<WorkingCard, int> CardPlayed;

        public event Action<WorkingCard, Data.InstanceId> CardAttacked;

        public event Action LeaveMatch;

        public event Action<List<WorkingCard>> Mulligan;

        public GameObject PlayerObject { get; }

        public GameObject AvatarObject => _avatarObject.transform.parent.gameObject;

        public Transform Transform => PlayerObject.transform;

        public Hero SelfHero { get; }

        public int GooVials
        {
            get => _gooVials;
            set
            {
                _gooVials = Mathf.Clamp(value, 0, (int) MaxGooVials);

                PlayerGooVialsChanged?.Invoke(_gooVials);
            }
        }

        public int CurrentGoo
        {
            get => _currentGoo;
            set
            {
                _currentGoo = Mathf.Clamp(value, 0, 999999);

                PlayerCurrentGooChanged?.Invoke(_currentGoo);
            }
        }

        public int Defense
        {
            get => _defense;
            set
            {
                _defense = Mathf.Clamp(value, 0, 99);

                PlayerDefenseChanged?.Invoke(_defense);
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

        public UniquePositionedList<BoardUnitView> BoardCards { get; }

        public UniquePositionedList<BoardSpell> BoardSpellsInUse { get; }

        public UniquePositionedList<WorkingCard> CardsInDeck { get; }

        public UniquePositionedList<WorkingCard> CardsInGraveyard { get; }

        public UniquePositionedList<WorkingCard> CardsInHand { get; }

        public UniquePositionedList<WorkingCard> CardsOnBoard { get; }

        public UniquePositionedList<WorkingCard> CardsPreparingToHand { get; }

        public bool IsStunned { get; private set; }

        public int BuffedHp { get; set; }

        public int MaxCurrentHp => InitialHp + BuffedHp;

        public void InvokeTurnEnded()
        {
            TurnEnded?.Invoke();
            if (CurrentGoo > GooVials)
            {
                CurrentGoo = GooVials;
            }
        }

        public void InvokeTurnStarted()
        {
            if (_gameplayManager.CurrentTurnPlayer.Equals(this))
            {
                GooVials++;
                CurrentGoo = GooVials + CurrentGooModificator + ExtraGoo;
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

                // Backend already draws a card at the start
                if (!_pvpManager.UseBackendGameLogic ||
                    _pvpManager.UseBackendGameLogic && _battlegroundController.CurrentTurn != 1)
                {
                    IView cardView = _cardsController.AddCardToHand(this);
                    (cardView as BoardCard)?.SetDefaultAnimation();
                }

                // Second player draw two cards on their first turn
                if (_battlegroundController.CurrentTurn == 2 && !_gameplayManager.IsTutorial)
                {
                    IView cardView = _cardsController.AddCardToHand(this);
                    (cardView as BoardCard)?.SetDefaultAnimation();
                }
            }

            TurnStarted?.Invoke();
        }

        public void AddCardToDeck(WorkingCard card, bool shuffle = false)
        {
            if (shuffle)
            {
                CardsInDeck.Insert(MTwister.IRandom(0, CardsInDeck.Count), card);
            }
            else
            {
                CardsInDeck.Insert(ItemPosition.End, card);
            }

            DeckChanged?.Invoke(CardsInDeck.Count);
        }

        public void RemoveCardFromDeck(WorkingCard card)
        {
            CardsInDeck.Remove(card);

            DeckChanged?.Invoke(CardsInDeck.Count);
        }

        public void InvokeDeckChangedEvent () {
            DeckChanged?.Invoke(CardsInDeck.Count);
        }

        public IView AddCardToHand(WorkingCard card, bool silent = false)
        {
            IView cardView;
            CardsInHand.Insert(ItemPosition.End, card);

            if (IsLocalPlayer)
            {
                cardView = _cardsController.AddCardToHand(card, silent);
                _battlegroundController.UpdatePositionOfCardsInPlayerHand(silent);
            }
            else
            {
                cardView = _cardsController.AddCardToOpponentHand(card, silent);

                _battlegroundController.UpdatePositionOfCardsInOpponentHand(true, !silent);
            }

            HandChanged?.Invoke(CardsInHand.Count);

            return cardView;
        }

        public void AddCardToHandFromOpponentDeck(Player opponent, WorkingCard card)
        {
            card.Owner = this;

            CardsInHand.Insert(ItemPosition.End, card);

            if (IsLocalPlayer)
            {
                _animationsController.MoveCardFromPlayerDeckToPlayerHandAnimation(
                    opponent,
                    this,
                    _cardsController.GetBoardCard(card));
            }
            else
            {
                _animationsController.MoveCardFromPlayerDeckToOpponentHandAnimation(
                    opponent,
                    this,
                    _cardsController.GetOpponentBoardCard(card)
                    );
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

        public void AddCardToBoard(WorkingCard card, ItemPosition position)
        {
            if (CardsOnBoard.Contains(card))
            {
                Log.Warn($"Attempt to add card {card} to CardsOnBoard when it is already added");
                return;
            }
            CardsOnBoard.Insert(position, card);
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
            if (CardsInGraveyard.Contains(card))
                return;

            CardsInGraveyard.Insert(ItemPosition.End, card);

            GraveyardChanged?.Invoke(CardsInGraveyard.Count);
        }

        public void RemoveCardFromGraveyard(WorkingCard card)
        {
            if (!CardsInGraveyard.Contains(card))
                return;

            CardsInGraveyard.Remove(card);

            GraveyardChanged?.Invoke(CardsInGraveyard.Count);
        }

        public void SetDeck(List<WorkingCard> cards, bool isMainTurnSecond)
        {
            CardsInDeck.Clear();

            switch (_matchManager.MatchType)
            {
                case Enumerators.MatchType.LOCAL:
                    if (!_gameplayManager.IsTutorial)
                    {
                        cards.ShuffleList();
                    }

                    if(isMainTurnSecond)
                    {
                        _cardsController.SetNewCardInstanceId(Constants.MinDeckSize);
                    }
                    else
                    {
                        _cardsController.SetNewCardInstanceId(0);
                    }

                    CardsInDeck.InsertRange(ItemPosition.End, cards);

                    break;
                case Enumerators.MatchType.PVP:
                    CardsInDeck.InsertRange(ItemPosition.End, cards);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DeckChanged?.Invoke(CardsInDeck.Count);
        }

        public void SetFirstHandForLocalMatch(bool skip)
        {
            if (skip)
                return;

            for (int i = 0; i < InitialCardsInHandCount; i++)
            {
                if (i >= CardsInDeck.Count)
                    break;

                if (IsLocalPlayer && (!_gameplayManager.IsTutorial ||
                    (_gameplayManager.IsTutorial &&
                    _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().
                    SpecificBattlegroundInfo.DisabledInitialization)))
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

        public void SetFirstHandForPvPMatch(List<WorkingCard> workingCards, bool removeCardsFromDeck = true)
        {
            foreach (WorkingCard workingCard in workingCards)
            {
                if (IsLocalPlayer && !_gameplayManager.IsTutorial)
                {
                    _cardsController.AddCardToDistributionState(this, workingCard);
                }
                else
                {
                    _cardsController.AddCardToHand(this, workingCard, removeCardsFromDeck);
                }
            }

            ThrowMulliganCardsEvent(_cardsController.MulliganCards);
        }

        public void PlayerDie()
        {
            _avatarAnimator.enabled = true;
            _overlordDeathObject.SetActive(true);
            _deathAnimator.enabled = true;
            _regularAnimator.enabled = true;
            _overlordFactionFrameAnimator.speed = 1;
            _avatarAnimator.Play(0);
            _deathAnimator.Play(0);
            _regularAnimator.Play(0);

            _gameplayManager.GetController<InputController>().PlayerSelectedEvent -= PlayerSelectedEventHandler;

            FadeTool overlordFactionFrameFadeTool = _overlordFactionFrameAnimator.transform.GetComponent<FadeTool>();
            if (overlordFactionFrameFadeTool != null)
                overlordFactionFrameFadeTool.FadeIn();

            List<MeshRenderer> overlordImagePieces = _avatarObject.transform.GetComponentsInChildren<MeshRenderer>().ToList();
            Color color = new Color(1, 1, 1, 1);
            DOTween.ToAlpha(() => color, changedColor => color = changedColor, 0, 2).SetDelay(2).OnUpdate(
                () => {
                    foreach (MeshRenderer renderer in overlordImagePieces)
                    {
                        if (renderer == null || !renderer)
                            continue;

                        renderer.material.color = color;
                    }
                }
            );

            _skillsController.DisableSkillsContent(this);

            switch (SelfHero.HeroElement)
            {
                case Enumerators.SetType.FIRE:
                case Enumerators.SetType.WATER:
                case Enumerators.SetType.EARTH:
                case Enumerators.SetType.AIR:
                case Enumerators.SetType.LIFE:
                case Enumerators.SetType.TOXIC:
                    var soundType = (Enumerators.SoundType)Enum.Parse(typeof(Enumerators.SoundType), "HERO_DEATH_" + SelfHero.HeroElement);
                    _soundManager.PlaySound(soundType, Constants.HeroDeathSoundVolume);
                    break;
                default:
                    _soundManager.PlaySound(Enumerators.SoundType.HERO_DEATH, Constants.HeroDeathSoundVolume);
                    break;
            }

            if (!_gameplayManager.IsTutorial || ( _gameplayManager.IsTutorial &&
                                                 _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().
                                                 SpecificBattlegroundInfo.DisabledInitialization))
            {
                _gameplayManager.EndGame(IsLocalPlayer ? Enumerators.EndGameType.LOSE : Enumerators.EndGameType.WIN);
                if (!IsLocalPlayer && _matchManager.MatchType == Enumerators.MatchType.PVP)
                {
                    _actionsQueueController.ClearActions();

                    _actionsQueueController.AddNewActionInToQueue((param, completeCallback) =>
                    {
                        _queueManager.AddAction(
                            new MatchRequestFactory(_pvpManager.MatchMetadata.Id).EndMatch(
                                _backendDataControlMediator.UserDataModel.UserId,
                                IsLocalPlayer ? _pvpManager.GetOpponentUserId() : _backendDataControlMediator.UserDataModel.UserId
                            )
                        );

                        completeCallback?.Invoke();
                    }, Enumerators.QueueActionType.EndMatch);
                }
            }
            else
            {
                if (IsLocalPlayer)
                {
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlorDied);
                }
                else
                {
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.EnemyOverlordDied);
                }
            }
        }

        public void SetGlowStatus(bool status)
        {
            _avatarSelectedHighlight.SetActive(status);
        }

        public void PlayDrawCardVFX()
        {
            _drawCradParticle.Play();
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

        public void RevertStun()
        {
            IsStunned = false;
            _freezedHighlightObject.SetActive(false);
            _turnsLeftToFreeFromStun = 0;


            _skillsController.UnBlockSkill(this);
        }

        public void ThrowDrawCardEvent(WorkingCard card)
        {
            DrawCard?.Invoke(card);
        }

        public void ThrowPlayCardEvent(WorkingCard card, int position)
        {
            CardPlayed?.Invoke(card, position);
        }

        public void ThrowCardAttacked(WorkingCard card, Data.InstanceId instanceId)
        {
            CardAttacked?.Invoke(card, instanceId);
        }

        public void ThrowLeaveMatch()
        {
            _actionsQueueController.ClearActions();

            _actionsQueueController.AddNewActionInToQueue((param, completeCallback) =>
            {
                LeaveMatch?.Invoke();

                completeCallback?.Invoke();
            }, Enumerators.QueueActionType.LeaveMatch);
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
            List<WorkingCard> cards = CardsInDeck.FindAll(x => !CardsPreparingToHand.Contains(x)).ToList();

            return cards[0];
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_avatarObject == null || AvatarObject == null)
            {
                MainApp.Instance.OnDrawGizmosCalled -= OnDrawGizmos;
                return;
            }

            DebugCardInfoDrawer.Draw(AvatarObject.transform.position, InstanceId.Id, SelfHero.Name);
        }
#endif

#region handlers

        private void PlayerDefenseChangedHandler(int now)
        {
            if (now <= 0 && !_isDead)
            {
                if (!IsLocalPlayer)
                {
                    GameClient.Get<IOverlordExperienceManager>().ReportExperienceAction(_gameplayManager.CurrentPlayer.SelfHero, Common.Enumerators.ExperienceActionType.KillOverlord);
                }

                PlayerDie();

                _isDead = true;
            }
        }


        private void PlayerSelectedEventHandler(Player player)
        {
            if (IsLocalPlayer)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordSelected);
            }
            else
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.EnemyOverlordSelected);
            }
        }

#endregion

        public override string ToString()
        {
            return $"{{InstanceId: {InstanceId}, IsLocalPlayer: {IsLocalPlayer}}}";
        }
    }
}
