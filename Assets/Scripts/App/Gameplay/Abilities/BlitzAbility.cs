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

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                List<BoardUnitModel> units = new List<BoardUnitModel>();

                if (PredefinedTargets != null)
                {
                    units = PredefinedTargets.Select(target => target.BoardObject).Cast<BoardUnitModel>().ToList();
                }
                else
                {
                    foreach (Enumerators.Target targetType in AbilityTargetTypes)
                    {
                        switch (targetType)
                        {
                            case Enumerators.Target.OPPONENT_CARD:
                                units.AddRange(GetOpponentOverlord().CardsOnBoard.FindAll(x => x.Card.InstanceId != AbilityUnitOwner.InstanceId && x.Card.Prototype.Faction == Faction));
                                break;
                            case Enumerators.Target.PLAYER_CARD:
                                units.AddRange(PlayerCallerOfAbility.CardsOnBoard.FindAll(x => x.Card.InstanceId != AbilityUnitOwner.InstanceId && x.Card.Prototype.Faction == Faction));
                                break;
                        }
                    }

                    units = InternalTools.GetRandomElementsFromList(units, Count);
                }

                foreach (BoardUnitModel unit in units)
                {
                    TakeBlitzToUnit(unit);
                }

                InvokeUseAbilityEvent(
                    units
                        .Select(x => new ParametrizedAbilityBoardObject(x))
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
            unit.ApplyBuff(Enumerators.BuffType.BLITZ, true);
        }
    }
}
