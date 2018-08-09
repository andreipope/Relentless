// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class SwingAbility : AbilityBase
    {
        public int value = 0;

        public SwingAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PushVFX");
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }


        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);

            if (abilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            if (info is BoardUnit)
                Action(info);
        }
    
        public override void Action(object info = null)
        {
            base.Action(info);

            var unit = info as BoardUnit;

            int targetIndex = -1;
            for (int i = 0; i < unit.ownerPlayer.BoardCards.Count; i++)
            {
                if (unit.ownerPlayer.BoardCards[i] == unit)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                    TakeDamageToUnit(unit.ownerPlayer.BoardCards[targetIndex - 1]);
                if (targetIndex + 1 < unit.ownerPlayer.BoardCards.Count)
                    TakeDamageToUnit(unit.ownerPlayer.BoardCards[targetIndex + 1]);
            }
        }

        private void TakeDamageToUnit(BoardUnit unit)
        {
            _battleController.AttackUnitByAbility(abilityUnitOwner, abilityData, unit);
            CreateVFX(unit.transform.position, true, 5f);
        }
    }
}
