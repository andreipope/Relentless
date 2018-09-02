using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
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

            BoardUnit unit = info as BoardUnit;

            int targetIndex = -1;
            for (int i = 0; i < unit.OwnerPlayer.BoardCards.Count; i++)
            {
                if (unit.OwnerPlayer.BoardCards[i] == unit)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    TakeDamageToUnit(unit.OwnerPlayer.BoardCards[targetIndex - 1]);
                }

                if (targetIndex + 1 < unit.OwnerPlayer.BoardCards.Count)
                {
                    TakeDamageToUnit(unit.OwnerPlayer.BoardCards[targetIndex + 1]);
                }
            }
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);

            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            if (info is BoardUnit)
            {
                Action(info);
            }
        }

        private void TakeDamageToUnit(BoardUnit unit)
        {
            BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, unit);
            CreateVfx(unit.Transform.position, true, 5f);
        }
    }
}
