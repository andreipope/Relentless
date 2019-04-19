using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

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

        protected override void UnitDamagedHandler(BoardObject from)
        {
            base.UnitDamagedHandler(from);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AT_DEFENCE)
                return;

            if (from is BoardUnitModel unit) 
            {
                if (unit.HasBuffShield)
                    return;
                    
                unit.Stun(Enumerators.StunType.FREEZE, Value);

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = GetCaller(),
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
