using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ReplaceUnitsWithTypeOnStrongerOnesAbility : AbilityBase
    {
        private List<BoardUnitView> _boardUnits;
        private List<ReplaceUnitInfo> _replaceUnitInfos;

        public int Value;
        public Enumerators.Faction Faction;
        public List<Enumerators.AbilityTarget> TargetTypes;

        public ReplaceUnitsWithTypeOnStrongerOnesAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Faction = ability.Faction;
            TargetTypes = ability.AbilityTarget;

            _boardUnits = new List<BoardUnitView>();
            _replaceUnitInfos = new List<ReplaceUnitInfo>();
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }


        public override void Action(object info = null)
        {
            base.Action(info);

            if (PvPManager.UseBackendGameLogic)
                return;

            if(PredefinedTargets != null && PredefinedTargets.Count == 0)
                return;

            GetInfosAboutUnitsOnBoard();
            GetPossibleNewUnits();
            ClearOldUnitsOnBoard();
            GenerateNewUnitsOnBoard();

            List<ParametrizedAbilityBoardObject> targets = new List<ParametrizedAbilityBoardObject>();

            foreach(ReplaceUnitInfo unitinfo in _replaceUnitInfos)
            {
                targets.Add(new ParametrizedAbilityBoardObject(
                    unitinfo.OldUnitView.Model,
                    new ParametrizedAbilityParameters
                    {
                        CardName = unitinfo.NewUnitCardTitle
                    }
                ));
            }

            InvokeUseAbilityEvent(targets);
        }

        private void GetInfosAboutUnitsOnBoard()
        {
            foreach (Enumerators.AbilityTarget target in TargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTarget.OPPONENT_CARD:
                        _boardUnits.AddRange(GetOpponentOverlord().BoardCards.FindAll(unit => unit.Model.Card.Prototype.Faction == Faction));
                        break;
                    case Enumerators.AbilityTarget.PLAYER_CARD:
                        _boardUnits.AddRange(PlayerCallerOfAbility.BoardCards.FindAll(unit => unit.Model.Card.Prototype.Faction == Faction));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }


            if (AbilityUnitOwner != null)
            {
                BoardUnitView view = BattlegroundController.GetBoardUnitViewByModel(AbilityUnitOwner);

                if (_boardUnits.Contains(view))
                {
                    _boardUnits.Remove(view);
                }
            }
        }

        private void GenerateNewUnitsOnBoard()
        {
            BoardUnitView unit;
            ItemPosition itemPosition;
            foreach (ReplaceUnitInfo unitInfo in _replaceUnitInfos)
            {
                itemPosition = new ItemPosition(unitInfo.Position);
                unit = CardsController.SpawnUnitOnBoard(unitInfo.OwnerPlayer, unitInfo.NewUnitCardTitle, itemPosition);
                if (unit != null)
                {
                    AddUnitToBoardCards(unitInfo.OwnerPlayer, itemPosition, unit);
                }
            }
        }

        private void ClearOldUnitsOnBoard()
        {
            if (PredefinedTargets != null && PredefinedTargets.Count > 0)
            {
                foreach (BoardUnitView unit in _boardUnits)
                {
                    unit.Model.OwnerPlayer.BoardCards.Remove(unit);
                    unit.Model.OwnerPlayer.RemoveCardFromBoard(unit.Model);

                    unit.DisposeGameObject();
                }
            }
            else
            {
                foreach (ReplaceUnitInfo unitInfo in _replaceUnitInfos)
                {
                    unitInfo.OldUnitView.Model.OwnerPlayer.BoardCards.Remove(unitInfo.OldUnitView);
                    unitInfo.OldUnitView.Model.OwnerPlayer.RemoveCardFromBoard(unitInfo.OldUnitView.Model);

                    unitInfo.OldUnitView.DisposeGameObject();
                }
            }
        }

        private void GetPossibleNewUnits()
        {
            if (PredefinedTargets != null)
            {
                BoardUnitView unit;
                foreach (ParametrizedAbilityBoardObject boardObject in PredefinedTargets)
                {
                    unit = BattlegroundController.GetBoardUnitViewByModel(boardObject.BoardObject as BoardUnitModel);

                    _replaceUnitInfos.Add(new ReplaceUnitInfo()
                    {
                        OldUnitView = unit,
                        NewUnitCardTitle = boardObject.Parameters.CardName,
                        OwnerPlayer = unit.Model.OwnerPlayer,
                        Position = unit.Model.OwnerPlayer.BoardCards.IndexOf(unit)
                    });
                }
            }
            else
            {
                ReplaceUnitInfo replaceUnitInfo = null;
	            foreach (BoardUnitView unit in _boardUnits)
	            {
	                replaceUnitInfo = new ReplaceUnitInfo()
	                {
	                    OldUnitCost = unit.Model.Card.InstanceCard.Cost,
	                    NewUnitPossibleCost = unit.Model.Card.InstanceCard.Cost + 1,
	                    OldUnitView = unit,
	                    OwnerPlayer = unit.Model.OwnerPlayer,
	                    Position = unit.Model.OwnerPlayer.BoardCards.IndexOf(unit),
                        NewUnitCardTitle = unit.Model.Card.Prototype.Name
	                };

	                _replaceUnitInfos.Add(replaceUnitInfo);
	            }

	            _replaceUnitInfos.ForEach(GetPossibleNewUnitByMinCost);
			}
        }

        private void GetPossibleNewUnitByMinCost(ReplaceUnitInfo replaceUnitInfo)
        {
            List<Card> possibleUnits = DataManager.CachedCardsLibraryData.Cards
                .Where(x => x.Cost >= replaceUnitInfo.NewUnitPossibleCost && x.Faction == Faction)
                .ToList();

            if (possibleUnits.Count == 0)
            {
                possibleUnits = DataManager.CachedCardsLibraryData.Cards
                    .Where(x => x.Cost >= replaceUnitInfo.OldUnitCost && x.Faction == Faction)
                    .ToList();
            }

            replaceUnitInfo.NewUnitCardTitle = possibleUnits[UnityEngine.Random.Range(0, possibleUnits.Count)].Name;
        }

        private void AddUnitToBoardCards(Player owner, ItemPosition position, BoardUnitView unit)
        {
            if (owner.IsLocalPlayer)
            {
                BattlegroundController.PlayerBoardCards.Insert(position, unit);
            }
            else
            {
                BattlegroundController.OpponentBoardCards.Insert(position, unit);
            }
        }

        public class ReplaceUnitInfo
        {
            public int OldUnitCost;
            public int NewUnitPossibleCost;
            public string NewUnitCardTitle;
            public BoardUnitView OldUnitView;
            public BoardUnitView NewUnitView;
            public Player OwnerPlayer;
            public int Position;
        }

    }
}
