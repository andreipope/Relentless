using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class TakeControlEnemyUnitAbility : AbilityBase
    {
        private int Count { get; }

        public List<BoardUnitModel> MovedUnits => _movedUnits;

        private List<BoardUnitModel> _movedUnits;

        public TakeControlEnemyUnitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                if (PredefinedTargets != null)
                {
                    TakeControlEnemyUnit(PredefinedTargets.Select(x => x.BoardObject as BoardUnitModel).ToList()
                        .FindAll(card => card.CurrentDefense > 0 && !card.IsDead));
                }
                else
                {
                    TakeControlEnemyUnit(GetRandomEnemyUnits(Count));
                }
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                TakeControlEnemyUnit(new List<BoardUnitModel>() { TargetUnit });
            }
        }

        private void TakeControlEnemyUnit(List<BoardUnitModel> units)
        {
            _movedUnits = new List<BoardUnitModel>();

            foreach (BoardUnitModel unit in units)
            {
                if (PlayerCallerOfAbility.CardsOnBoard.Count >= PlayerCallerOfAbility.MaxCardsInPlay)
                    break;

                _movedUnits.Add(unit);
            }

            InvokeActionTriggered(_movedUnits);
        }

        private void TakeControlEnemyUnitEnded()
        {
            foreach (BoardUnitModel unit in _movedUnits)
            {
                BattlegroundController.TakeControlUnit(PlayerCallerOfAbility, unit);
            }

            if (_movedUnits.Count > 0)
            {
                InvokeUseAbilityEvent(
                    _movedUnits
                        .Select(x => new ParametrizedAbilityBoardObject(x))
                        .ToList()
                );
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            TakeControlEnemyUnitEnded();
        }
    }
}
