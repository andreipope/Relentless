using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DealDamageToThisAndAdjacentUnitsAbility : AbilityBase
    {
        public DealDamageToThisAndAdjacentUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
        }

        public override void Action(object param = null)
        {
            base.Action(param);

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
                    TakeDamageToUnit(PlayerCallerOfAbility.BoardCards[targetIndex - 1].Model);
                }

                if (targetIndex + 1 < PlayerCallerOfAbility.BoardCards.Count)
                {
                    targets.Add(PlayerCallerOfAbility.BoardCards[targetIndex + 1].Model);
                    TakeDamageToUnit(PlayerCallerOfAbility.BoardCards[targetIndex + 1].Model);
                }
            }

            targets.Add(AbilityUnitOwner);

            TakeDamageToUnit(AbilityUnitOwner);

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, targets, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.END)
                return;

            Action();
        }

        private void TakeDamageToUnit(BoardUnitModel unit)
        {
            BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, unit);
            CreateVfx(BattlegroundController.GetBoardUnitViewByModel(unit).Transform.position, true, 5f);
        }
    }
}
