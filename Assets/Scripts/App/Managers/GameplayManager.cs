using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
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

        private List<IController> _controllers;

        private ActionLogCollectorUploader ActionLogCollectorUploader { get; } = new ActionLogCollectorUploader();

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

        public int TutorialStep { get; set; }

        public Player CurrentTurnPlayer { get; set; }

        public Player CurrentPlayer { get; set; }

        public Player OpponentPlayer { get; set; }

        public bool CanDoDragActions { get; set; }

        public T GetController<T>()
            where T : IController
        {
            return (T) _controllers.Find(x => x is T);
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
            if (IsGameEnded)
                return;

            IsGameEnded = true;

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
                GetController<AIController>().InitializePlayer();
            }

            GetController<SkillsController>().InitializeSkills();
            GetController<BattlegroundController>().InitializeBattleground();

            if (!IsTutorial)
            {
                CurrentTurnPlayer = Random.Range(0, 100) > 50 ? CurrentPlayer : OpponentPlayer;
            }
            else
            {
                CurrentTurnPlayer = CurrentPlayer;
            }

            OpponentPlayer.SetFirstHand(IsTutorial);

            if (!IsTutorial)
            {
                _uiManager.DrawPopup<PlayerOrderPopup>(new object[]
                {
                    CurrentPlayer.SelfHero, OpponentPlayer.SelfHero
                });
            }
            else
            {
                GetController<PlayerController>().SetHand();
                GetController<CardsController>().StartCardDistribution();
            }

            IsGameEnded = false;

            GameInitialized?.Invoke();
        }
    }
}
