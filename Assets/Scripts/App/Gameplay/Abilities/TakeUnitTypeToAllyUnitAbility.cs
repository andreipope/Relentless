using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

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

            List<BoardUnitView> allies = PlayerCallerOfAbility.BoardCards
                .Where(unit => unit != AbilityUnitViewOwner && !unit.Model.HasFeral && unit.Model.NumTurnsOnBoard == 0)
                .ToList();

            if (allies.Count > 0)
            {
                int random = Random.Range(0, allies.Count);
                TakeTypeToUnit(allies[random]);
            }
        }

        private void TakeTypeToUnit(BoardUnitView unit)
        {
            if (unit == null)
                return;

            switch (UnitType)
            {
                case Enumerators.CardType.HEAVY:
                    unit.Model.SetAsHeavyUnit();
                    break;
                case Enumerators.CardType.FERAL:
                    unit.Model.SetAsFeralUnit();
                    break;
            }
        }
    }
}
