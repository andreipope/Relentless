using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class RanksController : IController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(RanksController));

        public event Action<BoardUnitModel, List<BoardUnitView>> RanksUpdated;

        private ITutorialManager _tutorialManager;
        private IGameplayManager _gameplayManager;

        private Action _ranksUpgradeCompleteAction;

        private List<BoardUnitView> _unitsForIgnoreRankBuff;

        public void Dispose()
        {
        }

        public void Init()
        {
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _unitsForIgnoreRankBuff = new List<BoardUnitView>();
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
        }

        public void UpdateRanksByElements(IReadOnlyList<BoardUnitView> units, BoardUnitModel boardUnitModel, GameplayQueueAction<object> actionInQueue)
        {
            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
            {
                if (!boardUnitModel.Owner.IsLocalPlayer)
                    return;
            }

            actionInQueue.Action = (parameter, completeCallback) =>
                   {
                       _ranksUpgradeCompleteAction = completeCallback;

                       List<BoardUnitView> filter = units.Where(unit =>
                                    unit.Model.Card.Prototype.CardSetType == boardUnitModel.Prototype.CardSetType &&
                                    (int)unit.Model.Card.Prototype.CardRank < (int)boardUnitModel.Prototype.CardRank &&
                                    !unit.WasDestroyed && !unit.Model.IsDead &&
                                    !_unitsForIgnoreRankBuff.Contains(unit))
                                    .ToList();

                       _unitsForIgnoreRankBuff.Clear();

                       if (filter.Count > 0 && (!_tutorialManager.IsTutorial ||
                           (_tutorialManager.IsTutorial &&
                           _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.RankSystemHasEnabled)))
                       {
                           DoRankUpgrades(filter, boardUnitModel);

                           GameClient.Get<IOverlordExperienceManager>().ReportExperienceAction(filter[0].Model.OwnerPlayer.SelfHero,
                            Common.Enumerators.ExperienceActionType.ActivateRankAbility);

                           _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.RanksUpdated);
                       }
                       else
                       {
                           _ranksUpgradeCompleteAction?.Invoke();
                           _ranksUpgradeCompleteAction = null;
                       }
                   };
        }

        public void DoRankUpgrades(List<BoardUnitView> units, BoardUnitModel boardUnitModel, bool randomly = true)
        {
            switch (boardUnitModel.Prototype.CardSetType)
            {
                case Enumerators.SetType.AIR:
                    AirRankBuff(units, boardUnitModel.Prototype.CardRank, boardUnitModel, randomly);
                    break;
                case Enumerators.SetType.EARTH:
                    EarthRankBuff(units, boardUnitModel.Prototype.CardRank, boardUnitModel, randomly);
                    break;
                case Enumerators.SetType.WATER:
                    WaterRankBuff(units, boardUnitModel.Prototype.CardRank, boardUnitModel, randomly);
                    break;
                case Enumerators.SetType.FIRE:
                    FireRankBuff(units, boardUnitModel.Prototype.CardRank, boardUnitModel, randomly);
                    break;
                case Enumerators.SetType.TOXIC:
                    ToxicRankBuff(units, boardUnitModel.Prototype.CardRank, boardUnitModel, randomly);
                    break;
                case Enumerators.SetType.LIFE:
                    LifeRankBuff(units, boardUnitModel.Prototype.CardRank, boardUnitModel, randomly);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(boardUnitModel.Prototype.CardSetType), boardUnitModel.Prototype.CardSetType, null);
            }
        }

        private void AirRankBuff(List<BoardUnitView> units, Enumerators.CardRank rank, BoardUnitModel boardUnitModel, bool randomly = true)
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
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.GUARD);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            BuffRandomAlly(units, count, buffs, boardUnitModel, randomly);
        }

        private void EarthRankBuff(List<BoardUnitView> units, Enumerators.CardRank rank, BoardUnitModel boardUnitModel, bool randomly = true)
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
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.HEAVY);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            BuffRandomAlly(units, count, buffs, boardUnitModel, randomly);
        }

        private void FireRankBuff(List<BoardUnitView> units, Enumerators.CardRank rank, BoardUnitModel boardUnitModel, bool randomly = true)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.BLITZ);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.BLITZ);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.BLITZ);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            BuffRandomAlly(units, count, buffs, boardUnitModel, randomly);
        }

        public void AddUnitForIgnoreRankBuff(BoardUnitView unit)
        {
            if (!_unitsForIgnoreRankBuff.Contains(unit))
            {
                _unitsForIgnoreRankBuff.Add(unit);
            }
        }

        private void LifeRankBuff(List<BoardUnitView> units, Enumerators.CardRank rank, BoardUnitModel boardUnitModel, bool randomly = true)
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
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.REANIMATE);
                    buffs.Add(Enumerators.BuffType.ATTACK);
                    count = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            BuffRandomAlly(units, count, buffs, boardUnitModel, randomly);
        }

        private void ToxicRankBuff(List<BoardUnitView> units, Enumerators.CardRank rank, BoardUnitModel boardUnitModel, bool randomly = true)
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            BuffRandomAlly(units, count, buffs, boardUnitModel, randomly);
        }

        private void WaterRankBuff(List<BoardUnitView> units, Enumerators.CardRank rank, BoardUnitModel boardUnitModel, bool randomly = true)
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

            BuffRandomAlly(units, count, buffs, boardUnitModel, randomly);
        }

        private void BuffRandomAlly(List<BoardUnitView> units, int count,
                                    List<Enumerators.BuffType> buffTypes,
                                    BoardUnitModel boardUnitModel, bool randomly = true)
        {
            if (_tutorialManager.IsTutorial)
            {
                units = units.FindAll(x => x.Model.UnitCanBeUsable());
            }

            if (randomly)
            {
                units = InternalTools.GetRandomElementsFromList(units, count);
            }

            foreach (Enumerators.BuffType buff in buffTypes)
            {
                foreach (BoardUnitView unit in units)
                {
                    if (unit == null || unit.Model == null)
                    {
                        ExceptionReporter.LogExceptionAsWarning(Log, new Exception("Tried to Buff Null Unit in Ranks System"));
                        continue;
                    }

                    unit.Model.ApplyBuff(buff);
                }
            }


            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
            {
                if (!boardUnitModel.Owner.IsLocalPlayer)
                {
                    _ranksUpgradeCompleteAction?.Invoke();
                    _ranksUpgradeCompleteAction = null;
                    return;
                }
            }

            RanksUpdated?.Invoke(boardUnitModel, units);

            _ranksUpgradeCompleteAction?.Invoke();
            _ranksUpgradeCompleteAction = null;
        }

        public void BuffAllyManually(List<BoardUnitView> units, BoardUnitModel card)
        {
            _gameplayManager.GetController<ActionsQueueController>().AddNewActionInToQueue(
                 (parameter, completeCallback) =>
                 {
                     _ranksUpgradeCompleteAction = completeCallback;

                     DoRankUpgrades(units, card, false);
                 }, Enumerators.QueueActionType.RankBuff);
        }
    }
}
