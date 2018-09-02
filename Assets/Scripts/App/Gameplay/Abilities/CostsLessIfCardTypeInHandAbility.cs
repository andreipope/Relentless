using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class CostsLessIfCardTypeInHandAbility : AbilityBase
    {
        public Enumerators.SetType SetType;

        public int Value;

        private int _changedCostOn = 0;

        public CostsLessIfCardTypeInHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.InHand)
                return;

            PlayerCallerOfAbility.HandChangedEvent += HandChangedEventHandler;
            PlayerCallerOfAbility.CardPlayedEvent += CardPlayedEventHandler;

            TimerManager.AddTimer(
                x =>
                {
                    Action();
                },
                null,
                0.5f);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (!PlayerCallerOfAbility.CardsInHand.Contains(MainWorkingCard))
                return;

            int gooCost = PlayerCallerOfAbility.CardsInHand.FindAll(x => (x.LibraryCard.CardSetType == SetType) && (x != MainWorkingCard)).Count * Value;

            // gooCost = _changedCostOn;

            // _changedCostOn = gooCost;
            CardsController.SetGooCostOfCardInHand(PlayerCallerOfAbility, MainWorkingCard, MainWorkingCard.RealCost + gooCost, BoardCard);
        }

        private void CardPlayedEventHandler(WorkingCard card)
        {
            if (!card.Equals(MainWorkingCard))
                return;

            PlayerCallerOfAbility.HandChangedEvent -= HandChangedEventHandler;
            PlayerCallerOfAbility.CardPlayedEvent -= CardPlayedEventHandler;
        }

        private void HandChangedEventHandler(int obj)
        {
            Action();
        }
    }
}
