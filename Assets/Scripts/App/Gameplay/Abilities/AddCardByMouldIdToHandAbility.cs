using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AddCardByMouldIdToHandAbility : AbilityBase
    {
        public static readonly MouldId TaintedGoo = new MouldId(155);
        public static readonly MouldId CorruptedGoo = new MouldId(156);

        public MouldId MouldId { get; }

        public AddCardByMouldIdToHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            MouldId = ability.MouldId;
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

            if (MouldId != CorruptedGoo && MouldId != TaintedGoo ||
                (MouldId == CorruptedGoo || MouldId == TaintedGoo) &&
                CardOwnerOfAbility.Faction == PlayerCallerOfAbility.SelfOverlord.Faction)
            {
                BoardUnitModel card = PlayerCallerOfAbility.PlayerCardsController.CreateNewCardByMouldIdAndAddToHand(MouldId);

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = GetCaller(),
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.AddCardToHand,
                            Target = card,
                        }
                    }
                });
            }
        }
    }
}
