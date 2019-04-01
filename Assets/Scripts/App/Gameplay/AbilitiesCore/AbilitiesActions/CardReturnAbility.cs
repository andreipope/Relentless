using System;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class CardReturnAbility : CardAbility
    {
        public override void DoAction()
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.TargetHand))
                        {
                            CardsController.ReturnCardToHand(boardUnitModel);
                        }
                        if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.TargetDeck))
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
