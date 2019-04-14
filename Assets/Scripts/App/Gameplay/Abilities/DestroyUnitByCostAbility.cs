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

        private CardModel _unit;

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

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
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

            DestroyUnit(_unit);
        }

        private CardModel GetRandomUnit()
        {
            List<CardModel> units = new List<CardModel>();

            if (PredefinedTargets != null)
            {
                units = PredefinedTargets.Select(x => x.BoardObject).Cast<CardModel>().ToList();
            }
            else
            {
                if (AbilityData.Targets.Contains(Enumerators.Target.OPPONENT_CARD))
                {
                    units.AddRange(GetOpponentOverlord().CardsOnBoard.Where(x => x.Card.InstanceCard.Cost <= Cost).ToList());
                }

                if (AbilityData.Targets.Contains(Enumerators.Target.PLAYER_CARD))
                {
                    units.AddRange(PlayerCallerOfAbility.CardsOnBoard.Where(x => x.Card.InstanceCard.Cost <= Cost).ToList());
                }
            }

            if (units != null && units.Count > 0)
            {
                return GetRandomUnits(units, 1)[0];
            }

            return null;
        }

        private void DestroyUnit(CardModel unit)
        {
            if (unit != null)
            {
                InvokeUseAbilityEvent(
                    new List<ParametrizedAbilityBoardObject>
                    {
                        new ParametrizedAbilityBoardObject(unit)
                    }
                );

                BattlegroundController.DestroyBoardUnit(unit, false);

                if(AbilityUnitOwner != null && AbilityUnitOwner.Prototype.MouldId == TorchCardId)
                {
                    _checkForCardOwner = true;
                }

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = AbilityUnitOwner,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                            Target = unit
                        }
                    },
                    CheckForCardOwner = _checkForCardOwner,
                    Model = AbilityUnitOwner
                });
            }
        }
    }
}
