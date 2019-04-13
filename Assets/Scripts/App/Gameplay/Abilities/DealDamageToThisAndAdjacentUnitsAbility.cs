using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DealDamageToThisAndAdjacentUnitsAbility : AbilityBase
    {
        private List<CardModel> _units;

        public DealDamageToThisAndAdjacentUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Action(object param = null)
        {
            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            base.Action(param);
            _units = new List<CardModel>();

            int targetIndex = -1;
            for (int i = 0; i < PlayerCallerOfAbility.CardsOnBoard.Count; i++)
            {
                if (PlayerCallerOfAbility.CardsOnBoard[i] == AbilityUnitOwner)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    _units.Add(PlayerCallerOfAbility.CardsOnBoard[targetIndex - 1]);
                }

                if (targetIndex + 1 < PlayerCallerOfAbility.CardsOnBoard.Count)
                {
                    _units.Add(PlayerCallerOfAbility.CardsOnBoard[targetIndex + 1]);
                }
            }

            _units.Add(AbilityUnitOwner);

            foreach (CardModel target in _units)
            {
                target.HandleDefenseBuffer(AbilityData.Value);
            }

            InvokeActionTriggered(_units);
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.END ||
                GameplayManager.CurrentTurnPlayer != PlayerCallerOfAbility)
                return;

            Action();
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            foreach (var unit in _units)
            {
                TakeDamageToUnit(unit);
            }
            _units.Clear();

            InvokeUseAbilityEvent(
                _units
                    .Select(x => new ParametrizedAbilityBoardObject(x))
                    .ToList()
            );

            AbilityProcessingAction?.ForceActionDone();
        }

        private void TakeDamageToUnit(CardModel unit)
        {
            BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, unit);
        }
    }
}
