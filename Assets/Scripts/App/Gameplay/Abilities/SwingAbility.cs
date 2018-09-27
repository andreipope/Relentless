using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SwingAbility : AbilityBase
    {
        public int Value { get; }

        public SwingAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PushVFX");
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            BoardUnitView unit = info as BoardUnitView;

            int targetIndex = -1;
            for (int i = 0; i < unit.Model.OwnerPlayer.BoardCards.Count; i++)
            {
                if (unit.Model.OwnerPlayer.BoardCards[i] == unit)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    TakeDamageToUnit(unit.Model.OwnerPlayer.BoardCards[targetIndex - 1]);
                }

                if (targetIndex + 1 < unit.Model.OwnerPlayer.BoardCards.Count)
                {
                    TakeDamageToUnit(unit.Model.OwnerPlayer.BoardCards[targetIndex + 1]);
                }
            }
        }

        protected override void UnitAttackedHandler(object info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);

            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            if (info is BoardUnitView)
            {
                Action(info);
            }
        }

        private void TakeDamageToUnit(BoardUnitView unit)
        {
            BattleController.AttackUnitByAbility(AbilityUnitViewOwner, AbilityData, unit.Model);
            CreateVfx(unit.Transform.position, true, 5f);
        }
    }
}
