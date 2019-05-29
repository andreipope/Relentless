using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class EnemyThatAttacksBecomeFrozenAbility : AbilityBase
    {
        public int Value { get; } = 1;

        public EnemyThatAttacksBecomeFrozenAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
        }

        protected override void UnitDamagedHandler(IBoardObject from, bool fromGettingAttacked = false)
        {
            base.UnitDamagedHandler(from);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AT_DEFENCE)
                return;

            if (from is CardModel unit)
            {
                if (unit.HasBuffShield)
                    return;

                if (CardModel.CurrentDamage <= 0 || from is CardModel && -Mathf.Min(CardModel.CurrentDamage, ((CardModel) from).MaximumDamageFromAnySource) <= 0)
                    return;

                unit.Stun(Enumerators.StunType.FREEZE, Value);

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = AbilityUnitOwner,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        {
                            new PastActionsPopup.TargetEffectParam()
                            {
                                ActionEffectType = Enumerators.ActionEffectType.Freeze,
                                Target = unit,
                            }
                        }
                });
            }
        }
    }
}
