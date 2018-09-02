using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DrawCardAbility : AbilityBase
    {
        public Enumerators.SetType SetType;

        public DrawCardAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.Entry)
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

        public override void Action(object info = null)
        {
            base.Action(info);
            if ((SetType == Enumerators.SetType.None) || ((SetType != Enumerators.SetType.None) && (PlayerCallerOfAbility.BoardCards.FindAll(x => (x.Card.LibraryCard.CardSetType == SetType) && (x != AbilityUnitOwner)).Count > 0)))
            {
                if (AbilityTargetTypes.Count > 0)
                {
                    if (AbilityTargetTypes[0] == Enumerators.AbilityTargetType.Player)
                    {
                        CardsController.AddCardToHandFromOtherPlayerDeck(PlayerCallerOfAbility, PlayerCallerOfAbility);
                    }
                    else if (AbilityTargetTypes[0] == Enumerators.AbilityTargetType.Opponent)
                    {
                        CardsController.AddCardToHandFromOtherPlayerDeck(PlayerCallerOfAbility, PlayerCallerOfAbility.Equals(GameplayManager.CurrentPlayer)?GameplayManager.OpponentPlayer:GameplayManager.CurrentPlayer);
                    }
                }
                else
                {
                    CardsController.AddCardToHand(PlayerCallerOfAbility);
                }
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }
    }
}
