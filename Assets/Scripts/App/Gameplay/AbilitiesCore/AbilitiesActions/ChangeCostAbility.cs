namespace Loom.ZombieBattleground
{
    public class ChangeCostAbility : CardAbility
    {
        public override void DoAction()
        {
            if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Cost))
            {
                int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
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
