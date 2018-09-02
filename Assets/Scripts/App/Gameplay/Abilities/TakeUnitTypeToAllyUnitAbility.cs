using System.Collections.Generic;
using System.Linq;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
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

            if (AbilityCallType != Enumerators.AbilityCallType.Entry)

                return;

            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardUnit> allies = new List<BoardUnit>();

            allies = PlayerCallerOfAbility.BoardCards.Where(unit => (unit != AbilityUnitOwner) && !unit.HasFeral && (unit.NumTurnsOnBoard == 0)).ToList();

            if (allies.Count > 0)
            {
                int random = Random.Range(0, allies.Count);
                TakeTypeToUnit(allies[random]);
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        private void TakeTypeToUnit(BoardUnit unit)
        {
            if (unit == null)

                return;

            switch (UnitType)
            {
                case Enumerators.CardType.Heavy:
                    unit.SetAsHeavyUnit();
                    break;
                case Enumerators.CardType.Feral:
                    unit.SetAsFeralUnit();
                    break;
            }
        }
    }
}
