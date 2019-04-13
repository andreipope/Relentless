using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class CostsLessAbility : AbilityBase
    {
        public int Cost;

        public CostsLessAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Cost = ability.Cost;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.IN_HAND)
                return;

            PlayerCallerOfAbility.PlayerCardsController.HandChanged += HandChangedHandler;
            PlayerCallerOfAbility.CardPlayed += CardPlayedHandler;

            InternalTools.DoActionDelayed(() =>
            {
                Action();
            }, 0.5f);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (!PlayerCallerOfAbility.CardsInHand.Contains(CardModel))
                return;

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.OnlyThisCardInHand)
            {
                int cost = CardModel.Prototype.Cost;

                if (PlayerCallerOfAbility.PlayerCardsController.CardsInHand.Count == 1)
                {
                    cost = Cost;
                }

                CardsController.SetGooCostOfCardInHand(
                    PlayerCallerOfAbility,
                    CardModel,
                    cost,
                    BattlegroundController.GetCardViewByModel<BoardCardView>(CardModel)
                );
            }
        }
        
        private void CardPlayedHandler(CardModel cardModel, int position)
        {
            if (cardModel != CardModel)
                return;

            PlayerCallerOfAbility.PlayerCardsController.HandChanged -= HandChangedHandler;
            PlayerCallerOfAbility.CardPlayed -= CardPlayedHandler;
        }

        private void HandChangedHandler(int obj)
        {
            Action();
        }
    }
}
