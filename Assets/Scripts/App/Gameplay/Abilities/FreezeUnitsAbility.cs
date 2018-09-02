using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class FreezeUnitsAbility : AbilityBase
    {
        public int Value { get; }

        public FreezeUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
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

            Player opponent = PlayerCallerOfAbility.Equals(GameplayManager.CurrentPlayer)?GameplayManager.OpponentPlayer:GameplayManager.CurrentPlayer;

            foreach (BoardUnit unit in opponent.BoardCards)
            {
                unit.Stun(Enumerators.StunType.FREEZE, Value);
                CreateVfx(unit.Transform.position, true, 5f);
            }
        }
    }
}
