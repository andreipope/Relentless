using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AdjacentUnitsGetHeavyAbility : AbilityBase
    {
        public AdjacentUnitsGetHeavyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            int targetIndex = -1;
            for (int i = 0; i < PlayerCallerOfAbility.BoardCards.Count; i++)
            {
                if (PlayerCallerOfAbility.BoardCards[i].Model == AbilityUnitOwner)
                {
                    targetIndex = i;
                    break;
                }
            }

            List<BoardObject> targets = new List<BoardObject>();

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    targets.Add(PlayerCallerOfAbility.BoardCards[targetIndex - 1].Model);
                    TakeHeavyToUnit(PlayerCallerOfAbility.BoardCards[targetIndex - 1]);
                }

                if (targetIndex + 1 < PlayerCallerOfAbility.BoardCards.Count)
                {
                    targets.Add(PlayerCallerOfAbility.BoardCards[targetIndex + 1].Model);
                    TakeHeavyToUnit(PlayerCallerOfAbility.BoardCards[targetIndex + 1]);
                }
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, targets, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }

        private static void TakeHeavyToUnit(BoardUnitView unit)
        {
            unit?.Model.SetAsHeavyUnit();
        }
    }
}
