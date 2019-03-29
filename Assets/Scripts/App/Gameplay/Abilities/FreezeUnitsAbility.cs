using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class FreezeUnitsAbility : AbilityBase
    {
        public int Value { get; }

        private Player _opponent;

        public FreezeUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            foreach (BoardUnitModel unit in _opponent.CardsOnBoard)
            {
                unit.Stun(Enumerators.StunType.FREEZE, Value);
            }

            InvokeUseAbilityEvent();

            AbilityProcessingAction?.ForceActionDone();
        }

        public override void Activate()
        {
            base.Activate();

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            _opponent = PlayerCallerOfAbility == GameplayManager.CurrentPlayer ?
            GameplayManager.OpponentPlayer :
            GameplayManager.CurrentPlayer;

            InvokeActionTriggered(_opponent);
        }

        public override void Action(object info = null)
        {
            base.Action(info);
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();
        }
    }
}
