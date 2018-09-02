using System.Collections.Generic;
using System.Linq;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

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

        public void ResetAll()
        {
        }

        public void UpdateRanksByElements(List<BoardUnit> units, Card card)
        {
            List<BoardUnit> filter = units.Where(unit => (unit.Card.LibraryCard.CardSetType == card.CardSetType) && ((int)unit.Card.LibraryCard.CardRank < (int)card.CardRank)).ToList();
            if (filter.Count > 0)
            {
                DoRankUpgrades(filter, card.CardSetType, card.CardRank);
            }
        }

        public void DoRankUpgrades(List<BoardUnit> units, Enumerators.SetType element, Enumerators.CardRank rank)
        {
            switch (element)
            {
                case Enumerators.SetType.Air:
                    AirRankBuff(units, rank);
                    break;
                case Enumerators.SetType.Earth:
                    EarthRankBuff(units, rank);
                    break;
                case Enumerators.SetType.Water:
                    WaterRankBuff(units, rank);
                    break;
                case Enumerators.SetType.Fire:
                    FireRankBuff(units, rank);
                    break;
                case Enumerators.SetType.Toxic:
                    ToxicRankBuff(units, rank);
                    break;
                case Enumerators.SetType.Life:
                    LifeRankBuff(units, rank);
                    break;
            }

            // foreach (var unit in units)
            // unit.ApplyBuffs();
        }

        private void AirRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.Officer:
                    buffs.Add(Enumerators.BuffType.Guard);
                    break;
                case Enumerators.CardRank.Commander:
                    buffs.Add(Enumerators.BuffType.Guard);
                    buffs.Add(Enumerators.BuffType.Defence);
                    count = 2;
                    break;
                case Enumerators.CardRank.General:
                    buffs.Add(Enumerators.BuffType.Guard);
                    buffs.Add(Enumerators.BuffType.Defence);
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
                case Enumerators.CardRank.Officer:
                    buffs.Add(Enumerators.BuffType.Heavy);
                    break;
                case Enumerators.CardRank.Commander:
                    buffs.Add(Enumerators.BuffType.Heavy);
                    buffs.Add(Enumerators.BuffType.Defence);
                    count = 2;
                    break;
                case Enumerators.CardRank.General:
                    buffs.Add(Enumerators.BuffType.Heavy);
                    buffs.Add(Enumerators.BuffType.Defence);
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
                case Enumerators.CardRank.Officer:
                    buffs.Add(Enumerators.BuffType.Rush);
                    break;
                case Enumerators.CardRank.Commander:
                    buffs.Add(Enumerators.BuffType.Rush);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 2;
                    break;
                case Enumerators.CardRank.General:
                    buffs.Add(Enumerators.BuffType.Rush);
                    buffs.Add(Enumerators.BuffType.Attack);
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
                case Enumerators.CardRank.Officer:
                    buffs.Add(Enumerators.BuffType.Reanimate);
                    break;
                case Enumerators.CardRank.Commander:
                    buffs.Add(Enumerators.BuffType.Reanimate);
                    buffs.Add(Enumerators.BuffType.Defence);
                    count = 2;
                    break;
                case Enumerators.CardRank.General:
                    buffs.Add(Enumerators.BuffType.Reanimate);
                    buffs.Add(Enumerators.BuffType.Defence);
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
                case Enumerators.CardRank.Officer:
                    buffs.Add(Enumerators.BuffType.Destroy);
                    break;
                case Enumerators.CardRank.Commander:
                    buffs.Add(Enumerators.BuffType.Destroy);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 2;
                    break;
                case Enumerators.CardRank.General:
                    buffs.Add(Enumerators.BuffType.Destroy);
                    buffs.Add(Enumerators.BuffType.Attack);
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
                case Enumerators.CardRank.Officer:
                    buffs.Add(Enumerators.BuffType.Freeze);
                    break;
                case Enumerators.CardRank.Commander:
                    buffs.Add(Enumerators.BuffType.Freeze);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 2;
                    break;
                case Enumerators.CardRank.General:
                    buffs.Add(Enumerators.BuffType.Freeze);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 3;
                    break;
            }

            BuffRandomAlly(units, count, buffs);
        }

        private void BuffHorde(List<BoardUnit> units, Enumerators.BuffType buffType)
        {
            foreach (BoardUnit unit in units)
            {
                Debug.Log(unit.Card.LibraryCard.Name);

                unit.BuffUnit(buffType);
            }
        }

        private void BuffRandomAlly(List<BoardUnit> units, int count, List<Enumerators.BuffType> buffTypes)
        {
            int random;
            for (int i = 0; i < count; i++)
            {
                if (units.Count == 0)
                {
                    break;
                }

                random = Random.Range(0, units.Count);

                foreach (Enumerators.BuffType buff in buffTypes)
                {
                    // units[random].BuffUnit(buffs);
                    units[random].ApplyBuff(buff);
                }

                units.RemoveAt(random);
            }
        }
    }
}
