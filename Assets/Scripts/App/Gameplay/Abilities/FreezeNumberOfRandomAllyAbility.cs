using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Helpers;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class FreezeNumberOfRandomAllyAbility : AbilityBase
    {
        public int Value;

        public int Turns = 1;

        public FreezeNumberOfRandomAllyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Turns = ability.Turns;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.Entry)

                return;

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");

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

            allies.AddRange(PlayerCallerOfAbility.BoardCards);
            allies.Remove(AbilityUnitOwner);
            allies.Add(PlayerCallerOfAbility);

            allies = InternalTools.GetRandomElementsFromList(allies, Value);

            for (int i = 0; i < allies.Count; i++)
            {
                if (allies[i] is Player)
                {
                    (allies[i] as Player).Stun(Enumerators.StunType.Freeze, Turns);
                    CreateVfx((allies[i] as Player).AvatarObject.transform.position, true, 5f);
                }
                else if (allies[i] is BoardUnit)
                {
                    (allies[i] as BoardUnit).Stun(Enumerators.StunType.Freeze, Turns);
                    CreateVfx((allies[i] as BoardUnit).Transform.position, true, 5f);
                }
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }
    }
}
