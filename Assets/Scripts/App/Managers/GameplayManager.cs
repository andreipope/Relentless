using CCGKit;
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
        private List<IController> _controllers;

        public int PlayerDeckId { get; set; }
        public int OpponentDeckId { get; set; }


        public Player CurrentPlayerOwnerOfTurn { get; set; }

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
            _controllers.Add(new ActionsQueueController());
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
            (NetworkingUtils.GetHumanLocalPlayer() as DemoHumanPlayer).RearrangeBottomBoard();
            (NetworkingUtils.GetHumanLocalPlayer() as DemoHumanPlayer).RearrangeTopBoard();
        }
    }
}
