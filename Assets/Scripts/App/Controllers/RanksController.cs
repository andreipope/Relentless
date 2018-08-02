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

        public void UpdateRanksBuffs(Player player, Enumerators.CardRank rank = Enumerators.CardRank.MINION)
        {
            //foreach (var unit in player.BoardCards)
            //    unit.ClearBuffs();

            UpdateRanksByElements(player.BoardCards, player.SelfHero.heroElement, rank);
            //for (int i = 0; i < 6; i++)
            //  UpdateRanksByElements(player.BoardCards, (Enumerators.SetType)i, rank);
        }

        private void UpdateRanksByElements(List<BoardUnit> units, Enumerators.SetType element, Enumerators.CardRank highestRank)
        {
            var elementFilter = units.Where((unit) => unit.Card.libraryCard.cardSetType == element && (int)unit.Card.libraryCard.cardRank < (int)highestRank).ToList();
            var weakerUnitsList = elementFilter.Where((unit) => (int)unit.Card.libraryCard.cardRank < (int)highestRank).ToList();
            if(weakerUnitsList.Count > 0)
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
           
            //foreach (var unit in units)
             //   unit.ApplyBuffs();
        }

        private void AirRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            List<Enumerators.BuffType> buffs =  new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.SHIELD);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.SHIELD);
                    buffs.Add(Enumerators.BuffType.DEFENCE);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.SHIELD);
                    buffs.Add(Enumerators.BuffType.DEFENCE);
                    count = 3;
                    break;
            }
            BuffRandomAlly(units, count, buffs);
        }

        private void EarthRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.HEAVY);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.HEAVY);
                    buffs.Add(Enumerators.BuffType.DEFENCE);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.HEAVY);
                    buffs.Add(Enumerators.BuffType.DEFENCE);
                    count = 3;
                    break;
            }
            BuffRandomAlly(units, count, buffs);
        }

        private void FireRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.RUSH);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.RUSH);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.RUSH);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 3;
                    break;
            }
            BuffRandomAlly(units, count, buffs);
        }

        private void LifeRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.REANIMATE_UNIT);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.REANIMATE_UNIT);
                    buffs.Add(Enumerators.BuffType.DEFENCE);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.REANIMATE_UNIT);
                    buffs.Add(Enumerators.BuffType.DEFENCE);
                    count = 3;
                    break;
            }
            BuffRandomAlly(units, count, buffs);
        }

        private void ToxicRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.DESTROY_TARGET_UNIT_AFTER_ATTACK);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.DESTROY_TARGET_UNIT_AFTER_ATTACK);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.DESTROY_TARGET_UNIT_AFTER_ATTACK);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 3;
                    break;
            }
            BuffRandomAlly(units, count, buffs);
        }

        private void WaterRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.FREEZE);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.FREEZE);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.FREEZE);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 3;
                    break;
            }
            BuffRandomAlly(units, count, buffs);
        }

        private void BuffHorde(List<BoardUnit> units, Enumerators.BuffType buffType)
        {
            foreach (var unit in units)
            {
                Debug.Log(unit.Card.libraryCard.name);

                unit.BuffUnit(buffType);
            }
        }

        private void BuffRandomAlly(List<BoardUnit> units, int count, List<Enumerators.BuffType> buffTypes)
        {
            int random;
            for (int i = 0; i < count; i++)
            {
                if (units.Count == 0)
                    break;
                random = Random.Range(0, units.Count);

                foreach (Enumerators.BuffType buff in buffTypes)
                {
                    //units[random].BuffUnit(buffs);
                    units[random].ApplyBuff(buff);
                }
                units.RemoveAt(random);
                
            }
        }
    }
}