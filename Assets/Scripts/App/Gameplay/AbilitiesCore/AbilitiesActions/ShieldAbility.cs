using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class ShieldAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        boardUnitModel.AddBuffShield();

                        targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Common.Enumerators.ActionEffectType.Guard,
                            Target = boardUnitModel
                        });
                        break;
                }
            }

            PostGameActionReport(Common.Enumerators.ActionType.CardAffectingCard, targetEffects);
        }
    }
}
