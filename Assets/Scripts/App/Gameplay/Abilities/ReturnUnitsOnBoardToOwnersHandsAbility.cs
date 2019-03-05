using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class ReturnUnitsOnBoardToOwnersHandsAbility : AbilityBase
    {
        public int Value { get; }

        public List<BoardUnitView> Units => _units;

        private List<BoardUnitView> _units;

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

            _units = new List<BoardUnitView>();
            _units.AddRange(GameplayManager.CurrentPlayer.BoardCards);
            _units.AddRange(GameplayManager.OpponentPlayer.BoardCards);
            _units =
                _units
                    .Where(card => card.Model != AbilityUnitOwner &&
                        card.Model.CurrentHp > 0 &&
                        !card.Model.IsDead)
                    .ToList();

            if (Value > 0)
            {
                _units = _units.Where(x => x.Model.Card.InstanceCard.Cost <= Value).ToList();
            }

            InvokeActionTriggered(_units);
        }

        private void ReturnBoardUnitToHand(BoardUnitView unit)
        {
            CardsController.ReturnCardToHand(unit);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            foreach (BoardUnitView unit in _units)
            {
                ReturnBoardUnitToHand(unit);
            }

            AbilityProcessingAction?.ForceActionDone();
        }
    }
}
