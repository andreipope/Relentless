using Loom.ZombieBattleground.Common;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class OverflowGooAbility : CardAbility
    {
        public override void DoAction()
        {
            if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Goo))
            {
                int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                           Common.Enumerators.AbilityParameter.Goo);

                PlayerOwner.CurrentGoo += value;
            }
        }
    }
}
