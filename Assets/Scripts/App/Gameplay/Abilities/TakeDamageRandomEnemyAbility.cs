using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Helpers;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class TakeDamageRandomEnemyAbility : AbilityBase
    {
        public int Value;

        public TakeDamageRandomEnemyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.Entry)
                return;

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");

            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<object> allies = new List<object>();

            allies.AddRange(GetOpponentOverlord().BoardCards);
            allies.Add(GetOpponentOverlord());

            allies = InternalTools.GetRandomElementsFromList(allies, 1);

            for (int i = 0; i < allies.Count; i++)
            {
                if (allies[i] is Player)
                {
                    BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, allies[i] as Player);
                    CreateVfx((allies[i] as Player).AvatarObject.transform.position, true, 5f, true);
                }
                else if (allies[i] is BoardUnit)
                {
                    BattleController.AttackUnitByAbility(GetCaller(), AbilityData, allies[i] as BoardUnit);
                    CreateVfx((allies[i] as BoardUnit).Transform.position, true, 5f);
                }
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }
    }
}
