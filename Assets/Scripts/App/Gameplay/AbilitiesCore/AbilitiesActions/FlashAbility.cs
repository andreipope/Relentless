using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class FlashAbility : CardAbility
    {
        private int _numberOfAttacksWas;

        public override void DoAction()
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Attack))
                        {
                            int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                                       Common.Enumerators.AbilityParameter.Attack);

                            _numberOfAttacksWas++;

                            if (_numberOfAttacksWas < value)
                            {
                                UnitModelOwner.ForceSetCreaturePlayable();
                            }
                            else
                            {
                                _numberOfAttacksWas = 0;
                            }
                        }
                       
                        break;
                }
            }
        }
    }
}
