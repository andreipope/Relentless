// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class CostsLessIfCardTypeInHandAbility : AbilityBase
    {
        public Enumerators.SetType setType;

        public CostsLessIfCardTypeInHandAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            setType = ability.abilitySetType;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.IN_HAND)
                return;

            playerCallerOfAbility.HandChangedEvent += HandChangedEventHandler;
            playerCallerOfAbility.CardPlayedEvent += CardPlayedEventHandler;

            Action();
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

            int value = playerCallerOfAbility.CardsInHand.FindAll(x => x.libraryCard.cardSetType.Equals(setType) && x != mainWorkingCard).Count;

            _cardsController.SetGooCostOfCardInHand(playerCallerOfAbility, mainWorkingCard, mainWorkingCard.initialCost - value, boardCard);
        }
    }
}
