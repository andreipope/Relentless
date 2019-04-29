using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DamageTargetAdjustmentsAbility : AbilityBase
    {
        private int Value { get; }

        private List<BoardUnitModel> _targetUnits;

        public DamageTargetAdjustmentsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (AbilityEffect)
            {
                case Enumerators.AbilityEffect.TARGET_ADJUSTMENTS_AIR:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/WhirlwindVFX");
                    break;
                default:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetToxicAttack");
                    break;
            }

            if(AbilityTrigger != Enumerators.AbilityTrigger.ENTRY &&
               AbilityActivity != Enumerators.AbilityActivity.ACTIVE)
            {
                InvokeUseAbilityEvent();
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                DamageTargetAdjacent(TargetUnit);
            }
        }

        protected override void UnitAttackedHandler(BoardObject boardObject, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(boardObject, damage, isAttacker);

            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            if (boardObject is BoardUnitModel unit)
            {
                DamageTargetAdjacent(unit);
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            ActionCompleted();

            AbilityProcessingAction?.ForceActionDone();
        }

        private void DamageTargetAdjacent(BoardUnitModel targetUnit)
        {
            if (targetUnit == null)
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            _targetUnits = new List<BoardUnitModel>();

            _targetUnits.Add(targetUnit);
            _targetUnits.AddRange(BattlegroundController.GetAdjacentUnitsToUnit(targetUnit));

            foreach (BoardUnitModel target in _targetUnits)
            {
                target.HandleDefenseBuffer(Value);
            }

            InvokeActionTriggered(_targetUnits);
        }

        private void ActionCompleted()
        {
            if (_targetUnits == null || _targetUnits.Count == 0)
                return;

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (BoardUnitModel unit in _targetUnits)
            {
                BattleController.AttackUnitByAbility(GetCaller(), AbilityData, unit, Value);

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                    Target = unit,
                    HasValue = true,
                    Value = -Value
                });
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = GetCaller(),
                TargetEffects = TargetEffects
            });

            InvokeUseAbilityEvent(_targetUnits.Select(targ => new ParametrizedAbilityBoardObject(targ)).ToList());
        }
    }
}
