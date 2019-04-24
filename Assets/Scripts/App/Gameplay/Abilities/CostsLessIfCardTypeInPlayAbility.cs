using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class CostsLessIfCardTypeInPlayAbility : AbilityBase
    {
        public Enumerators.Faction Faction;

        public int Value;

        private int _lastCost;

        public CostsLessIfCardTypeInPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Faction = ability.Faction;
            Value = ability.Value;
            _lastCost = 0;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.IN_HAND)
                return;

            PlayerCallerOfAbility.PlayerCardsController.BoardChanged += BoardChangedHandler;
            PlayerCallerOfAbility.CardPlayed += CardPlayedHandler;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);
            if (!PlayerCallerOfAbility.CardsInHand.Contains(CardModel))
                return;

            int gooCost = 0;

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.IfHasUnitsWithFactionInPlay)
            {
                if (PlayerCallerOfAbility.CardsOnBoard.FindAll(x => x.Card.Prototype.Faction == Faction).Count > 0)
                {
                    gooCost = -Mathf.Abs(Value);
                }
            }
            else if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.AllAllyUnitsInPlay)
            {
                gooCost = -(Mathf.Abs(Value) * PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard.Count);
            }
            else
            {
                gooCost = PlayerCallerOfAbility.CardsOnBoard.FindAll(x => x.Card.Prototype.Faction == Faction).Count * Value;
            }

            BoardCardView boardCardView = BattlegroundController.GetCardViewByModel<BoardCardView>(CardModel);
            if (_lastCost != 0) 
            {
                CardsController.SetGooCostOfCardInHand(PlayerCallerOfAbility, CardModel,
                -_lastCost, boardCardView);
            }

            CardsController.SetGooCostOfCardInHand(PlayerCallerOfAbility, CardModel,
                gooCost, boardCardView);

            _lastCost = gooCost;
        }

        private void CardPlayedHandler(CardModel cardModel, int position)
        {
            if (cardModel != CardModel)
                return;

            PlayerCallerOfAbility.PlayerCardsController.BoardChanged -= BoardChangedHandler;
            PlayerCallerOfAbility.CardPlayed -= CardPlayedHandler;
        }

        protected override void BoardChangedHandler(int count)
        {
            Action();
        }
    }
}
