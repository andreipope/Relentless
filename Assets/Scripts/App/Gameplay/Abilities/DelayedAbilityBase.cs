using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DelayedAbilityBase : AbilityBase
    {
        public int Delay { get; }

        private int _delayedTurnsLeft;

        public bool ActionDone { get; protected set; }

        public DelayedAbilityBase(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Delay = ability.Delay;
            _delayedTurnsLeft = Delay;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (CheckActionEnded())
                return;
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (CheckActionEnded())
                return;

            if (GameplayManager.CurrentTurnPlayer == PlayerCallerOfAbility)
            {
                CountDelay();
            }
        }

        private void CountDelay()
        {
            _delayedTurnsLeft--;

            CheckDelayEnded();           
        }

        private void CheckDelayEnded()
        {
            if (_delayedTurnsLeft <= 0 && !ActionDone)
            {
                Action();

                ActionDone = true;

                Deactivate();
            }
        }

        private bool CheckActionEnded()
        {
            if (GetCaller() == null)
            {
                ActionDone = true;
            }

            if(ActionDone)
            {
                Deactivate();
            }

            return ActionDone;
        }
    }
}
