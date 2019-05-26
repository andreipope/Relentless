using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DrawCardByFactionAbility : AbilityBase
    {
        public Enumerators.Faction Faction { get; }

        public DrawCardByFactionAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Faction = ability.Faction;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        protected override void UnitKilledUnitHandler(CardModel unit)
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.KILL_UNIT)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            CardModel card = PlayerCallerOfAbility.CardsInDeck.FirstOrDefault(x => x.Prototype.Faction == Faction);

            if (card != null)
            {
                PlayerCallerOfAbility.PlayDrawCardVFX();
                PlayerCallerOfAbility.PlayerCardsController.AddCardFromDeckToHand(card);

                InvokeUseAbilityEvent();
            }
        }
    }
}
