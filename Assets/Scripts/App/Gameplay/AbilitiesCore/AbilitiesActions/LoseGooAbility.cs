namespace Loom.ZombieBattleground
{
    internal class LoseGooAbility : CardAbility
    {
        public override void DoAction()
        {
            if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Turns) &&
                AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Goo))
            {
                int countOfTurns = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                           Common.Enumerators.AbilityParameter.Turns);

                int gooValue = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                           Common.Enumerators.AbilityParameter.Goo);

                if (countOfTurns == 0)
                {
                    PlayerOwner.CurrentGoo -= gooValue;
                    PlayerOwner.GooVials -= gooValue;
                }
                else
                {
                    PlayerOwner.CurrentGooModificator += gooValue;
                }
            }
        }
    }
}
