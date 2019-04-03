using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AdditionalStatAbility : CardAbility
    {
        private bool _isActive = false;

        private List<BoardObject> _targets = new List<BoardObject>();

        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            if (genericParameters != null)
            {
                foreach (BoardObject target in _targets)
                {
                    switch (target)
                    {
                        case BoardUnitModel boardUnitModel:
                            ProcessUnit(boardUnitModel, _isActive);
                            break;
                    }
                }

                _targets.Clear();
            }
            else
            {
                foreach (BoardObject target in Targets)
                {
                    switch (target)
                    {
                        case BoardUnitModel boardUnitModel:
                            ProcessUnit(boardUnitModel, _isActive);
                            break;
                    }

                    _targets.Add(target);
                }
            }
        }

        private void ProcessUnit(BoardUnitModel boardUnitModel, bool revert)
        {
            if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Defense))
            {
                int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                           Common.Enumerators.AbilityParameter.Defense);

                boardUnitModel.BuffedDefense += revert ? -value : value;
                boardUnitModel.CurrentDefense += revert ? -value : value;
            }
            if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Attack))
            {
                int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                              Common.Enumerators.AbilityParameter.Attack);
            
                boardUnitModel.BuffedDamage += revert ? -value : value;
                boardUnitModel.CurrentDamage += revert ? -value : value;
            }

            _isActive = !revert;
        }
    }
}
