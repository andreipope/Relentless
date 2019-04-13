using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class SwingAbility : AbilityBase
    {
        public int Value { get; }

        private int _targetIndex;

        private CardModel _unit;

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

            _unit = (CardModel) info;
           
            if (_unit != null)
            {
                InvokeActionTriggered(info);
            }
        }

        protected override void UnitAttackedHandler(IBoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);

            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            if (info is CardModel)
            {
                Action(info);
            }
        }

        private void TakeDamageToUnit(CardModel unit)
        {
            int damage = Value;
            if (damage == 0)
            {
                damage = AbilityUnitOwner.CurrentDamage;
            }

            BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, unit, damage);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();
 
            foreach(CardModel unit in BattlegroundController.GetAdjacentUnitsToUnit(_unit))
            {
                TakeDamageToUnit(unit);
            }

            AbilityProcessingAction?.ForceActionDone();
        }
    }
}
