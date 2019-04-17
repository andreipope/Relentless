using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GainNumberOfLifeForEachDamageThisDealsAbility : AbilityBase
    {
        public int Value { get; }

        private int _damage;

        private bool _isAttacker;

        public GainNumberOfLifeForEachDamageThisDealsAbility(Enumerators.CardKind cardKind, AbilityData ability)
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

            int damageDeal = (int) info;

            AbilityUnitOwner.CurrentDefense = Mathf.Clamp(AbilityUnitOwner.CurrentDefense + (Value * damageDeal), 0, AbilityUnitOwner.MaxCurrentDefense);
        }

        protected override void UnitAttackedHandler(IBoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);

            _isAttacker = isAttacker;

            _damage = damage;
        }

        protected override void UnitAttackedEndedHandler()
        {
            base.UnitAttackedEndedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !_isAttacker)
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            InvokeActionTriggered();
        }

        protected override void VFXAnimationEndedHandler()
        {
            Action(_damage);

            AbilityProcessingAction?.TriggerActionExternally();
        }
    }
}
