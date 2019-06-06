using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class GainStatsOfAdjacentUnitsAbility : AbilityBase
    {
        private int _addedDamage,
                    _addedDefense;

        private List<CardModel> _adjacentUnits;
        public GainStatsOfAdjacentUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            _adjacentUnits = new List<CardModel>();
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
        }

        public override void Update()
        {
            base.Update();
            
            if(AbilityUnitOwner.IsDead || AbilityUnitOwner.CurrentDefense <= 0 || !LastAuraState)
                return;
                
            int currentDamage = 0;
            int currentDefense = 0;

            foreach (CardModel unit in _adjacentUnits) {
                currentDamage += unit.CurrentDamage;
                currentDefense += unit.CurrentDefense;
            }

            if (currentDamage != _addedDamage || currentDefense != _addedDefense) 
            {
                TriggerAdjacentsRecheck();
            }
        }

        protected override void ChangeAuraStatusAction(bool status)
        {
            base.ChangeAuraStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            if (status)
            {
                GainStats(AbilityUnitOwner, GetAdjacentUnits());
            }
            else
            {
                RestoreGainedStats(AbilityUnitOwner, _addedDamage, _addedDefense);
                ResetStoredStats();
            }
        }

        protected override void BoardChangedHandler(int count)
        {
            base.BoardChangedHandler(count);

            if(AbilityUnitOwner.IsUnitActive && !AbilityUnitOwner.IsDead && AbilityUnitOwner.CurrentDefense > 0 && LastAuraState)
            {
                TriggerAdjacentsRecheck();
            }
        }

        private void TriggerAdjacentsRecheck () 
        {
            int oldAddedDefense = _addedDefense;
            int oldAddedDamage = _addedDamage;
            GainStats(AbilityUnitOwner, GetAdjacentUnits());
            RestoreGainedStats(AbilityUnitOwner, oldAddedDamage, oldAddedDefense);
        }

        private void GainStats(CardModel boardUnit, List<CardModel> boardUnits)
        {
            _addedDefense = 0;
            _addedDamage = 0;

            foreach (CardModel cardModel in boardUnits)
            {
                _addedDefense += cardModel.CurrentDefense;
                _addedDamage += cardModel.CurrentDamage;
            }

            boardUnit.BuffedDefense += _addedDefense;
            boardUnit.AddToCurrentDefenseHistory(_addedDefense, Enumerators.ReasonForValueChange.AbilityBuff);
            boardUnit.BuffedDamage += _addedDamage;
            boardUnit.AddToCurrentDamageHistory(_addedDamage, Enumerators.ReasonForValueChange.AbilityBuff);
        }

        private void RestoreGainedStats(CardModel card, int addedDamage, int addedDefense)
        {
            card.BuffedDefense -= addedDefense;
            card.AddToCurrentDefenseHistory(-addedDefense, Enumerators.ReasonForValueChange.AbilityBuff);
            card.BuffedDamage -= addedDamage;
            card.AddToCurrentDamageHistory(-addedDamage, Enumerators.ReasonForValueChange.AbilityBuff);
        }

        private List<CardModel> GetAdjacentUnits () 
        {
            List<CardModel> units = BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner);
            if (units.Count > 0)
            {
                units = units.Where(x => x.Prototype.MouldId != AbilityUnitOwner.Prototype.MouldId).ToList();
            }

            return units;
        }

        private void ResetStoredStats ()
        {
            _adjacentUnits = new List<CardModel>();
            _addedDamage = 0;
            _addedDefense = 0;
        }
    }
}
