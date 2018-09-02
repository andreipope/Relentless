using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DrawCardAbility : AbilityBase
    {
        public Enumerators.SetType SetType { get; }

        public DrawCardAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);
            if (SetType == Enumerators.SetType.NONE || SetType != Enumerators.SetType.NONE && PlayerCallerOfAbility.BoardCards.FindAll(x => x.Card.LibraryCard.CardSetType == SetType && x != AbilityUnitOwner).Count > 0)
            {
                if (AbilityTargetTypes.Count > 0)
                {
                    if (AbilityTargetTypes[0] == Enumerators.AbilityTargetType.PLAYER)
                    {
                        CardsController.AddCardToHandFromOtherPlayerDeck(PlayerCallerOfAbility, PlayerCallerOfAbility);
                    }
                    else if (AbilityTargetTypes[0] == Enumerators.AbilityTargetType.OPPONENT)
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
    }
}
