using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class BlitzAbility : AbilityBase
    {
        private int Count { get; }
        private Enumerators.Faction Faction { get; }

        public BlitzAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
            Faction = ability.Faction;
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

            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                List<BoardUnitView> units = new List<BoardUnitView>();

                if (PredefinedTargets != null)
                {
                    units = PredefinedTargets.Select(target => target.BoardObject).Cast<BoardUnitModel>().
                             Select(model => BattlegroundController.GetBoardUnitViewByModel(model)).ToList();
                }
                else
                {
                    foreach (Enumerators.AbilityTarget targetType in AbilityTargetTypes)
                    {
                        switch (targetType)
                        {
                            case Enumerators.AbilityTarget.OPPONENT_CARD:
                                units.AddRange(GetOpponentOverlord().BoardCards.FindAll(x => x.Model.Card.InstanceId != AbilityUnitOwner.InstanceId && x.Model.Card.Prototype.Faction == Faction));
                                break;
                            case Enumerators.AbilityTarget.PLAYER_CARD:
                                units.AddRange(PlayerCallerOfAbility.BoardCards.FindAll(x => x.Model.Card.InstanceId != AbilityUnitOwner.InstanceId && x.Model.Card.Prototype.Faction == Faction));
                                break;
                        }
                    }

                    units = InternalTools.GetRandomElementsFromList(units, Count);
                }

                foreach (BoardUnitView unit in units)
                {
                    TakeBlitzToUnit(unit.Model);
                }

                InvokeUseAbilityEvent(
                    units
                        .Select(x => new ParametrizedAbilityBoardObject(x.Model))
                        .ToList()
                );
            }
            else
            {
                TakeBlitzToUnit(AbilityUnitOwner);
                InvokeUseAbilityEvent(
                    new List<ParametrizedAbilityBoardObject>
                    {
                        new ParametrizedAbilityBoardObject(AbilityUnitOwner)
                    }
                );
            }
        }

        private void TakeBlitzToUnit(BoardUnitModel unit)
        {
            unit.ApplyBuff(Enumerators.BuffType.BLITZ);
        }
    }
}
