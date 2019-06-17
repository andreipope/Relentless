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
            TargetTypes = ability.Targets;
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
                AbilityProcessingAction?.TriggerActionExternally();
                AbilityProcessingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue:true);
            }
        }

        public override void Action(object param = null)
        {
            base.Action(param);

            if (TargetTypes.Contains(Enumerators.Target.PLAYER))
            {
                PlayerCallerOfAbility.PlayerCardsController.RemoveCardFromGraveyard(CardModel);
                CardModel.ResetToInitial();

                Card prototype = new Card(DataManager.CachedCardsLibraryData.GetCardFromName(CardModel.Card.Prototype.Name));
                InstanceId updatedId = new InstanceId(CardModel.InstanceId.Id, Enumerators.ReasonForInstanceIdChange.BackToDeck);
                WorkingCard card = new WorkingCard(prototype, prototype, SelectedPlayer, id: updatedId);
                CardModel cardModel = new CardModel(card);
                PlayerCallerOfAbility.PlayerCardsController.AddCardToDeck(cardModel, true);
            }
            AbilityProcessingAction?.TriggerActionExternally();
        }
    }
}
