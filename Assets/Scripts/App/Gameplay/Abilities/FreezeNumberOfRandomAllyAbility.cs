using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Helpers;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class FreezeNumberOfRandomAllyAbility : AbilityBase
    {

        public int Value { get; }

        public int Turns { get; }

        public FreezeNumberOfRandomAllyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Turns = ability.Turns;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");

            Action();
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
                switch (allies[i])
                {
                    case Player player:
                        player.Stun(Enumerators.StunType.FREEZE, Turns);
                        CreateVfx(player.AvatarObject.transform.position, true, 5f);
                        break;
                    case BoardUnit unit:
                        unit.Stun(Enumerators.StunType.FREEZE, Turns);
                        CreateVfx(unit.Transform.position, true, 5f);
                        break;
                }
            }
        }
    }
}
