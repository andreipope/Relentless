using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class DestroyUnitByCostAbility : AbilityBase
    {
        private const int TorchCardId = 147;

        public int Cost { get; }

        private BoardUnitModel _unit;

        private bool _isRandom;

        private bool _checkForCardOwner;

        public DestroyUnitByCostAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Cost = ability.Cost;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                _isRandom = true;
                _unit = GetRandomUnit();
                InvokeActionTriggered(_unit);

            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                _isRandom = false;
                _unit = TargetUnit;
                InvokeActionTriggered(_unit);
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            if(_isRandom)
            {
                DestroyUnit(_unit);
            }
            else
            {
                AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

                DestroyUnit(TargetUnit);

                AbilityProcessingAction?.ForceActionDone();
            }
        }

        private BoardUnitModel GetRandomUnit()
        {
            List<BoardUnitModel> units = new List<BoardUnitModel>();

            if (PredefinedTargets != null)
            {
                units = PredefinedTargets.Select(x => x.BoardObject).Cast<BoardUnitModel>().ToList();
            }
            else
            {
                if (AbilityData.AbilityTarget.Contains(Enumerators.AbilityTarget.OPPONENT_CARD))
                {
                    units.AddRange(GetOpponentOverlord().BoardCards.Where(x => x.Model.Card.InstanceCard.Cost <= Cost).Select(x => x.Model).ToList());
                }

                if (AbilityData.AbilityTarget.Contains(Enumerators.AbilityTarget.PLAYER_CARD))
                {
                    units.AddRange(PlayerCallerOfAbility.BoardCards.Where(x => x.Model.Card.InstanceCard.Cost <= Cost).Select(x => x.Model).ToList());
                }
            }

            if (units != null && units.Count > 0)
            {
                return InternalTools.GetRandomElementsFromList(units, 1)[0];
            }

            return null;
        }

        private void DestroyUnit(BoardUnitModel unit)
        {
            if (unit != null && unit.Card.InstanceCard.Cost <= Cost)
            {
                InvokeUseAbilityEvent(
                    new List<ParametrizedAbilityBoardObject>
                    {
                        new ParametrizedAbilityBoardObject(unit)
                    }
                );

                BattlegroundController.DestroyBoardUnit(unit, false);

                BoardUnitModel card = BoardSpell?.BoardUnitModel;

                if(card != null && card.Prototype.MouldId == TorchCardId)
                {
                    _checkForCardOwner = true;
                }

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = GetCaller(),
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                            Target = unit
                        }
                    },
                    CheckForCardOwner = _checkForCardOwner,
                    BoardUnitModel = card
                });
            }
        }
    }
}
