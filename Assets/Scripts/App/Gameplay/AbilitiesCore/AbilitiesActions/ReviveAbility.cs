using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class ReviveAbility : CardAbility
    {
        public override void DoAction()
        {
            UnityEngine.Debug.LogError(3333);
            Log.Error(11111);
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
            UnityEngine.Debug.LogError(5555);
        }

        private void ReviveUnit(BoardUnitModel boardUnitModel)
        {
            Player playerOwner = boardUnitModel.Owner;
            Log.Error(2222);

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
