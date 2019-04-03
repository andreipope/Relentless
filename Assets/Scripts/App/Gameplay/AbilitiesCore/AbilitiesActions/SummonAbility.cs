using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class SummonAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();


            if (AbilitiesController.HasSubTrigger(this, Common.Enumerators.AbilitySubTrigger.ByName))
            {
                if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Name))
                {
                    string name = AbilitiesController.GetParameterValue<string>(GenericParameters,
                                                   Common.Enumerators.AbilityParameter.Name);

                    int count = 1;

                    if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Name))
                    {
                        count = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                                           Common.Enumerators.AbilityParameter.Name);
                    }

                    for (int i = 0; i < count; i++)
                    {
                        //PlayerOwner.PlayerCardsController.SpawnUnitOnBoard(name, ItemPosition.End, IsPVPAbility);
                    }
                }
            }

            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                       

                        //targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                        //{
                        //    ActionEffectType = Common.Enumerators.ActionEffectType.Guard,
                        //    Target = boardUnitModel
                        //});
                        break;
                }
            }

            //PostGameActionReport(Common.Enumerators.ActionType.CardAffectingCard, targetEffects);
        }
    }
}
