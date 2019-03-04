using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AdjacentUnitsGetGuardAbility : AbilityBase
    {
        public AdjacentUnitsGetGuardAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (BoardUnitView unit in BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner))
            {
                unit?.Model.AddBuffShield();

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.Guard,
                    Target = unit.Model
                });
            }

            if (TargetEffects.Count > 0)
            {
                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = GetCaller(),
                    TargetEffects = TargetEffects
                });
            }
        }
    }
}
