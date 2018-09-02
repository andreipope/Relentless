using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class DealDamageToThisAndAdjacentUnitsAbility : AbilityBase
    {
        public int Value = 1;

        public DealDamageToThisAndAdjacentUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object param = null)
        {
            base.Action(param);

            int targetIndex = -1;
            for (int i = 0; i < PlayerCallerOfAbility.BoardCards.Count; i++)
            {
                if (PlayerCallerOfAbility.BoardCards[i] == AbilityUnitOwner)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    TakeDamageToUnit(PlayerCallerOfAbility.BoardCards[targetIndex - 1]);
                }

                if (targetIndex + 1 < PlayerCallerOfAbility.BoardCards.Count)
                {
                    TakeDamageToUnit(PlayerCallerOfAbility.BoardCards[targetIndex + 1]);
                }
            }

            TakeDamageToUnit(AbilityUnitOwner);
        }

        protected override void OnEndTurnEventHandler()
        {
            base.OnEndTurnEventHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.End)
                return;

            Action();
        }

        private void TakeDamageToUnit(BoardUnit unit)
        {
            BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, unit);
            CreateVfx(unit.Transform.position, true, 5f);
        }
    }
}
