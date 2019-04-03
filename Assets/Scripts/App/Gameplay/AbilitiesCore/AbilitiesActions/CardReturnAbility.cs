using Loom.ZombieBattleground.Data;
using System;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class CardReturnAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        if (AbilitiesController.HasSubTrigger(this, Common.Enumerators.AbilitySubTrigger.TargetHand))
                        {
                            CardsController.ReturnCardToHand(boardUnitModel);
                        }
                        if (AbilitiesController.HasSubTrigger(this, Common.Enumerators.AbilitySubTrigger.TargetDeck))
                        {
                            boardUnitModel.ResetToInitial();
                            PlayerOwner.PlayerCardsController.AddCardToDeck(boardUnitModel, true);
                        }
                        break;
                }
            }
        }
    }
}
