using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class ReturnUnitsOnBoardToOwnersHandsAbility : AbilityBase
    {
        public int Value { get; }

        public List<BoardUnitModel> Units { get; private set; }

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

            Units = new List<BoardUnitModel>();
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

            InvokeActionTriggered(Units);
        }

        private void ReturnBoardUnitToHand(BoardUnitModel unit)
        {
            CardsController.ReturnCardToHand(unit);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            foreach (BoardUnitModel unit in Units)
            {
                ReturnBoardUnitToHand(unit);
            }

            AbilityProcessingAction?.ForceActionDone();
        }
    }
}
