using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class CostsLessIfCardTypeInHandAbility : AbilityBase
    {
        public Enumerators.Faction Faction;

        public int Value;

        public CostsLessIfCardTypeInHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
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

            if (!PlayerCallerOfAbility.CardsInHand.Contains(BoardUnitModel))
                return;

            int gooCost = PlayerCallerOfAbility.CardsInHand
                .FindAll(x => x.Prototype.Faction == Faction && x != BoardUnitModel).Count * Value;
            CardsController.SetGooCostOfCardInHand(
                PlayerCallerOfAbility,
                BoardUnitModel,
                BoardUnitModel.Prototype.Cost + gooCost,
                BoardCardView
            );
        }
        
        private void CardPlayedHandler(BoardUnitModel boardUnitModel, int position)
        {
            if (boardUnitModel != BoardUnitModel)
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
