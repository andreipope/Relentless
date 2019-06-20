using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class FreezeNumberOfRandomAllyAbility : AbilityBase
    {
        public int Value { get; }

        public int Turns { get; }

        private List<IBoardObject> _allies;

        public FreezeNumberOfRandomAllyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Turns = ability.Turns;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            AbilityProcessingAction?.TriggerActionExternally();
            AbilityProcessingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            _allies = new List<IBoardObject>();

            if (PredefinedTargets != null)
            {
                _allies = PredefinedTargets.Select(x => x.BoardObject).ToList();
            }
            else
            {
                _allies.AddRange(PlayerCallerOfAbility.CardsOnBoard);
                _allies.Remove(AbilityUnitOwner);
                _allies.Add(PlayerCallerOfAbility);

                _allies = InternalTools.GetRandomElementsFromList(_allies, Value);
            }

            InvokeActionTriggered(_allies);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            for (int i = 0; i < _allies.Count; i++)
            {
                object ally = _allies[i];
                switch (ally)
                {
                    case Player player:
                        player.Stun(Enumerators.StunType.FREEZE, Turns);
                        break;
                    case CardModel unit:
                        unit.Stun(Enumerators.StunType.FREEZE, Turns);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ally), ally, null);
                }
            }

            InvokeUseAbilityEvent(
                _allies
                    .Select(x => new ParametrizedAbilityBoardObject(x))
                    .ToList()
            );

            AbilityProcessingAction?.TriggerActionExternally();
        }
    }
}
