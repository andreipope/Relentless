using Loom.ZombieBattleground.Common;
using System;

namespace Loom.ZombieBattleground
{
    public class ChangeTypeAbility : CardAbility
    {
        public override void DoAction()
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        if (AbilitiesController.HasParameter(GenericParameters, Enumerators.AbilityParameter.Type))
                        {
                            Enumerators.CardType type = AbilitiesController.GetParameterValue<Enumerators.CardType>(GenericParameters,
                                                                       Enumerators.AbilityParameter.Type);

                            TakeTypeToUnit(boardUnitModel, type);
                        }
                        break;
                }
            }
        }

        private void TakeTypeToUnit(BoardUnitModel unit, Enumerators.CardType type)
        {
            if (unit == null)
                return;

            switch (type)
            {
                case Enumerators.CardType.HEAVY:
                    unit.SetAsHeavyUnit();
                    break;
                case Enumerators.CardType.FERAL:
                    unit.SetAsFeralUnit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
