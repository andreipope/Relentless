using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DealDamageToTargetThatAttackThisAbility : AbilityBase
    {
        public int Damage { get; }

        private BoardObject _targetObject;

        public DealDamageToTargetThatAttackThisAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Damage = ability.Damage;
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

            if (from is BoardUnitModel boardUnit)
            {
                DamageTarget(boardUnit);
            }
        }

        private void DamageTarget(BoardUnitModel unit)
        {
            BattleController.AttackUnitByAbility(BoardUnitModel, AbilityData, unit, Damage);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAttackCard,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = unit,
                            HasValue = true,
                            Value = -AbilityData.Value
                        }
                    }
            });
        }
    }
}
