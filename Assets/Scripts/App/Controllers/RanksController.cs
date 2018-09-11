using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class RanksController : IController
    {
        private ITutorialManager _tutorialManager;

        public void Dispose()
        {
        }

        public void Init()
        {
            _tutorialManager = GameClient.Get<ITutorialManager>();

        }

        public void Update()
        {
        }

        public void ResetAll()
        {
        }

        public void UpdateRanksByElements(List<BoardUnit> units, Card card)
        {
            List<BoardUnit> filter = units.Where(unit =>
                unit.Card.LibraryCard.CardSetType == card.CardSetType &&
                (int) unit.Card.LibraryCard.CardRank < (int) card.CardRank).ToList();
            if (filter.Count > 0)
            {
                DoRankUpgrades(filter, card.CardSetType, card.CardRank);
            }
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
        }

        private void AirRankBuff(List<BoardUnit> units, Enumerators.CardRank rank)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.GUARD);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.GUARD);
                    buffs.Add(Enumerators.BuffType.DEFENCE);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.GUARD);
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
                    buffs.Add(Enumerators.BuffType.REANIMATE);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.REANIMATE);
                    buffs.Add(Enumerators.BuffType.DEFENCE);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.REANIMATE);
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
                    buffs.Add(Enumerators.BuffType.DESTROY);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.DESTROY);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.DESTROY);
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

        private void BuffRandomAlly(List<BoardUnit> units, int count, List<Enumerators.BuffType> buffTypes)
        {
            if(_tutorialManager.IsTutorial)
            {
                // need for attacking by Poizom's
                units = units.FindAll(x => x.UnitCanBeUsable());
            }

            units = InternalTools.GetRandomElementsFromList(units, count);

            foreach (Enumerators.BuffType buff in buffTypes)
            {
                foreach (BoardUnit unit in units)
                {
                    unit.ApplyBuff(buff);
                }
            }
        }
    }
}
