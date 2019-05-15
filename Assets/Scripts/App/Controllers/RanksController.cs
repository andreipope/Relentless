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

        public delegate void RanksUpdatedDelegate(CardModel originUnit, IReadOnlyList<CardModel> targetUnits);
        public event RanksUpdatedDelegate RanksUpdated;

        private ITutorialManager _tutorialManager;
        private IGameplayManager _gameplayManager;
        private IOverlordExperienceManager _overlordExperienceManager;

        private BattlegroundController _battlegroundController;
        private ActionsQueueController _actionsQueueController;

        private Action _ranksUpgradeCompleteAction;

        private List<CardModel> _unitsForIgnoreRankBuff;

        public void Dispose()
        {
        }

        public void Init()
        {
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _overlordExperienceManager = GameClient.Get<IOverlordExperienceManager>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _unitsForIgnoreRankBuff = new List<CardModel>();
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
        }

        public GameplayActionQueueAction AddUpdateRanksByElementsAction(IReadOnlyList<CardModel> units, CardModel cardModel)
        {
            if (GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
            {
                if (!cardModel.Owner.IsLocalPlayer)
                    return null;
            }

            GameplayActionQueueAction.ExecutedActionDelegate action = completeCallback =>
            {
                _ranksUpgradeCompleteAction = completeCallback;

                List<CardModel> filter = units.Where(unit =>
                        unit.Card.Prototype.Faction == cardModel.Prototype.Faction &&
                        (int)unit.Card.Prototype.Rank < (int)cardModel.Prototype.Rank &&
                        !_battlegroundController.GetCardViewByModel<BoardUnitView>(unit).WasDestroyed &&
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
                    DoRankUpgrades(filter, cardModel);

                           _overlordExperienceManager.ReportExperienceAction(Enumerators.ExperienceActionType.ActivateRankAbility, _overlordExperienceManager.PlayerMatchMatchExperienceInfo);

                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.RanksUpdated);
                }
                else
                {
                    _ranksUpgradeCompleteAction?.Invoke();
                    _ranksUpgradeCompleteAction = null;
                }
            };

            return _actionsQueueController.AddNewActionInToQueue(action, Enumerators.QueueActionType.RankBuff);
        }

        public void DoRankUpgrades(List<CardModel> targetUnits, CardModel originUnit, bool randomly = true)
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
                    _ranksUpgradeCompleteAction?.Invoke();
                    _ranksUpgradeCompleteAction = null;
                    break;
            }
        }

        private void AirRankBuff(List<CardModel> targetUnits, Enumerators.CardRank originUnitRank, CardModel originUnit, bool randomly = true)
        {
            List<Enumerators.BuffType> buffs = new List<Enumerators.BuffType>();
            int count = 1;
            switch (originUnitRank)
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
                    throw new ArgumentOutOfRangeException(nameof(originUnitRank), originUnitRank, null);
            }

            BuffRandomAlly(targetUnits, count, buffs, originUnit, randomly);
        }

        private void EarthRankBuff(List<CardModel> units, Enumerators.CardRank rank, CardModel cardModel, bool randomly = true)
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

            BuffRandomAlly(units, count, buffs, cardModel, randomly);
        }

        private void FireRankBuff(List<CardModel> units, Enumerators.CardRank rank, CardModel cardModel, bool randomly = true)
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

            BuffRandomAlly(units, count, buffs, cardModel, randomly);
        }

        private void LifeRankBuff(List<CardModel> units, Enumerators.CardRank rank, CardModel cardModel, bool randomly = true)
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

            BuffRandomAlly(units, count, buffs, cardModel, randomly);
        }

        private void ToxicRankBuff(List<CardModel> units, Enumerators.CardRank rank, CardModel cardModel, bool randomly = true)
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

            BuffRandomAlly(units, count, buffs, cardModel, randomly);
        }

        private void WaterRankBuff(List<CardModel> units, Enumerators.CardRank rank, CardModel cardModel, bool randomly = true)
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

            BuffRandomAlly(units, count, buffs, cardModel, randomly);
        }

        public void AddUnitForIgnoreRankBuff(CardModel unit)
        {
            if (!_unitsForIgnoreRankBuff.Contains(unit))
            {
                _unitsForIgnoreRankBuff.Add(unit);
            }
        }

        private void BuffRandomAlly(List<CardModel> targetUnits, int count,
                                    List<Enumerators.BuffType> buffTypes,
                                    CardModel originUnit, bool randomly = true)
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
                foreach (CardModel unit in targetUnits)
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

        public void BuffAllyManually(List<CardModel> units, CardModel card)
        {
            _gameplayManager.GetController<ActionsQueueController>().AddNewActionInToQueue(
                 completeCallback =>
                 {
                     _ranksUpgradeCompleteAction = completeCallback;

                     DoRankUpgrades(units, card, false);
                 }, Enumerators.QueueActionType.RankBuff);
        }
    }
}
