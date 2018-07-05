// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            for (int i = 0; i < 6; i++)
                UpdateRanksByElements(player.BoardCards, (Enumerators.SetType)i);
        }

        private void UpdateRanksByElements(List<BoardUnit> units, Enumerators.SetType element)
        {
            var elementFilter = units.Where((unit) => unit.Card.libraryCard.cardSetType == element).ToList();
            Enumerators.CardRank highestRank = Enumerators.CardRank.MINION;
            foreach (var unit in elementFilter)
            {
                if ((int)unit.Card.libraryCard.cardRank > (int)highestRank)
                    highestRank = unit.Card.libraryCard.cardRank;
            }
           // Debug.Log(highestRank + "  | " + element);

            var weakerUnitsList = elementFilter.Where((unit) => unit.Card.libraryCard.cardRank != highestRank).ToList();
            DoRankUpgrades(weakerUnitsList, element, highestRank);
        }

        public void DoRankUpgrades(List<BoardUnit> units, Enumerators.SetType element, Enumerators.CardRank rank)
        {
          //  Debug.Log(units.Count + " | " + rank);

            switch (element)
            {
                case Enumerators.SetType.AIR:
                    AirRankBuff(units, rank);
                    break;
                case Enumerators.SetType.EARTH:
                    EarthRankBuff(units, rank);
                    break;
                case Enumerators.SetType.WATER:
                    WaterRankBuff(units, rank);
                    break;
                case Enumerators.SetType.FIRE:
                    FireRankBuff(units, rank);
                    break;
                case Enumerators.SetType.TOXIC:
                    ToxicRankBuff(units, rank);
                    break;
                case Enumerators.SetType.LIFE:
                    LifeRankBuff(units, rank);
                    break;
            }
        }

        private void ToxicRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            foreach (var unit in units)
            {
                switch (rank)
                {
                    case Enumerators.CardRank.OFFICER:
                        unit.Damage += 1;
                        break;
                    case Enumerators.CardRank.COMMANDER:
                        unit.Damage += 2;
                        break;
                    case Enumerators.CardRank.GENERAL:
                        unit.Damage += 3;
                        break;
                }
            }
        }

        private void AirRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            foreach (var unit in units)
            {
                switch (rank)
                {
                    case Enumerators.CardRank.OFFICER:
                        break;
                    case Enumerators.CardRank.COMMANDER:
                        unit.HP++;
                        break;
                    case Enumerators.CardRank.GENERAL:
                        unit.HP += 2;
                        break;
                }
            }
        }

        private void EarthRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            foreach (var unit in units)
            {
                switch(rank)
                {
                    case Enumerators.CardRank.OFFICER:
                        unit.HP++;
                        break;
                    case Enumerators.CardRank.COMMANDER:
                        unit.HP++;
                        unit.hasProvoke = true;
                        break;
                    case Enumerators.CardRank.GENERAL:
                        unit.HP += 2;
                        unit.hasProvoke = true;
                        break;
                }
            }
        }

        private void FireRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            foreach (var unit in units)
            {
                switch (rank)
                {
                    case Enumerators.CardRank.OFFICER:
                        unit.hasImpetus = true;
                        break;
                    case Enumerators.CardRank.COMMANDER:
                        unit.Damage++;
                        unit.hasImpetus = true;
                        break;
                    case Enumerators.CardRank.GENERAL:
                        unit.Damage += 2;
                        unit.hasImpetus = true;
                        break;
                }
            }
        }

        private void LifeRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            int cycles = (int)rank;
            if (cycles > units.Count)
                cycles = units.Count;

            BoardUnit unit;
            int random;
            for (int i = 0; i < cycles; i++)
            {
                random = Random.Range(0, units.Count);
                unit = units[random];
                units.RemoveAt(random);

                if (unit.HP < unit.initialHP)
                    unit.HP++;
            }
        }
        private void WaterRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
           
        }
    }
}