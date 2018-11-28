using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class DestroyUnitByCostAbility : AbilityBase
    {
        public int Cost { get; }

        public DestroyUnitByCostAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Cost = ability.Cost;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                DestroyUnit(GetRandomUnit());
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null);

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
                if (AbilityData.AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD))
                {
                    units.AddRange(GetOpponentOverlord().BoardCards.Where(x => x.Model.Card.InstanceCard.Cost <= Cost).Select(x => x.Model).ToList());
                }

                if (AbilityData.AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.PLAYER_CARD))
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
                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>() { unit }, AbilityData.AbilityType,
                                                     Protobuf.AffectObjectType.Types.Enum.Character);

                BattlegroundController.DestroyBoardUnit(unit);

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
                    }
                });
            }
        }
    }
}
