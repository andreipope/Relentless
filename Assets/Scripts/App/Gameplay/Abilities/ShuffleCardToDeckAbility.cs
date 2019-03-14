using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class ShuffleCardToDeckAbility : AbilityBase
    {
        public List<Enumerators.Target> TargetTypes { get; }

        public ShuffleCardToDeckAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetTypes = ability.AbilityTarget;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            Action();
        }

        protected override void UnitHpChangedHandler(int oldValue, int newValue)
        {
            base.UnitHpChangedHandler(oldValue, newValue);

            if (AbilityUnitOwner.CurrentDefense <= 0) 
            {   
                AbilityProcessingAction?.ForceActionDone();
                AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue:true);
            }
        }

        public override void Action(object param = null)
        {
            base.Action(param);

            if (TargetTypes.Contains(Enumerators.Target.PLAYER))
            {
                // FIXME: doesn't this cause de-sync?
                PlayerCallerOfAbility.LocalCardsController.AddCardToDeck(BoardUnitModel, true);
            }
            AbilityProcessingAction?.ForceActionDone();
        }
    }
}
