using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    internal class DevourAbility : CardAbility
    {
        private List<BoardUnitModel> _units;

        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            _units = Targets.Select(target => target as BoardUnitModel).ToList();

            _units.Remove(UnitModelOwner);

            foreach (BoardUnitModel unit in _units)
            {
                DevourTargetZombie(unit);
            }

            VFXAnimationEndedHandler();
        }

        private void DevourTargetZombie(BoardUnitModel unit)
        {
            if (unit == UnitModelOwner)
                return;

            int defense = unit.Card.Prototype.Defense;
            int damage = unit.Card.Prototype.Damage;

            UnitModelOwner.BuffedDefense += defense;
            UnitModelOwner.CurrentDefense += defense;

            UnitModelOwner.BuffedDamage += damage;
            UnitModelOwner.CurrentDamage += damage;

            RanksController.AddUnitForIgnoreRankBuff(unit);

            unit.IsReanimated = true;
            BoardUnitView view = BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit);
            view.StopSleepingParticles();

            unit.RemoveGameMechanicDescriptionFromUnit(Common.Enumerators.GameMechanicDescription.Reanimate);
        }

        private void VFXAnimationEndedHandler()
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();


            foreach (BoardUnitModel unit in _units)
            {
                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Common.Enumerators.ActionEffectType.Devour,
                    Target = unit,
                });

                BattlegroundController.DestroyBoardUnit(unit, false, true, false);
            }

            BoardController.UpdateCurrentBoardOfPlayer(PlayerOwner, null);

            PostGameActionReport(Common.Enumerators.ActionType.CardAffectingMultipleCards, TargetEffects);
        }
    }
}
