using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class CostsLessIfCardTypeInHandAbility : AbilityBase
    {
        public Enumerators.SetType SetType;

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

            if (AbilityCallType != Enumerators.AbilityCallType.IN_HAND)
                return;

            PlayerCallerOfAbility.HandChanged += HandChangedHandler;
            PlayerCallerOfAbility.CardPlayed += CardPlayedHandler;

            TimerManager.AddTimer(
                x =>
                {
                    Action();
                },
                null,
                0.5f);
        }

        public override void Action(object info = null)
        {
            base.Action(info);
            if (!PlayerCallerOfAbility.CardsInHand.Contains(MainWorkingCard))
                return;

            int gooCost = PlayerCallerOfAbility.CardsInHand.FindAll(x => x.LibraryCard.CardSetType == SetType && x != MainWorkingCard).Count * Value;
            CardsController.SetGooCostOfCardInHand(PlayerCallerOfAbility, MainWorkingCard, MainWorkingCard.RealCost + gooCost, BoardCard);
        }

        private void CardPlayedHandler(WorkingCard card)
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
