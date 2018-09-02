// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class ReturnUnitsOnBoardToOwnersDecksAbility : AbilityBase
    {
        public ReturnUnitsOnBoardToOwnersDecksAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
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

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardUnit> units = new List<BoardUnit>();
            units.AddRange(_gameplayManager.CurrentPlayer.BoardCards);
            units.AddRange(_gameplayManager.OpponentPlayer.BoardCards);

            foreach (BoardUnit unit in units)
            {
                ReturnBoardUnitToDeck(unit);
            }

            units.Clear();
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }

        private void ReturnBoardUnitToDeck(BoardUnit unit)
        {
            if (((abilityUnitOwner != null) && (unit == abilityUnitOwner)) || (unit == null))

                return;

            // implement animation
            unit.ownerPlayer.AddCardToDeck(new WorkingCard(unit.Card.libraryCard.Clone(), unit.ownerPlayer));
            unit.MoveUnitFromBoardToDeck();
        }
    }
}
