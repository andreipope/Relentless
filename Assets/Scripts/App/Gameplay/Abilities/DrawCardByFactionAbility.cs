using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DrawCardByFactionAbility : AbilityBase
    {
        public Enumerators.Faction SetType { get; }

        public DrawCardByFactionAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        protected override void UnitKilledUnitHandler(BoardUnitModel unit)
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

            BoardUnitModel card = PlayerCallerOfAbility.CardsInDeck.FirstOrDefault(x => x.Prototype.Faction == SetType);

            if (card != null)
            {
                PlayerCallerOfAbility.PlayDrawCardVFX();
                CardsController.AddCardToHand(PlayerCallerOfAbility, card);

                InvokeUseAbilityEvent();
            }
        }
    }
}
