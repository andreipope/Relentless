// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DrawCardAbility : AbilityBase
    {
        public Enumerators.SetType setType;

        public DrawCardAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.setType = ability.abilitySetType;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.AT_START)
                return;

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

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if ((setType == Enumerators.SetType.NONE) ||
                (setType != Enumerators.SetType.NONE && playerCallerOfAbility.BoardCards.FindAll(x => x.Card.libraryCard.cardSetType == setType).Count > 0))
            {

                if (abilityTargetTypes.Count > 0)
                {
                    if (abilityTargetTypes[0] == Enumerators.AbilityTargetType.PLAYER)
                        _cardsController.AddCardToHandFromOtherPlayerDeck(playerCallerOfAbility, playerCallerOfAbility);
                    else if(abilityTargetTypes[0] == Enumerators.AbilityTargetType.OPPONENT)
                        _cardsController.AddCardToHandFromOtherPlayerDeck(playerCallerOfAbility, playerCallerOfAbility.Equals(_gameplayManager.CurrentPlayer) ? _gameplayManager.OpponentPlayer : _gameplayManager.CurrentPlayer);
                }
                else
                    _cardsController.AddCardToHand(playerCallerOfAbility);
            }
        }
    }
}{