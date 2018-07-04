// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public class RanksController : IController
    {
        private IGameplayManager _gameplayManager;

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
        }

        public void Update()
        {
        }

        public void UpdateRanksBuffs()
        {
            UpdateRanksBuffs(_gameplayManager.CurrentTurnPlayer);
            UpdateRanksBuffs(_gameplayManager.OpponentPlayer);
        }

        public void UpdateRanksBuffs(Player player)
        {
            Enumerators.CardRank highestRank = Enumerators.CardRank.MINION;
            foreach (var unit in player.BoardCards)
            {
                Debug.Log(unit.Card.libraryCard.cardRank);
                if ((int)unit.Card.libraryCard.cardRank > (int)highestRank)
                    highestRank = unit.Card.libraryCard.cardRank;
            }
        }
    }
}