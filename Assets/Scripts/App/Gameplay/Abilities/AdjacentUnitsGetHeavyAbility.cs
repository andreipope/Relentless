using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AdjacentUnitsGetHeavyAbility : AbilityBase
    {
        public AdjacentUnitsGetHeavyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityActivity == Enumerators.AbilityActivity.PASSIVE)
            {
                InvokeUseAbilityEvent();
            }

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            TakeHeavyToUnits(BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner));
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                TakeHeavyToUnits(BattlegroundController.GetAdjacentUnitsToUnit(TargetUnit));

                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });
            }
        }

        private void TakeHeavyToUnits(List<BoardUnitModel> units)
        {
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (BoardUnitModel unit in units)
            {
                if (unit.IsHeavyUnit)
                    continue;

                unit.SetAsHeavyUnit();

                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.Heavy,
                    Target = unit
                });
            }

            if (targetEffects.Count > 0)
            {
                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                    Caller = GetCaller(),
                    TargetEffects = targetEffects
                });
            }
        }
    }
}
