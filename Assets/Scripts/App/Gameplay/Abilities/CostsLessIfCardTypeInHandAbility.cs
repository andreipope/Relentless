using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class CostsLessIfCardTypeInHandAbility : AbilityBase
    {
        public Enumerators.Faction SetType;

        public int Value;

        public CostsLessIfCardTypeInHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.IN_HAND)
                return;

            PlayerCallerOfAbility.HandChanged += HandChangedHandler;
            PlayerCallerOfAbility.CardPlayed += CardPlayedHandler;

            InternalTools.DoActionDelayed(() =>
            {
                Action();
            }, 0.5f);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (!PlayerCallerOfAbility.CardsInHand.Contains(MainWorkingCard))
                return;

            int gooCost = PlayerCallerOfAbility.CardsInHand
                .FindAll(x => x.LibraryCard.Faction == SetType && x != MainWorkingCard).Count * Value;
            CardsController.SetGooCostOfCardInHand(
                PlayerCallerOfAbility,
                MainWorkingCard,
                MainWorkingCard.LibraryCard.Cost + gooCost,
                BoardCard
            );
        }
        
        private void CardPlayedHandler(WorkingCard card, int position)
        {
            if (!card.Equals(MainWorkingCard))
                return;

            PlayerCallerOfAbility.HandChanged -= HandChangedHandler;
            PlayerCallerOfAbility.CardPlayed -= CardPlayedHandler;
        }

        private void HandChangedHandler(int obj)
        {
            Action();
        }
    }
}
