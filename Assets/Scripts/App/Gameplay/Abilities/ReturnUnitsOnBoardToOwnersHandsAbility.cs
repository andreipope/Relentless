using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReturnUnitsOnBoardToOwnersHandsAbility : AbilityBase
    {
        public int Value { get; }

        public List<CardModel> Units { get; private set; }

        public ReturnUnitsOnBoardToOwnersHandsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            Units = new List<CardModel>();
            Units.AddRange(GameplayManager.CurrentPlayer.CardsOnBoard);
            Units.AddRange(GameplayManager.OpponentPlayer.CardsOnBoard);
            Units =
                Units
                    .Where(card => card != AbilityUnitOwner &&
                        card.CurrentDefense > 0 &&
                        !card.IsDead)
                    .ToList();

            if (Value > 0)
            {
                Units = Units.Where(x => x.Card.InstanceCard.Cost <= Value).ToList();
            }

            foreach(CardModel unit in Units)
            {
                unit.SetUnitActiveStatus(false);
            }

            InvokeActionTriggered(Units);
        }

        private void ReturnBoardUnitToHand(CardModel unit)
        {
            CardsController.ReturnCardToHand(unit);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            foreach (CardModel unit in Units)
            {
                ReturnBoardUnitToHand(unit);
            }

            AbilityProcessingAction?.TriggerActionManually();
        }
    }
}
