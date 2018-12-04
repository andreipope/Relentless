using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class TakeControlEnemyUnitAbility : AbilityBase
    {
        private int Count { get; }

        public TakeControlEnemyUnitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                if (PredefinedTargets != null)
                {
                    TakeControlEnemyUnit(PredefinedTargets.Select(x => x.BoardObject as BoardUnitModel).ToList());
                }
                else
                {
                    TakeControlEnemyUnit(GetRandomEnemyUnits());
                }
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                InvokeActionTriggered();
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            TakeControlEnemyUnit(new List<BoardUnitModel>() { TargetUnit });
        }

        private List<BoardUnitModel> GetRandomEnemyUnits()
        {
            return InternalTools.GetRandomElementsFromList(GetOpponentOverlord().BoardCards, Count).Select(x => x.Model).ToList();
        }

        private void TakeControlEnemyUnit(List<BoardUnitModel> units)
        {
            foreach (BoardUnitModel unit in units)
            {
                BattlegroundController.TakeControlUnit(PlayerCallerOfAbility, unit);
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, units.Cast<BoardObject>().ToList(), AbilityData.AbilityType,
                                                     Protobuf.AffectObjectType.Types.Enum.Character);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Action();
        }
    }
}
