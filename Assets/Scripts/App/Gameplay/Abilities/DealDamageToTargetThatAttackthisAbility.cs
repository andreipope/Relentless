using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DealDamageToTargetThatAttackThisAbility : AbilityBase
    {
        public int Damage { get; }

        private IBoardObject _targetObject;

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

        protected override void UnitDamagedHandler(IBoardObject from)
        {
            base.UnitDamagedHandler(from);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AT_DEFENCE)
                return;

            if (from is CardModel boardUnit)
            {
                DamageTarget(boardUnit);
            }
        }

        private void DamageTarget(CardModel unit)
        {
            BattleController.AttackUnitByAbility(CardModel, AbilityData, unit, Damage);

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAttackCard,
                Caller = AbilityUnitOwner,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = _targetObject,
                            HasValue = true,
                            Value = -AbilityData.Value
                        }
                    }
            });
        }
    }
}
