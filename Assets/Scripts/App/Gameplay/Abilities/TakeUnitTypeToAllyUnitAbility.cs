// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class TakeUnitTypeToAllyUnitAbility : AbilityBase
    {
        public Enumerators.CardType unitType;

        public TakeUnitTypeToAllyUnitAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            unitType = ability.targetUnitType;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.ENTRY)
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

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        private void TakeTypeToUnit(BoardUnit unit)
        {
            if (unit == null)
                return;

            switch (unitType)
            {
                case Enumerators.CardType.HEAVY:
                    unit.SetAsHeavyUnit();
                    break;
                case Enumerators.CardType.FERAL:
                    unit.SetAsFeralUnit();
                    break;
            }
        }
        public override void Action(object info = null)
        {
            base.Action(info);


            //Debug.Log();
            List<BoardUnit> allies = new List<BoardUnit>();
            allies.AddRange(playerCallerOfAbility.BoardCards);
            allies.Remove(abilityUnitOwner);

            int random = Random.Range(0, playerCallerOfAbility.BoardCards.Count);
            TakeTypeToUnit(playerCallerOfAbility.BoardCards[random]);
        }
    }
}
