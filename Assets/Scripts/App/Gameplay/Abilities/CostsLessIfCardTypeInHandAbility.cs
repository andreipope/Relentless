using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class CostsLessIfCardTypeInHandAbility : AbilityBase
    {
        public Enumerators.SetType setType;

        public int value;

        private int _changedCostOn = 0;

        public CostsLessIfCardTypeInHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            setType = ability.abilitySetType;
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.IN_HAND)

                return;

            playerCallerOfAbility.HandChangedEvent += HandChangedEventHandler;
            playerCallerOfAbility.CardPlayedEvent += CardPlayedEventHandler;

            _timerManager.AddTimer(
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

            if (!playerCallerOfAbility.CardsInHand.Contains(mainWorkingCard))

                return;

            int gooCost = playerCallerOfAbility.CardsInHand.FindAll(x => (x.libraryCard.cardSetType == setType) && (x != mainWorkingCard)).Count * value;

            // gooCost = _changedCostOn;

            // _changedCostOn = gooCost;
            _cardsController.SetGooCostOfCardInHand(playerCallerOfAbility, mainWorkingCard, mainWorkingCard.realCost + gooCost, boardCard);
        }

        private void CardPlayedEventHandler(WorkingCard card)
        {
            if (!card.Equals(mainWorkingCard))

                return;

            playerCallerOfAbility.HandChangedEvent -= HandChangedEventHandler;
            playerCallerOfAbility.CardPlayedEvent -= CardPlayedEventHandler;
        }

        private void HandChangedEventHandler(int obj)
        {
            Action();
        }
    }
}
