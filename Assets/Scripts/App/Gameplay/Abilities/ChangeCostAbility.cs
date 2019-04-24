using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        }

        protected override void ChangeAuraStatusAction(bool status)
        {
            base.ChangeAuraStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            HandleTargets(status);
        }

        protected override void HandChangedHandler(int count)
        {
            base.HandChangedHandler(count);

            HandleTargets(true, true);
        }

        private void HandleTargets(bool status, bool refresh = false)
        {
            foreach (Enumerators.Target target in AbilityData.Targets)
            {
                switch (target)
                {
                    case Enumerators.Target.PLAYER:
                        ChangeCostByStatus(PlayerCallerOfAbility, status, refresh);
                        break;
                    case Enumerators.Target.OPPONENT:
                        ChangeCostByStatus(GetOpponentOverlord(), status, refresh);
                        break;
                }
            }
        }

        private void ChangeCostByStatus(Player player, bool status, bool refresh = false)
        {
            IEnumerable<CardModel> units = null;

            if (!status)
            {
                units = new List<CardModel>(_updatedCostUnits);
            }
            else
            {
                if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.AllCardsInHand)
                {
                    units = player.PlayerCardsController.CardsInHand.
                                      Where(unit => unit.Card.Prototype.Kind == TargetCardKind);
                }
            }

            if (units != null)
            {
                int calculatedCost = 0;

                foreach (CardModel boardUnit in units)
                {
                    calculatedCost = boardUnit.CurrentCost;

                    if (!refresh || !_updatedCostUnits.Contains(boardUnit))
                    {
                        calculatedCost += status ? Cost : - Cost;
                    }

                    if (boardUnit.CurrentCost == calculatedCost)
                        continue;

                    CardsController.SetGooCostOfCardInHand(
                        player,
                        boardUnit,
                        status ? Cost : - Cost
                    );
                    
                    if (!_updatedCostUnits.Contains(boardUnit))
                    {
                        _updatedCostUnits.Add(boardUnit);
                    }
                }
            }

            if (!status)
            {
                _updatedCostUnits.Clear();
            }
        }
    }
}
