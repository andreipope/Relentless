using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class DestroyAbility : CardAbility
    {
        public override void DoAction()
        {
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        BattlegroundController.DestroyBoardUnit(boardUnitModel, false);

                        targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Common.Enumerators.ActionEffectType.DeathMark,
                            Target = boardUnitModel
                        });
                        break;
                }
            }

            PostGameActionReport(Common.Enumerators.ActionType.CardAffectingCard, targetEffects);
        }
    }
}
