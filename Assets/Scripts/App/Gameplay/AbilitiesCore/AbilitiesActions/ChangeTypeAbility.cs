using Loom.ZombieBattleground.Common;
using System;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class ChangeTypeAbility : CardAbility
    {
        public override void DoAction()
        {
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

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

                            targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                            {
                                ActionEffectType = (Enumerators.ActionEffectType)Enum.Parse(typeof(Enumerators.ActionEffectType), type.ToString(), true),
                                Target = boardUnitModel
                            });
                        }
                        break;
                }
            }

            Enumerators.ActionType actionType = Targets.Count > 1 ? Enumerators.ActionType.CardAffectingMultipleCards : Enumerators.ActionType.CardAffectingCard;

            PostGameActionReport(actionType, targetEffects);
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
