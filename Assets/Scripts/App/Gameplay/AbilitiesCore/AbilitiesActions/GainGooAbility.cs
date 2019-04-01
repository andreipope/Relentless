namespace Loom.ZombieBattleground
{
    internal class GainGooAbility : CardAbility
    {
        public override void DoAction()
        {
            
            if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Goo))
            {
                int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                           Common.Enumerators.AbilityParameter.Goo);

                PlayerOwner.GooVials += value;
            }
        }
    }
}
