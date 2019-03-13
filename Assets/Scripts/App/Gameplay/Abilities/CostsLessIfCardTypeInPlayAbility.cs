using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class CostsLessIfCardTypeInPlayAbility : AbilityBase
    {
        public Enumerators.Faction Faction;

        public int Value;

        public CostsLessIfCardTypeInPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Faction = ability.Faction;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.IN_HAND)
                return;

            PlayerCallerOfAbility.BoardChanged += BoardChangedHandler;
            PlayerCallerOfAbility.CardPlayed += CardPlayedHandler;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);
            if (!PlayerCallerOfAbility.CardsInHand.Contains(BoardUnitModel))
                return;

            int gooCost = 0;

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.IfHasUnitsWithFactionInPlay)
            {
                if (PlayerCallerOfAbility.BoardCards.FindAll(x => x.Model.Card.Prototype.Faction == Faction).Count > 0)
                {
                    gooCost = -Mathf.Abs(Value);
                }
            }
            else
            {
                gooCost = PlayerCallerOfAbility.BoardCards.FindAll(x => x.Model.Card.Prototype.Faction == Faction).Count * Value;
            }

            CardsController.SetGooCostOfCardInHand(PlayerCallerOfAbility, BoardUnitModel,
                BoardUnitModel.Prototype.Cost + gooCost, boardCardView);
        }

        private void CardPlayedHandler(BoardUnitModel boardUnitModel, int position)
        {
            if (!boardUnitModel.Equals(BoardUnitModel))
                return;

            PlayerCallerOfAbility.BoardChanged -= BoardChangedHandler;
            PlayerCallerOfAbility.CardPlayed -= CardPlayedHandler;
        }

        private void BoardChangedHandler(int obj)
        {
            Action();
        }
    }
}
