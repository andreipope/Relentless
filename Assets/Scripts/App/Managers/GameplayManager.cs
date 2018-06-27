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
        public event Action OnGameEndedEvent;
        public event Action OnTurnStartedEvent;
        public event Action OnTurnEndedEvent;

        private List<IController> _controllers;

        public int PlayerDeckId { get; set; }
        public int OpponentDeckId { get; set; }

        public bool GameStarted { get; set; }
        public bool IsTutorial { get; set; }

        public int TurnDuration { get; set; }
        public int CurrentTurn { get; set; }

        public int TutorialStep { get; set; }

        public List<Player> PlayersInGame { get; set; }

        public void Dispose()
        {
            foreach (var item in _controllers)
                item.Dispose();
        }

        public void Init()
        {
            InitControllers();

            if(!GameClient.Get<IDataManager>().CachedUserLocalData.tutorial)
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
            _controllers.Add(new AbilitiesController());
            _controllers.Add(new ParticlesController());
            _controllers.Add(new PlayerController());
            _controllers.Add(new ActionsQueueController());
            _controllers.Add(new BattlegroundController());
            _controllers.Add(new VFXController());  
        }

        public string GetCardSet(Data.Card card)
        {
            foreach (var cardSet in GameClient.Get<IDataManager>().CachedCardsLibraryData.sets)
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

        public void StartGameplay()
        {
            GameStarted = true;
        }

        public void StopGameplay()
        {
            GameStarted = false;
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
    }
}
