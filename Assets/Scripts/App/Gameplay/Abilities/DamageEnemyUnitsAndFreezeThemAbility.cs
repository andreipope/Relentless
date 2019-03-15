using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DamageEnemyUnitsAndFreezeThemAbility : AbilityBase
    {
        public int Value { get; }

        private List<BoardObject> _targets;

        private const int CountOfStun = 1;

        public DamageEnemyUnitsAndFreezeThemAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;

            _targets = new List<BoardObject>();
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

            Player opponent = PlayerCallerOfAbility.Equals(GameplayManager.CurrentPlayer) ?
                GameplayManager.OpponentPlayer :
                GameplayManager.CurrentPlayer;

            _targets.Clear();

            foreach (Enumerators.Target target in AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                        _targets.AddRange(opponent.CardsOnBoard);
                        break;
                    case Enumerators.Target.OPPONENT:
                        _targets.Add(opponent);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }

            InvokeActionTriggered(_targets);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            foreach (BoardObject boardObject in _targets)
            {
                switch (boardObject)
                {
                    case Player player:
                        BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, player);
                        player.Stun(Enumerators.StunType.FREEZE, CountOfStun);
                        break;
                    case BoardUnitModel unit:
                        BattleController.AttackUnitByAbility(GetCaller(), AbilityData, unit);
                        unit.Stun(Enumerators.StunType.FREEZE, CountOfStun);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(boardObject), boardObject, null);
                }
            }
        }
    }
}
