using System;
using System.Collections.Generic;
using LoomNetwork.CZB.BackendCommunication;
using LoomNetwork.CZB.Common;
using Random = UnityEngine.Random;

namespace LoomNetwork.CZB
{
    public class GameplayManager : IService, IGameplayManager
    {
        private IDataManager _dataManager;

        private IMatchManager _matchManager;

        private ISoundManager _soundManager;

        private IUIManager _uiManager;

        private ITimerManager _timerManager;

        private List<IController> _controllers;

        private ActionLogCollectorUploader ActionLogCollectorUploader { get; } = new ActionLogCollectorUploader();

        public event Action OnGameStartedEvent;

        public event Action OnGameInitializedEvent;

        public event Action<Enumerators.EndGameType> OnGameEndedEvent;

        public event Action OnTurnStartedEvent;

        public event Action OnTurnEndedEvent;

        public int PlayerDeckId { get; set; }

        public int OpponentDeckId { get; set; }

        public bool GameStarted { get; set; }

        public bool GameEnded { get; set; }

        public bool IsTutorial { get; set; }

        public bool IsPrepairingEnded { get; set; }

        public int TutorialStep { get; set; }

        public Player CurrentTurnPlayer { get; set; }

        public Player CurrentPlayer { get; set; }

        public Player OpponentPlayer { get; set; }

        public T GetController<T>()
            where T : IController
        {
            return (T)_controllers.Find(x => x is T);
        }

        public void RearrangeHands()
        {
            GetController<BattlegroundController>().UpdatePositionOfBoardUnitsOfPlayer(CurrentPlayer.BoardCards);
            GetController<BattlegroundController>().UpdatePositionOfBoardUnitsOfOpponent();
            GetController<BattlegroundController>().UpdatePositionOfCardsInPlayerHand();
            GetController<BattlegroundController>().UpdatePositionOfCardsInOpponentHand();
        }

        public void EndGame(Enumerators.EndGameType endGameType, float timer = 4f)
        {
            if (GameEnded)
                return;

            GameEnded = true;

            _soundManager.PlaySound(Enumerators.SoundType.BACKGROUND, 128, Constants.BackgroundSoundVolume, null, true);

            if (endGameType != Enumerators.EndGameType.CANCEL)
            {
                _timerManager.AddTimer(
                    x =>
                    {
                        if (endGameType == Enumerators.EndGameType.WIN)
                        {
                            _uiManager.DrawPopup<YouWonPopup>();
                        }
                        else if (endGameType == Enumerators.EndGameType.LOSE)
                        {
                            _uiManager.DrawPopup<YouLosePopup>();
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

            OnGameEndedEvent?.Invoke(endGameType);
        }

        public void StartGameplay()
        {
            _uiManager.DrawPopup<PreparingForBattlePopup>();

            _timerManager.AddTimer(
                x =>
                {
                    _uiManager.HidePopup<PreparingForBattlePopup>();

                    GameStarted = true;
                    GameEnded = false;
                    IsPrepairingEnded = false;

                    OnGameStartedEvent?.Invoke();

                    StartInitializeGame();
                },
                null,
                2f);
        }

        public void StopGameplay()
        {
            GameStarted = false;
            GameEnded = true;
            IsPrepairingEnded = false;
        }

        public bool IsLocalPlayerTurn()
        {
            return CurrentTurnPlayer.Equals(CurrentPlayer);
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
            return !GameEnded && GameStarted && IsPrepairingEnded;
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
                new AiController(),
                new CardsController(),
                new BattlegroundController(),
                new AnimationsController(),
                new BattleController(),
                new BoardArrowController(),
                new SkillsController(),
                new RanksController(),
                new InputController()
            };

            foreach (IController controller in _controllers)
            {
                controller.Init();
            }
        }

        private void StartInitializeGame()
        {
            // initialize players
            GetController<PlayerController>().InitializePlayer();

            if (_matchManager.MatchType == Enumerators.MatchType.LOCAL)
            {
                GetController<AiController>().InitializePlayer();
            }

            GetController<SkillsController>().InitializeSkills();
            GetController<BattlegroundController>().InitializeBattleground();

            if (!IsTutorial)
            {
                CurrentTurnPlayer = Random.Range(0, 100) > 50?CurrentPlayer:OpponentPlayer;
            }
            else
            {
                CurrentTurnPlayer = CurrentPlayer;
            }

            OpponentPlayer.SetFirstHand(IsTutorial);

            if (!IsTutorial)
            {
                _uiManager.DrawPopup<PlayerOrderPopup>(new object[] { CurrentPlayer.SelfHero, OpponentPlayer.SelfHero });
            }
            else
            {
                GetController<PlayerController>().SetHand();
                GetController<CardsController>().StartCardDistribution();
            }

            GameEnded = false;

            OnGameInitializedEvent?.Invoke();
        }
    }
}
