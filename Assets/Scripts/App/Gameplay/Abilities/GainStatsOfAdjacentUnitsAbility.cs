using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class GainStatsOfAdjacentUnitsAbility : AbilityBase
    {
        private int _addedDamage,
                    _addedDefense;

        public GainStatsOfAdjacentUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        protected override void ChangeAuraStatusAction(bool status)
        {
            base.ChangeAuraStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            if (status)
            {
                GainStats(AbilityUnitOwner, BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner));
            }
            else
            {
                RestoreGainedStats(AbilityUnitOwner);
            }
        }

        protected override void BoardChangedHandler(int count)
        {
            base.BoardChangedHandler(count);

            if(AbilityUnitOwner.IsUnitActive && !AbilityUnitOwner.IsDead)
            {
                RestoreGainedStats(AbilityUnitOwner);
                GainStats(AbilityUnitOwner, BattlegroundController.GetAdjacentUnitsToUnit(AbilityUnitOwner));
            }
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
            boardUnit.CurrentDefense += _addedDefense;
            boardUnit.BuffedDamage += _addedDamage;
            boardUnit.CurrentDamage += _addedDamage;
        }

        private void RestoreGainedStats(CardModel boardUnit)
        {
            boardUnit.BuffedDefense -= _addedDefense;
            boardUnit.CurrentDefense -= _addedDefense;
            boardUnit.BuffedDamage -= _addedDamage;
            boardUnit.CurrentDamage -= _addedDamage;

            _addedDefense = 0;
            _addedDamage = 0;
        }
    }
}
