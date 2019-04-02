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

                int gooVials = value;
                for (int i = 0; i < value; i++)
                {
                    if(PlayerOwner.GooVials + gooVials >= PlayerOwner.MaxGooVials)
                    {
                        PlayerOwner.PlayerCardsController.AddCardFromDeckToHand();
                        gooVials--;
                    }
                }

                PlayerOwner.GooVials += gooVials;
            }
        }
    }
}
