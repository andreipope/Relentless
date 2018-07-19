// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class ReturnUnitsOnBoardToOwnersDecksAbility : AbilityBase
    {
        public ReturnUnitsOnBoardToOwnersDecksAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.AT_START)
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

        protected override void UnitOnAttackEventHandler(object info)
        {
            base.UnitOnAttackEventHandler(info);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            foreach(var unit in _gameplayManager.CurrentPlayer.BoardCards)
                ReturnBoardUnitToDeck(unit);


            foreach (var unit in _gameplayManager.OpponentPlayer.BoardCards)
                ReturnBoardUnitToDeck(unit);
        }

        private void ReturnBoardUnitToDeck(BoardUnit unit)
        {
            // implement animation

            unit.ownerPlayer.AddCardToDeck(new WorkingCard(unit.Card.libraryCard.Clone(), unit.ownerPlayer));
            unit.MoveUnitFromBoardToDeck();
        }
    }
}
