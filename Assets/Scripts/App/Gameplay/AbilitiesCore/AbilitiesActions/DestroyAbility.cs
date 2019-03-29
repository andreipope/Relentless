namespace Loom.ZombieBattleground
{
    public class DestroyAbility : CardAbility
    {
        public override void DoAction()
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        BattlegroundController.DestroyBoardUnit(boardUnitModel, false);
                        break;
                }
            }
        }
    }
}
