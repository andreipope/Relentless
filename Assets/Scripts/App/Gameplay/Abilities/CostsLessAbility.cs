using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class CostsLessAbility : AbilityBase
    {
        public int Cost;
        
        private ValueHistory _currentValue;
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

            PlayerCallerOfAbility.CardPlayed += CardPlayedHandler;

            if (AbilityData.SubTrigger != Enumerators.AbilitySubTrigger.DuringCardInHand)
            {
                InternalTools.DoActionDelayed(() =>
                {
                    Action();
                }, 0.5f);
            }
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (!GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (!PlayerCallerOfAbility.CardsInHand.Contains(CardModel))
            {
                Deactivate();
                return;
            }

            BoardCardView boardCardView = BattlegroundController.GetCardViewByModel<BoardCardView>(CardModel);
            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.OnlyThisCardInHand)
            {
                int cost = CardModel.Prototype.Cost;

                if (PlayerCallerOfAbility.PlayerCardsController.CardsInHand.
                    FindAll(item => item.Card.Prototype.Kind == Enumerators.CardKind.CREATURE).Count == 1)
                {
                    cost = Cost;
                }

                Debug.LogWarning("BOH " + cost + " " + Cost + " " + PlayerCallerOfAbility.IsLocalPlayer);

                if (cost == Cost)
                {
                    if (_currentValue == null)
                    {
                        _currentValue = CardsController.SetGooCostOfCardInHand(
                            PlayerCallerOfAbility,
                            CardModel,
                            cost,
                            boardCardView,
                            forced: true
                        );
                    }
                }
                else
                {
                    if (_currentValue != null)
                    {
                        _currentValue.Enabled = false;
                        _currentValue = null;

                        CardsController.SetGooCostOfCardInHand(
                            PlayerCallerOfAbility,
                            CardModel,
                            0,
                            boardCardView
                        );

                        Debug.LogWarning("RESULT " + CardModel.CurrentCost + " " + PlayerCallerOfAbility.IsLocalPlayer);
                    }
                }
            }
            else if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.DuringCardInHand)
            {
                CardsController.SetGooCostOfCardInHand(
                       PlayerCallerOfAbility,
                       CardModel,
                       -Cost,
                       boardCardView
                   );
            }
        }
        
        private void CardPlayedHandler(CardModel cardModel, int position)
        {
            if (cardModel != CardModel)
                return;

            PlayerCallerOfAbility.CardPlayed -= CardPlayedHandler;
        }

        protected override void HandChangedHandler(int obj)
        {
            if (AbilityData.SubTrigger != Enumerators.AbilitySubTrigger.DuringCardInHand)
            {
                Action();
            }
        }
    }
}
