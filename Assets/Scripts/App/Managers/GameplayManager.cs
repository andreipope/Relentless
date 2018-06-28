using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class GameplayManager : IService, IGameplayManager
    {
        public event Action OnGameStartedEvent;
        public event Action OnGameInitializedEvent;
        public event Action OnGameEndedEvent;
        public event Action OnTurnStartedEvent;
        public event Action OnTurnEndedEvent;

        private IDataManager _dataManager;
        private IMatchManager _matchManager;
        private ISoundManager _soundManager;
        private IUIManager _uiManager;

        private List<IController> _controllers;

        public int PlayerDeckId { get; set; }
        public int OpponentDeckId { get; set; }

        public bool GameStarted { get; set; }
        public bool IsTutorial { get; set; }

        public int TurnDuration { get; set; }
        public int CurrentTurn { get; set; }

        public int TutorialStep { get; set; }

        public List<Player> PlayersInGame { get; set; }

        public Player WhoseTurn { get; set; }

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
            GetController<BattlegroundController>().RearrangeBottomBoard();
            GetController<BattlegroundController>().RearrangeTopBoard();
        }

        public void EndGame(Enumerators.EndGameType endGameType)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.BACKGROUND, 128, Constants.BACKGROUND_SOUND_VOLUME, null, true);

            if (endGameType == Enumerators.EndGameType.WIN)
                GameObject.Find("Opponent/Avatar").GetComponent<PlayerAvatar>().OnAvatarDie();
            else if (endGameType == Enumerators.EndGameType.LOSE)
                GameObject.Find("Player/Avatar").GetComponent<PlayerAvatar>().OnAvatarDie();

            GameClient.Get<ITimerManager>().AddTimer((x) =>
            {
                if (endGameType == Enumerators.EndGameType.WIN)
                    _uiManager.DrawPopup<YouWonPopup>();
                else if (endGameType == Enumerators.EndGameType.LOSE)
                    _uiManager.DrawPopup<YouLosePopup>();
            }, null, 4f);

            _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);
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

            OnGameEndedEvent?.Invoke();
        }


        public void EndTurn()
        {
            GameClient.Get<ITutorialManager>().ReportAction(Enumerators.TutorialReportAction.END_TURN);
        }

        public Player GetLocalPlayer()
        {
            return PlayersInGame.Find(x => x.IsLocalPlayer);
        }

        public Player GetOpponentPlayer()
        {
            return PlayersInGame.Find(x => !x.IsLocalPlayer);
        }

        public bool IsLocalPlayerTurn()
        {
            return WhoseTurn.Equals(GetLocalPlayer());
        }

        private void StartInitializeGame()
        {
            PlayersInGame.Clear();

            //initialize players
            GetController<PlayerController>().InitializePlayer();

            if (_matchManager.MatchType == Enumerators.MatchType.LOCAL)
                GetController<AIController>().InitializePlayer();

            WhoseTurn = GetLocalPlayer();// local player starts as first


            GetController<BattlegroundController>().InitializeBattleground();

            OnGameInitializedEvent?.Invoke();
        }
    }
}