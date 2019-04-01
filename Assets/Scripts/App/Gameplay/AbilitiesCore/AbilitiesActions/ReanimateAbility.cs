using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public class ReanimateAbility : CardAbility
    {
        public override void DoAction()
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        if (boardUnitModel.IsReanimated)
                            return;

                        Player owner = boardUnitModel.OwnerPlayer;

                        owner.PlayerCardsController.RemoveCardFromGraveyard(boardUnitModel);

                        boardUnitModel.ResetToInitial();
                        BoardUnitView reanimatedUnit = CreateBoardUnit(boardUnitModel, owner);
                        boardUnitModel.IsReanimated = true;
                        boardUnitModel.RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Reanimate);

                        BattlegroundController.RegisterBoardUnitView(PlayerOwner, reanimatedUnit);
                        owner.PlayerCardsController.AddCardToBoard(boardUnitModel, ItemPosition.End);

                        if (GameplayManager.IsLocalPlayerTurn())
                        {
                            AbilitiesController.ReactivateAbilitiesOnUnit(reanimatedUnit.Model);
                        }
                        break;
                }
            }
        }

        private BoardUnitView CreateBoardUnit(BoardUnitModel boardUnitModel, Player owner)
        {
            BoardUnitView boardUnitView = BattlegroundController.CreateBoardUnit(owner, boardUnitModel);

            if (!owner.Equals(GameplayManager.CurrentTurnPlayer))
            {
                boardUnitView.Model.IsPlayable = true;
            }

            boardUnitView.StopSleepingParticles();

            GameplayManager.CanDoDragActions = true;

            return boardUnitView;
        }
    }
}
