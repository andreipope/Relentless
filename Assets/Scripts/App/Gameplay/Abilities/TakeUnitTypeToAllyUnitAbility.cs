using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class TakeUnitTypeToAllyUnitAbility : AbilityBase
    {
        public Enumerators.CardType UnitType;

        public TakeUnitTypeToAllyUnitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            UnitType = ability.TargetUnitType;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardUnitModel> allies;

            if (PredefinedTargets != null)
            {
                allies = PredefinedTargets.Cast<BoardUnitModel>().ToList();

                if (allies.Count > 0)
                {
                    TakeTypeToUnit(allies[0]);
                }
            }
            else
            {
                 allies = PlayerCallerOfAbility.BoardCards.Select(x => x.Model)
                .Where(unit => unit != AbilityUnitOwner && !unit.HasFeral && unit.NumTurnsOnBoard == 0)
                .ToList();

                if (allies.Count > 0)
                {
                    int random = Random.Range(0, allies.Count);
                    TakeTypeToUnit(allies[random]);
                }
            }
        }

        private void TakeTypeToUnit(BoardUnitModel unit)
        {
            if (unit == null)
                return;

            switch (UnitType)
            {
                case Enumerators.CardType.HEAVY:
                    unit.SetAsHeavyUnit();
                    break;
                case Enumerators.CardType.FERAL:
                    unit.SetAsFeralUnit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(UnitType), UnitType, null);
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
            {
               unit
            }, AbilityData.AbilityType, Protobuf.AffectObjectType.Types.Enum.Character);
        }
    }
}
