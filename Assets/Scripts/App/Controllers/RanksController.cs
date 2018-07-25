// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
            foreach (var unit in player.BoardCards)
                unit.ClearBuffs();

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

            var weakerUnitsList = elementFilter.Where((unit) => unit.Card.libraryCard.cardRank != highestRank).ToList();
            DoRankUpgrades(weakerUnitsList, element, highestRank);
        }

        public void DoRankUpgrades(List<BoardUnit> units, Enumerators.SetType element, Enumerators.CardRank rank)
        {
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

            foreach (var unit in units)
                unit.ApplyBuffs();
        }

        private void ToxicRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            foreach (var unit in units)
            {
                for (int i = 0; i < (int)rank; i++)
                    unit.BuffUnit(Enumerators.BuffType.ATTACK);
            }
        }

        private void AirRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            foreach (var unit in units)
            {
                for (int i = 0; i < (int)rank; i++)
                    unit.BuffUnit(Enumerators.BuffType.SHIELD);

                switch (rank)
                {
                    case Enumerators.CardRank.OFFICER:
                        break;
                    case Enumerators.CardRank.COMMANDER:
                        unit.BuffUnit(Enumerators.BuffType.DEFENCE);
                        break;
                    case Enumerators.CardRank.GENERAL:
                        unit.BuffUnit(Enumerators.BuffType.DEFENCE);
                        unit.BuffUnit(Enumerators.BuffType.DEFENCE);
                        break;
                }
            }
        }

        private void EarthRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            foreach (var unit in units)
            {
                for (int i = 0; i < (int)rank; i++)
                    unit.BuffUnit(Enumerators.BuffType.DEFENCE);

                switch (rank)
                {
                    case Enumerators.CardRank.OFFICER:
                        break;
                    case Enumerators.CardRank.COMMANDER:
                        unit.BuffUnit(Enumerators.BuffType.HEAVY);
                        break;
                    case Enumerators.CardRank.GENERAL:
                        unit.BuffUnit(Enumerators.BuffType.DEFENCE);
                        unit.BuffUnit(Enumerators.BuffType.HEAVY);
                        break;
                }
            }
        }

        private void FireRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            foreach (var unit in units)
            {
                for (int i = 0; i < (int)rank; i++)
                    unit.BuffUnit(Enumerators.BuffType.RUSH);

                switch (rank)
                {
                    case Enumerators.CardRank.OFFICER:
                        break;
                    case Enumerators.CardRank.COMMANDER:
                        unit.BuffUnit(Enumerators.BuffType.ATTACK);
                        break;
                    case Enumerators.CardRank.GENERAL:
                        unit.BuffUnit(Enumerators.BuffType.ATTACK);
                        unit.BuffUnit(Enumerators.BuffType.ATTACK);
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

                if (unit.CurrentHP < unit.MaxCurrentHP)
                    unit.CurrentHP++;
            }
        }

        private void WaterRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            foreach (var unit in units)
            {
                for (int i = 0; i < (int)rank; i++)
                    unit.BuffUnit(Enumerators.BuffType.FREEZE);

                switch (rank)
                {
                    case Enumerators.CardRank.OFFICER:
                        break;
                    case Enumerators.CardRank.COMMANDER:
                        unit.BuffUnit(Enumerators.BuffType.DAMAGE);
                        break;
                    case Enumerators.CardRank.GENERAL:
                        unit.BuffUnit(Enumerators.BuffType.DAMAGE);
                        unit.BuffUnit(Enumerators.BuffType.DAMAGE);
                        break;
                }
            }
        }
    }
}