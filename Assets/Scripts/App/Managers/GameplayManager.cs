// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;

namespace LoomNetwork.CZB
{
    public class GameplayManager : IService, IGameplayManager
    {
        public event Action OnGameStartedEvent;
        public event Action OnGameInitializedEvent;
        public event Action<Enumerators.EndGameType> OnGameEndedEvent;
        public event Action OnTurnStartedEvent;
        public event Action OnTurnEndedEvent;

        private IDataManager _dataManager;
        private IMatchManager _matchManager;
        private ISoundManager _soundManager;
        private IUIManager _uiManager;
        private ITimerManager _timerManager;
        private ITutorialManager _tutorialManager;

        private List<IController> _controllers;

        public int PlayerDeckId { get; set; }
        public int OpponentDeckId { get; set; }

        public bool GameStarted { get; set; }
        public bool GameEnded { get; set; }
        public bool IsTutorial { get; set; }

        public int TurnDuration { get; set; }
        public int CurrentTurn { get; set; }

        public int TutorialStep { get; set; }

        public List<Player> PlayersInGame { get; set; }

        public Player CurrentTurnPlayer { get; set; }

        public Player CurrentPlayer { get; set; }
        public Player OpponentPlayer { get; set; }

        public void Dispose()
        {
            foreach (var item in _controllers)
                item.Dispose();
        }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _matchManager = GameClient.Get<IMatchManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            PlayersInGame = new List<Player>();

            InitControllers();

            if (!_dataManager.CachedUserLocalData.tutorial)
            {
                Constants.ZOMBIES_SOUND_VOLUME *= 3;
                Constants.CREATURE_ATTACK_SOUND_VOLUME *= 3;
            }
        }

        public void Update()
        {
            foreach (var item in _controllers)
                item.Update();
        }

        public T GetController<T>() where T : IController
        {
            return (T)_controllers.Find(x => x is T);
        }

        private void InitControllers()
        {
            _controllers = new List<IController>();
            _controllers.Add(new VFXController());
            _controllers.Add(new ParticlesController());
            _controllers.Add(new AbilitiesController());
            _controllers.Add(new ActionsQueueController());
            _controllers.Add(new PlayerController());
            _controllers.Add(new AIController());
            _controllers.Add(new CardsController());
            _controllers.Add(new BattlegroundController());
            _controllers.Add(new AnimationsController());
            _controllers.Add(new BattleController());
            _controllers.Add(new BoardArrowController());

            foreach (var controller in _controllers)
                controller.Init();
        }

        public string GetCardSet(Data.Card card)
        {
            foreach (var cardSet in _dataManager.CachedCardsLibraryData.sets)
            {
                if (cardSet.cards.IndexOf(card) > -1)
                    return cardSet.name;
            }

            return string.Empty;
        }

        public void RearrangeHands()
        {
            GetController<BattlegroundController>().UpdatePositionOfBoardUnitsOfPlayer();
            GetController<BattlegroundController>().UpdatePositionOfBoardUnitsOfOpponent();
        }

        public void EndGame(Enumerators.EndGameType endGameType)
        {
            if (GameEnded)
                return;

            GameEnded = true;

            _soundManager.PlaySound(Enumerators.SoundType.BACKGROUND, 128, Constants.BACKGROUND_SOUND_VOLUME, null, true);

            if (endGameType != Enumerators.EndGameType.CANCEL)
            {
                _timerManager.AddTimer((x) =>
                {
                    if (endGameType == Enumerators.EndGameType.WIN)
                        _uiManager.DrawPopup<YouWonPopup>();
                    else if (endGameType == Enumerators.EndGameType.LOSE)
                        _uiManager.DrawPopup<YouLosePopup>();
                }, null, 4f);
            }

            _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);

            StopGameplay();

            OnGameEndedEvent?.Invoke(endGameType);
        }

        public void StartGameplay()
        {
            GameStarted = true;

            OnGameStartedEvent?.Invoke();

            StartInitializeGame();
        }

        public void StopGameplay()
        {
            GameStarted = false;
            GameEnded = true;
        }

        public bool IsLocalPlayerTurn()
        {
            return CurrentTurnPlayer.Equals(CurrentPlayer);
        }

        private void StartInitializeGame()
        {
            PlayersInGame.Clear();

            //initialize players
            GetController<PlayerController>().InitializePlayer();

            if (_matchManager.MatchType == Enumerators.MatchType.LOCAL)
                GetController<AIController>().InitializePlayer();

            CurrentTurnPlayer = CurrentPlayer;// local player starts as first


            GetController<BattlegroundController>().InitializeBattleground();

            GameEnded = false;

            OnGameInitializedEvent?.Invoke();
        }
    }
}