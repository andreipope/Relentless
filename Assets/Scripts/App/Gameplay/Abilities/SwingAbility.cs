using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SwingAbility : AbilityBase
    {
        public int Value { get; }

        private int _targetIndex;

        private BoardUnitModel _unit;

        public SwingAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            _unit = (BoardUnitModel) info;
           
            if (_unit != null)
            {
                InvokeActionTriggered(info);
            }
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);

            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            if (info is BoardUnitModel)
            {
                Action(info);
            }
        }

        private void TakeDamageToUnit(BoardUnitView unit)
        {
            BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, unit.Model);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();
 
            foreach(BoardUnitView unit in BattlegroundController.GetAdjacentUnitsToUnit(_unit))
            {
                TakeDamageToUnit(unit);
            }

            AbilityProcessingAction?.ForceActionDone();
        }
    }
}
