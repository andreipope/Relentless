using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class GameplayManager : IService, IGameplayManager
    {
        private IDataManager _dataManager;

        private IMatchManager _matchManager;

        private ISoundManager _soundManager;

        private IUIManager _uiManager;

        private ITimerManager _timerManager;

        private ITutorialManager _tutorialManager;

        private IPvPManager _pvpManager;

        private List<IController> _controllers;

        private ActionCollectorUploader ActionLogCollectorUploader { get; } = new ActionCollectorUploader();

        public Enumerators.StartingTurn StartingTurn { get; set; }

        public event Action GameStarted;

        public event Action GameInitialized;

        public event Action<Enumerators.EndGameType> GameEnded;

#pragma warning disable 67
        public event Action TurnStarted;

        public event Action TurnEnded;
#pragma warning restore 67

        public int PlayerDeckId { get; set; }

        public int OpponentDeckId { get; set; }

        public bool IsGameStarted { get; set; }

        public bool IsGameEnded { get; set; }

        public bool IsTutorial { get; set; }

        public bool IsPreparingEnded { get; set; }

        public Player CurrentTurnPlayer { get; set; }

        public Player CurrentPlayer { get; set; }

        public Player OpponentPlayer { get; set; }

        public bool IsSpecificGameplayBattleground { get; set; }

        public bool CanDoDragActions { get; set; }

        public bool IsGameplayInputBlocked { get; set; }

        public PlayerMoveAction PlayerMoves { get; set; }

        public T GetController<T>()
            where T : IController
        {
            return (T) _controllers.Find(x => x is T);
        }

        public void RearrangeHands()
        {
            GetController<BattlegroundController>().UpdatePositionOfBoardUnitsOfPlayer(CurrentPlayer.BoardCards);
            GetController<BattlegroundController>().UpdatePositionOfBoardUnitsOfOpponent();
        }

        public void EndGame(Enumerators.EndGameType endGameType, float timer = 4f)
        {
            if (IsGameEnded)
                return;

            IsGameEnded = true;

            _soundManager.PlaySound(Enumerators.SoundType.BACKGROUND, 128, Constants.BackgroundSoundVolume, null, true);

            if (endGameType != Enumerators.EndGameType.CANCEL)
            {
                _timerManager.AddTimer(
                    x =>
                    {
                        switch (endGameType)
                        {
                            case Enumerators.EndGameType.WIN:
                                _uiManager.DrawPopup<YouWonPopup>();
                                break;
                            case Enumerators.EndGameType.LOSE:
                                _uiManager.DrawPopup<YouLosePopup>();
                                break;
                            case Enumerators.EndGameType.CANCEL:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(endGameType), endGameType, null);
                        }
                    },
                    null,
                    timer);
            }

            _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);

            StopGameplay();

            CurrentTurnPlayer = null;
            CurrentPlayer = null;
            OpponentPlayer = null;
            StartingTurn = Enumerators.StartingTurn.UnDecided;
            PlayerMoves = null;

            //GameClient.Get<IQueueManager>().StopNetworkThread();

            GameEnded?.Invoke(endGameType);
        }

        public void StartGameplay()
        {
            _uiManager.DrawPopup<PreparingForBattlePopup>();

            _timerManager.AddTimer(
                x =>
                {
                    _uiManager.HidePopup<PreparingForBattlePopup>();

                    IsGameStarted = true;
                    IsGameEnded = false;
                    IsPreparingEnded = false;

                    CanDoDragActions = true;

                    GameStarted?.Invoke();

                    StartInitializeGame();
                },
                null,
                2f);
        }

        public void StopGameplay()
        {
            IsGameStarted = false;
            IsGameEnded = true;
            IsPreparingEnded = false;

            CanDoDragActions = false;
        }

        public bool IsLocalPlayerTurn()
        {
            return CurrentTurnPlayer.Equals(CurrentPlayer);
        }

        public Player GetOpponentByPlayer(Player player)
        {
            return player.IsLocalPlayer ? OpponentPlayer : CurrentPlayer;
        }

        public Player GetPlayerById(int id)
        {
            return CurrentPlayer.Id == id ? CurrentPlayer : OpponentPlayer;
        }

        public void ResetWholeGameplayScene()
        {
            foreach (IController controller in _controllers)
            {
                controller.ResetAll();
            }
        }

        public bool IsGameplayReady()
        {
            return !IsGameEnded && IsGameStarted && IsPreparingEnded;
        }

        public void Dispose()
        {
            foreach (IController item in _controllers)
            {
                item.Dispose();
            }
        }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _matchManager = GameClient.Get<IMatchManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _pvpManager = GameClient.Get<IPvPManager>();

            InitControllers();

            if (!_dataManager.CachedUserLocalData.Tutorial)
            {
                Constants.ZombiesSoundVolume = 0.25f;
                Constants.CreatureAttackSoundVolume *= 3;
            }
        }

        public void Update()
        {
            foreach (IController item in _controllers)
            {
                item.Update();
            }
        }

        private void InitControllers()
        {
            _controllers = new List<IController>
            {
                new VfxController(),
                new ParticlesController(),
                new AbilitiesController(),
                new ActionsQueueController(),
                new PlayerController(),
                new AIController(),
                new CardsController(),
                new BattlegroundController(),
                new AnimationsController(),
                new BattleController(),
                new BoardArrowController(),
                new SkillsController(),
                new RanksController(),
                new InputController(),
                new OpponentController(),
                new UniqueAnimationsController()
            };

            foreach (IController controller in _controllers)
            {
                controller.Init();
            }
        }

        private void StartInitializeGame()
        {
            if (IsTutorial)
            {
                IsSpecificGameplayBattleground = true;
            }

            GetController<PlayerController>().InitializePlayer(0);

            PlayerMoves = new PlayerMoveAction();

            switch (_matchManager.MatchType)
            {
                case Enumerators.MatchType.LOCAL:
                    GetController<AIController>().InitializePlayer(1);
                    break;
                case Enumerators.MatchType.PVP:
                    GetController<OpponentController>().InitializePlayer(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_matchManager.MatchType), _matchManager.MatchType, null);
            }

            GetController<SkillsController>().InitializeSkills();
            GetController<BattlegroundController>().InitializeBattleground();

            UnityEngine.Debug.Log(IsTutorial + " IsTutorial");

            if (IsTutorial)
            {
                CurrentTurnPlayer = _tutorialManager.CurrentTutorial.PlayerTurnFirst ? CurrentPlayer : OpponentPlayer;

                GetController<PlayerController>().SetHand();
                GetController<CardsController>().StartCardDistribution();
            }
            else
            {
                IsSpecificGameplayBattleground = false;

                switch (_matchManager.MatchType)
                {
                    case Enumerators.MatchType.LOCAL:
                        switch (StartingTurn)
                        {
                            case Enumerators.StartingTurn.UnDecided:
                                CurrentTurnPlayer = Random.Range(0, 100) > 50 ? CurrentPlayer : OpponentPlayer;
                                StartingTurn = CurrentTurnPlayer == CurrentPlayer ?
                                    Enumerators.StartingTurn.Player : Enumerators.StartingTurn.Enemy;
                                break;
                            case Enumerators.StartingTurn.Player:
                                CurrentTurnPlayer = CurrentPlayer;
                                break;
                            case Enumerators.StartingTurn.Enemy:
                                CurrentTurnPlayer = OpponentPlayer;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        OpponentPlayer.SetFirstHandForLocalMatch(false);
                        break;
                    case Enumerators.MatchType.PVP:
                        CurrentTurnPlayer = GameClient.Get<IPvPManager>().IsCurrentPlayer() ? CurrentPlayer : OpponentPlayer;
                        List<WorkingCard> opponentCardsInHand =
                            OpponentPlayer.PvPPlayerState.CardsInHand
                                .Select(instance => _pvpManager.GetWorkingCardFromCardInstance(instance, OpponentPlayer))
                                .ToList();

                        OpponentPlayer.SetFirstHandForPvPMatch(opponentCardsInHand);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_matchManager.MatchType), _matchManager.MatchType, null);
                }

                _uiManager.DrawPopup<PlayerOrderPopup>(new object[]
                {
                    CurrentPlayer.SelfHero, OpponentPlayer.SelfHero
                });
            }

            IsGameEnded = false;

            GameInitialized?.Invoke();
        }
    }
}
