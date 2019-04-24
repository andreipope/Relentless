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
#if UNITY_EDITOR
using ZombieBattleground.Editor.Runtime;
#endif

namespace Loom.ZombieBattleground
{
    public class Player : IBoardObject, IView, IInstanceIdOwner
    {
        private static readonly ILog Log = Logging.GetLog(nameof(Player));

        public int Turn { get; set; }

        public int InitialDefense { get; }

        public int CurrentGooModificator { get; set; }

        public int DamageByNoMoreCardsInDeck { get; set; }

        public int ExtraGoo { get; set; }

        public uint InitialCardsInHandCount { get; }

        public uint MaxCardsInPlay { get; }

        public uint MaxCardsInHand { get; }

        public uint MaxGooVials { get; }

        public uint TurnTime { get; }

        public PlayerState InitialPvPPlayerState { get; }

        public Data.InstanceId InstanceId { get; }

        public bool MulliganWasStarted { get; set; }

        public PlayerCardsController PlayerCardsController { get; }

        private readonly GameObject _freezedHighlightObject;

        private readonly IDataManager _dataManager;

        private readonly INetworkActionManager _networkActionManager;

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

        private readonly BoardArrowController _boardArrowController;

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
            _networkActionManager = GameClient.Get<INetworkActionManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _animationsController = _gameplayManager.GetController<AnimationsController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();

            PlayerCardsController = new PlayerCardsController(this);

            switch (_matchManager.MatchType)
            {
                case Enumerators.MatchType.PVP:

                    // TODO: REMOVE logs when issue will be fixed
                    Log.Debug($"UserDataModel.UserId: {_backendDataControlMediator.UserDataModel.UserId}");
                    Log.Debug($"isOpponent: {isOpponent}");

                    foreach(PlayerState state in _pvpManager.InitialGameState.PlayerStates)
                    {
                        Log.Debug($"state.id: {state.Id}");
                    }

                    InitialPvPPlayerState =
                        _pvpManager.InitialGameState.PlayerStates
                        .First(state =>
                                isOpponent ?
                                    state.Id != _backendDataControlMediator.UserDataModel.UserId :
                                    state.Id == _backendDataControlMediator.UserDataModel.UserId
                                    );

                    Log.Debug("InitialPvPPlayerState:\r\n" + Utilites.JsonPrettyPrint(InitialPvPPlayerState.ToString()));

                    InitialCardsInHandCount = (uint) InitialPvPPlayerState.InitialCardsInHandCount;
                    MaxCardsInHand = (uint) InitialPvPPlayerState.MaxCardsInHand;
                    MaxCardsInPlay = (uint) InitialPvPPlayerState.MaxCardsInPlay;
                    MaxGooVials = (uint) InitialPvPPlayerState.MaxGooVials;

                    Defense = InitialPvPPlayerState.Defense;

                    CurrentGoo = InitialPvPPlayerState.CurrentGoo;
                    GooVials = InitialPvPPlayerState.GooVials;

                    if (CurrentGoo == 1)
                    {
                        CurrentGoo = 0;
                    }
                    if (GooVials == 1)
                    {
                        GooVials = 0;
                    }

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

            int overlordId = -1;

            if (!isOpponent)
            {
                if (!_gameplayManager.IsTutorial || _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().
                            SpecificBattlegroundInfo.DisabledInitialization)
                {
                    if(_matchManager.MatchType == Enumerators.MatchType.PVP)
                    {
                        foreach (PlayerState playerState in _pvpManager.InitialGameState.PlayerStates)
                        {
                            if (playerState.Id == _backendDataControlMediator.UserDataModel.UserId)
                            {
                                overlordId = (int) playerState.Deck.OverlordId;
                            }
                        }
                    }
                    else
                    {
                        overlordId = _dataManager.CachedDecksData.Decks.First(d => d.Id == _gameplayManager.PlayerDeckId).OverlordId;
                    }
                }
                else
                {
                    overlordId = _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.PlayerInfo.OverlordId;
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
                            overlordId = _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.OpponentInfo.OverlordId;
                        }
                        else
                        {
                            overlordId = _dataManager.CachedAiDecksData.Decks.First(d => d.Deck.Id == _gameplayManager.OpponentDeckId).Deck.OverlordId;
                        }
                        break;
                    case Enumerators.MatchType.PVP:
                        overlordId = (int) InitialPvPPlayerState.Deck.OverlordId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            SelfOverlord = _dataManager.CachedOverlordData.Overlords[overlordId];

            // TODO: REMOVE logs when issue will be fixed
            Log.Debug($"SelfOverlord: {SelfOverlord}");

            InitialDefense = _defense;
            BuffedDefense = 0;

            _overlordDeathObject = playerObject.transform.Find("OverlordArea/OverlordDeath").gameObject;
            _overlordRegularObject = playerObject.transform.Find("OverlordArea/RegularModel").gameObject;
            _avatarObject = _overlordRegularObject.transform.Find("RegularPosition/Avatar/OverlordImage").gameObject;
            _avatarSelectedHighlight = _overlordRegularObject.transform.Find("RegularPosition/Avatar/SelectedHighlight").gameObject;
            _freezedHighlightObject = _overlordRegularObject.transform.Find("RegularPosition/Avatar/FreezedHighlight").gameObject;
            _drawCradParticle = playerObject.transform.Find("Deck_Illustration/DrawCardVFX").GetComponent<ParticleSystem>();

            string name = SelfOverlord.Faction + "HeroFrame";
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

        public event Action<CardModel> DrawCard;

        public event Action<CardModel, int> CardPlayed;

        public event Action<CardModel, Data.InstanceId> CardAttacked;

        public event Action LeaveMatch;

        public GameObject PlayerObject { get; }

        public GameObject AvatarObject => _avatarObject?.transform.parent?.gameObject;

        public GameObject GameObject => PlayerObject;

        public Transform Transform => PlayerObject.transform;

        public OverlordModel SelfOverlord { get; }

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

        // TODO: refactor-state: these list are here temporarily and will be removed
        public IReadOnlyList<CardModel> CardsInDeck => PlayerCardsController.CardsInDeck;

        public IReadOnlyList<CardModel> CardsInGraveyard => PlayerCardsController.CardsInGraveyard;

        public IReadOnlyList<CardModel> CardsInHand => PlayerCardsController.CardsInHand;

        public IReadOnlyList<CardModel> CardsOnBoard => PlayerCardsController.CardsOnBoard;

        public IReadOnlyList<CardModel> MulliganCards => PlayerCardsController.MulliganCards;

        public bool IsStunned { get; private set; }

        public int BuffedDefense { get; set; }

        public int MaxCurrentDefense => InitialDefense + BuffedDefense;

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
            if (_gameplayManager.CurrentTurnPlayer == this)
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
                    IView cardView = PlayerCardsController.AddCardFromDeckToHand();
                    (cardView as BoardCardView)?.SetDefaultAnimation();
                }

                // Second player draw two cards on their first turn
                if (_battlegroundController.CurrentTurn == 2 && !_gameplayManager.IsTutorial)
                {
                    IView cardView = PlayerCardsController.AddCardFromDeckToHand();
                    (cardView as BoardCardView)?.SetDefaultAnimation();
                }
            }

            TurnStarted?.Invoke();
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
            _boardArrowController.ResetCurrentBoardArrow();

            switch (SelfOverlord.Faction)
            {
                case Enumerators.Faction.FIRE:
                case Enumerators.Faction.WATER:
                case Enumerators.Faction.EARTH:
                case Enumerators.Faction.AIR:
                case Enumerators.Faction.LIFE:
                case Enumerators.Faction.TOXIC:
                    Enumerators.SoundType soundType = (Enumerators.SoundType)Enum.Parse(typeof(Enumerators.SoundType), "HERO_DEATH_" + SelfOverlord.Faction);
                    _soundManager.PlaySound(soundType, Constants.OverlordDeathSoundVolume);
                    break;
                default:
                    _soundManager.PlaySound(Enumerators.SoundType.HERO_DEATH, Constants.OverlordDeathSoundVolume);
                    break;
            }

            if (!_gameplayManager.IsTutorial || ( _gameplayManager.IsTutorial &&
                                                 _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().
                                                 SpecificBattlegroundInfo.DisabledInitialization))
            {
                InternalTools.DoActionDelayed(() =>
                {
                    _gameplayManager.EndGame(IsLocalPlayer ? Enumerators.EndGameType.LOSE : Enumerators.EndGameType.WIN);
                    if (!IsLocalPlayer && _matchManager.MatchType == Enumerators.MatchType.PVP)
                    {
                        _actionsQueueController.ClearActions();

                        _actionsQueueController.AddNewActionInToQueue(completeCallback =>
                        {
                            _networkActionManager.EnqueueMessage(
                                new MatchRequestFactory(_pvpManager.MatchMetadata.Id).EndMatch(
                                    _backendDataControlMediator.UserDataModel.UserId,
                                    IsLocalPlayer ? _pvpManager.GetOpponentUserId() : _backendDataControlMediator.UserDataModel.UserId
                                )
                            );

                            completeCallback?.Invoke();
                        }, Enumerators.QueueActionType.EndMatch);
                    }
                }, 2f);
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
            if (_gameplayManager.CurrentTurnPlayer != this)
                turnsCount++;

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

        public void ThrowDrawCardEvent(CardModel cardModel)
        {
            DrawCard?.Invoke(cardModel);
        }

        public void ThrowPlayCardEvent(CardModel cardModel, int position)
        {
            CardPlayed?.Invoke(cardModel, position);
        }

        public void ThrowCardAttacked(CardModel cardModel, Data.InstanceId instanceId)
        {
            CardAttacked?.Invoke(cardModel, instanceId);
        }

        public void ThrowLeaveMatch()
        {
            _actionsQueueController.ClearActions();

            _actionsQueueController.AddNewActionInToQueue(completeCallback =>
            {
                LeaveMatch?.Invoke();

                completeCallback?.Invoke();
            }, Enumerators.QueueActionType.LeaveMatch);
        }

        private CardModel GetCardThatNotInDistribution()
        {
            List<CardModel> cards = CardsInDeck.FindAll(x => !MulliganCards.Contains(x)).ToList();

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

            DebugCardInfoDrawer.Draw(AvatarObject.transform.position, InstanceId.Id, SelfOverlord.Name);
        }
#endif

#region handlers

        private void PlayerDefenseChangedHandler(int now)
        {
            if (now <= 0 && !_isDead)
            {
                if (!IsLocalPlayer)
                {
                    GameClient.Get<IOverlordExperienceManager>().ReportExperienceAction(_gameplayManager.CurrentPlayer.SelfOverlord, Common.Enumerators.ExperienceActionType.KillOverlord);
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
