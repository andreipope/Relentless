// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class ReviveDiedUnitsOfTypeFromMatchAbility : AbilityBase
    {
        public Enumerators.SetType setType;

        public ReviveDiedUnitsOfTypeFromMatchAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            setType = ability.abilitySetType;
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

            List<WorkingCard> units = _gameplayManager.CurrentPlayer.CardsInGraveyard.FindAll(x => x.libraryCard.cardSetType == setType);

            foreach (WorkingCard unit in units)
            {
                ReviveUnit(unit);
            }

            units = _gameplayManager.OpponentPlayer.CardsInGraveyard.FindAll(x => x.libraryCard.cardSetType == setType);

            foreach (WorkingCard unit in units)
            {
                ReviveUnit(unit);
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }

        private void ReviveUnit(WorkingCard workingCard)
        {
            Player playerOwner = workingCard.owner;

            if (playerOwner.BoardCards.Count >= Constants.MAX_BOARD_UNITS)

                return;

            Card libraryCard = workingCard.libraryCard.Clone();

            WorkingCard card = new WorkingCard(libraryCard, playerOwner);
            BoardUnit unit = _battlegroundController.CreateBoardUnit(playerOwner, card);

            playerOwner.RemoveCardFromGraveyard(workingCard);
            playerOwner.AddCardToBoard(card);
            playerOwner.BoardCards.Add(unit);

            if (playerOwner.IsLocalPlayer)
            {
                _battlegroundController.playerBoardCards.Add(unit);
                _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(_gameplayManager.CurrentPlayer.BoardCards);
            } else
            {
                _battlegroundController.opponentBoardCards.Add(unit);
                _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
            }
        }
    }
}
