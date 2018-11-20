using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

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
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null);

                Action();
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (TargetUnit != null && TargetUnit.Card.InstanceCard.Cost <= Cost)
            {
                BattlegroundController.DestroyBoardUnit(TargetUnit);


                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>() { TargetUnit }, AbilityData.AbilityType,
                                                         Protobuf.AffectObjectType.Types.Enum.Character);

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = GetCaller(),
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                            Target = TargetUnit
                        }
                    }
                });

                AbilityProcessingAction?.ForceActionDone();
            }
        }
    }
}
