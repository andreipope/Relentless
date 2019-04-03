using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class TakeControlAbility : CardAbility
    {
        private List<BoardUnitModel> _movedUnits;

        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            _movedUnits = new List<BoardUnitModel>();

            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        if (PlayerOwner.CardsOnBoard.Count >= PlayerOwner.MaxCardsInPlay)
                            break;

                        _movedUnits.Add(boardUnitModel);
                        break;
                }
            }

            TakeControlEnemyUnitEnded();
        }

        private void TakeControlEnemyUnitEnded()
        {
            foreach (BoardUnitModel unit in _movedUnits)
            {
                BattlegroundController.TakeControlUnit(PlayerOwner, unit);
            }
        }
    }
}
