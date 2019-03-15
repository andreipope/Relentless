using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AddCardByNameToHandAbility : AbilityBase
    {
        public string Name { get; }

        public AddCardByNameToHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Name = ability.Name;
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

            if (Name != "Corrupted Goo" && Name != "Tainted Goo" ||
                (Name == "Corrupted Goo" || Name == "Tainted Goo") &&
                CardOwnerOfAbility.Faction == PlayerCallerOfAbility.SelfHero.HeroElement)
            {
                BoardUnitModel card = PlayerCallerOfAbility.PlayerCardsController.CreateNewCardByNameAndAddToHand(Name);

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
