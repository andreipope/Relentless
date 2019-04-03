using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class ChangeCostAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            if (AbilitiesController.HasParameter(genericParameters, Common.Enumerators.AbilityParameter.Cost))
            {
                int value = AbilitiesController.GetParameterValue<int>(genericParameters,
                                                           Common.Enumerators.AbilityParameter.Cost);
                foreach (BoardObject target in Targets)
                {
                    switch (target)
                    {
                        case BoardUnitModel boardUnitModel:
                            boardUnitModel = CardsController.LowGooCostOfCardInHand(PlayerOwner, boardUnitModel, value);
                            break;
                    }
                }
            }
        }
    }
}
