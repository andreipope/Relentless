using Loom.ZombieBattleground.Common;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class FreezeAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case BoardUnitModel boardUnitModel:
                        boardUnitModel.Stun(Enumerators.StunType.FREEZE, 1);
                        break;
                    case Player player:
                        player.Stun(Enumerators.StunType.FREEZE, 1);
                        break;
                }

                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Common.Enumerators.ActionEffectType.Freeze,
                    Target = target
                });
            }

            PostGameActionReport(Common.Enumerators.ActionType.CardAffectingCardsWithOverlord, targetEffects);
        }
    }
}
