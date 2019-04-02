using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class RanksController : IController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(RanksController));

        public delegate void RanksUpdatedDelegate(BoardUnitModel originUnit, IReadOnlyList<BoardUnitModel> targetUnits);
        public event RanksUpdatedDelegate RanksUpdated;

        private ITutorialManager _tutorialManager;
        private IGameplayManager _gameplayManager;
        private ILoadObjectsManager _loadObjectsManager;
        private IDataManager _dataManager;
        private BattlegroundController _battlegroundController;

        private Action _ranksUpgradeCompleteAction;

        private List<BoardUnitModel> _unitsForIgnoreRankBuff;

        public RankBuffsData RankBuffsData { get; private set; }

        public void Dispose()
        {
        }

        public void Init()
        {
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _unitsForIgnoreRankBuff = new List<BoardUnitModel>();

            RankBuffsData = _dataManager.DeserializeFromJson<RankBuffsData>(
                                        _loadObjectsManager.GetObjectByPath<TextAsset>("rank_buffs_data").text);
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
        }

        public void UpdateRanksByElements(IReadOnlyList<BoardUnitModel> units, BoardUnitModel boardUnitModel, GameplayQueueAction<object> actionInQueue)
        {
            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
            {
                if (!boardUnitModel.Owner.IsLocalPlayer)
                    return;
            }

            actionInQueue.Action = (parameter, completeCallback) =>
                   {
                       _ranksUpgradeCompleteAction = completeCallback;

                       List<BoardUnitModel> filter = units.Where(unit =>
                                    unit.Card.Prototype.Faction == boardUnitModel.Prototype.Faction &&
                                    (int)unit.Card.Prototype.Rank < (int)boardUnitModel.Prototype.Rank &&
                                    !_battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit).WasDestroyed &&
                                    !unit.IsDead &&
                                    !_unitsForIgnoreRankBuff.Contains(unit) &&
                                      unit.Card.Prototype.Faction != Enumerators.Faction.ITEM &&
                                      unit.Card.Prototype.Kind == Enumerators.CardKind.CREATURE)
                                    .ToList();

                       _unitsForIgnoreRankBuff.Clear();

                       if (filter.Count > 0 && (!_tutorialManager.IsTutorial ||
                           (_tutorialManager.IsTutorial &&
                           _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.RankSystemHasEnabled)))
                       {
                           DoRankUpgrades(filter, boardUnitModel);

                           GameClient.Get<IOverlordExperienceManager>().ReportExperienceAction(filter[0].OwnerPlayer.SelfOverlord,
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

        public void DoRankUpgrades(List<BoardUnitModel> targetUnits, BoardUnitModel originUnit, bool randomly = true)
        {
            switch (originUnit.Prototype.Faction)
            {
                case Enumerators.Faction.AIR:
                    AirRankBuff(targetUnits, originUnit.Prototype.Rank, originUnit, randomly);
                    break;
                case Enumerators.Faction.EARTH:
                    EarthRankBuff(targetUnits, originUnit.Prototype.Rank, originUnit, randomly);
                    break;
                case Enumerators.Faction.WATER:
                    WaterRankBuff(targetUnits, originUnit.Prototype.Rank, originUnit, randomly);
                    break;
                case Enumerators.Faction.FIRE:
                    FireRankBuff(targetUnits, originUnit.Prototype.Rank, originUnit, randomly);
                    break;
                case Enumerators.Faction.TOXIC:
                    ToxicRankBuff(targetUnits, originUnit.Prototype.Rank, originUnit, randomly);
                    break;
                case Enumerators.Faction.LIFE:
                    LifeRankBuff(targetUnits, originUnit.Prototype.Rank, originUnit, randomly);
                    break;
                default:
                    Log.Warn($"Error occured. Tried to buff unit with faction: {originUnit.Prototype.Faction}. card id: {originUnit.InstanceId}");
                    break;
            }
        }

        private void AirRankBuff(List<BoardUnitModel> targetUnits, Enumerators.CardRank originUnitRank, BoardUnitModel originUnit, bool randomly = true)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (originUnitRank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.Guard);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.Guard);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.Guard);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(originUnitRank), originUnitRank, null);
            }

            BuffRandomAlly(targetUnits, count, buffs, originUnit, randomly);
        }

        private void EarthRankBuff(List<BoardUnitModel> units, Enumerators.CardRank rank, BoardUnitModel boardUnitModel, bool randomly = true)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.Heavy);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.Heavy);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.Heavy);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            BuffRandomAlly(units, count, buffs, boardUnitModel, randomly);
        }

        private void FireRankBuff(List<BoardUnitModel> units, Enumerators.CardRank rank, BoardUnitModel boardUnitModel, bool randomly = true)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.Blitz);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.Blitz);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.Blitz);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            BuffRandomAlly(units, count, buffs, boardUnitModel, randomly);
        }

        private void LifeRankBuff(List<BoardUnitModel> units, Enumerators.CardRank rank, BoardUnitModel boardUnitModel, bool randomly = true)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.Reanimate);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.Reanimate);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.Reanimate);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            BuffRandomAlly(units, count, buffs, boardUnitModel, randomly);
        }

        private void ToxicRankBuff(List<BoardUnitModel> units, Enumerators.CardRank rank, BoardUnitModel boardUnitModel, bool randomly = true)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.Destroy);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.Destroy);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.Destroy);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            BuffRandomAlly(units, count, buffs, boardUnitModel, randomly);
        }

        private void WaterRankBuff(List<BoardUnitModel> units, Enumerators.CardRank rank, BoardUnitModel boardUnitModel, bool randomly = true)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (rank)
            {
                case Enumerators.CardRank.OFFICER:
                    buffs.Add(Enumerators.BuffType.Freeze);
                    break;
                case Enumerators.CardRank.COMMANDER:
                    buffs.Add(Enumerators.BuffType.Freeze);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 2;
                    break;
                case Enumerators.CardRank.GENERAL:
                    buffs.Add(Enumerators.BuffType.Freeze);
                    buffs.Add(Enumerators.BuffType.Attack);
                    count = 3;
                    break;
            }

            BuffRandomAlly(units, count, buffs, boardUnitModel, randomly);
        }

        public void AddUnitForIgnoreRankBuff(BoardUnitModel unit)
        {
            if (!_unitsForIgnoreRankBuff.Contains(unit))
            {
                _unitsForIgnoreRankBuff.Add(unit);
            }
        }

        private void BuffRandomAlly(List<BoardUnitModel> targetUnits, int count,
                                    List<Enumerators.BuffType> buffTypes,
                                    BoardUnitModel originUnit, bool randomly = true)
        {
            if (_tutorialManager.IsTutorial)
            {
                targetUnits = targetUnits.FindAll(x => x.UnitCanBeUsable());
            }

            if (randomly)
            {
                targetUnits = InternalTools.GetRandomElementsFromList(targetUnits, count);
            }

            foreach (Enumerators.BuffType buff in buffTypes)
            {
                foreach (BoardUnitModel unit in targetUnits)
                {
                    if (unit == null)
                    {
                        ExceptionReporter.LogExceptionAsWarning(Log, new Exception("Tried to Buff Null Unit in Ranks System"));
                        continue;
                    }

                    unit.ApplyBuff(buff);
                }
            }


            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
            {
                if (!originUnit.Owner.IsLocalPlayer)
                {
                    _ranksUpgradeCompleteAction?.Invoke();
                    _ranksUpgradeCompleteAction = null;
                    return;
                }
            }

            RanksUpdated?.Invoke(originUnit, targetUnits);

            _ranksUpgradeCompleteAction?.Invoke();
            _ranksUpgradeCompleteAction = null;
        }

        public void BuffAllyManually(List<BoardUnitModel> units, BoardUnitModel card)
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
