using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class ReviveAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        ReviveUnit(boardUnitModel);
                        break;
                }
            }

        }

        public override void AbilityInitializedAction()
        {
            base.AbilityInitializedAction();
        }

        private void ReviveUnit(BoardUnitModel boardUnitModel)
        {
            Player playerOwner = boardUnitModel.Owner;

            if (playerOwner.CardsOnBoard.Count >= playerOwner.MaxCardsInPlay)
                return;

            playerOwner.PlayerCardsController.RemoveCardFromGraveyard(boardUnitModel);
            boardUnitModel.ResetToInitial();
            BoardUnitModel revivedBoardUnitModel = boardUnitModel;
            BoardUnitView revivedBoardUnitView = BattlegroundController.CreateBoardUnit(playerOwner, revivedBoardUnitModel);

            playerOwner.PlayerCardsController.AddCardToBoard(revivedBoardUnitModel, ItemPosition.End);

            if (playerOwner.IsLocalPlayer)
            {
                BattlegroundController.RegisterBoardUnitView(GameplayManager.CurrentPlayer, revivedBoardUnitView);
                AbilitiesController.ReactivateAbilitiesOnUnit(revivedBoardUnitView.Model);
            }
            else
            {
                BattlegroundController.RegisterBoardUnitView(GameplayManager.OpponentPlayer, revivedBoardUnitView);
                if (GameplayManager.IsLocalPlayerTurn())
                {
                    AbilitiesController.ReactivateAbilitiesOnUnit(revivedBoardUnitView.Model);
                }
            }

            RanksController.AddUnitForIgnoreRankBuff(revivedBoardUnitModel);

            BoardController.UpdateCurrentBoardOfPlayer(playerOwner, null);
        }
    }
}
