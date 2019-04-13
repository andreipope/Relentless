using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class ChangeCostAbility : AbilityBase
    {
        public int Cost;
        public Enumerators.CardKind TargetCardKind;


        private List<CardModel> _updatedCostUnits;

        public ChangeCostAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Cost = ability.Cost;
            TargetCardKind = ability.TargetKind;

            _updatedCostUnits = new List<CardModel>();
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            if (AbilityTargets.Contains(Enumerators.Target.ITSELF))
            {
                CardsController.SetGooCostOfCardInHand(
                        PlayerCallerOfAbility,
                        CardModel,
                        CardModel.Card.InstanceCard.Cost + Cost
                    );
            }
        }

        protected override void ChangeAuraStatusAction(bool status)
        {
            base.ChangeAuraStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            foreach (Enumerators.Target target in AbilityData.Targets)
            {
                switch (target)
                {
                    case Enumerators.Target.PLAYER:
                        ChangeCostByStatus(PlayerCallerOfAbility, status);
                        break;
                    case Enumerators.Target.OPPONENT:
                        ChangeCostByStatus(GetOpponentOverlord(), status);
                        break;
                }
            }
        }

        protected override void HandChangedHandler(int count)
        {
            base.HandChangedHandler(count);

            ChangeAuraStatusAction(false);
            ChangeAuraStatusAction(true);
        }

        private void ChangeCostByStatus(Player player, bool status)
        {
            IEnumerable<CardModel> units = null;

            if (!status)
            {
                units = _updatedCostUnits;
            }
            else
            {
                if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.AllCardsInHand)
                {
                    units = player.PlayerCardsController.CardsInHand.
                                      Where(unit => unit.Card.Prototype.Kind == TargetCardKind);
                }

                _updatedCostUnits = units.ToList();
            }

            if (units != null)
            {
                int calculatedCost = 0;

                foreach (CardModel boardUnit in units)
                {
                    if (status)
                    {
                        calculatedCost = boardUnit.Card.InstanceCard.Cost + Cost;
                    }
                    else
                    {
                        calculatedCost = boardUnit.Card.InstanceCard.Cost - Cost;
                    }

                    CardsController.SetGooCostOfCardInHand(
                        player,
                        boardUnit,
                        calculatedCost
                    );
                }
            }
        }
    }
}
