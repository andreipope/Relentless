// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ReviveDiedUnitsOfTypeFromMatchAbility : AbilityBase
    {
        public Enumerators.SetType setType;

        public ReviveDiedUnitsOfTypeFromMatchAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
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

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            var units = _gameplayManager.CurrentPlayer.CardsInGraveyard.FindAll(x => x.libraryCard.cardSetType == setType);

            foreach (var unit in units)
                ReviveUnit(unit);

            units = _gameplayManager.OpponentPlayer.CardsInGraveyard.FindAll(x => x.libraryCard.cardSetType == setType);

            foreach (var unit in units)
                ReviveUnit(unit);
        }

        private void ReviveUnit(WorkingCard workingCard)
        {
            var playerOwner = workingCard.owner;

            if (playerOwner.BoardCards.Count >= Constants.MAX_BOARD_UNITS)
                return;

            var libraryCard = workingCard.libraryCard.Clone();

            var card = new WorkingCard(libraryCard, playerOwner);
            var unit = _battlegroundController.CreateBoardUnit(playerOwner, card);

            playerOwner.RemoveCardFromGraveyard(workingCard);
            playerOwner.AddCardToBoard(card);
            playerOwner.BoardCards.Add(unit);

            if (playerOwner.IsLocalPlayer)
            {
                _battlegroundController.playerBoardCards.Add(unit);
                _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer();
            }
            else
            {
                _battlegroundController.opponentBoardCards.Add(unit);
                _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
            }
        }
    }
}
