using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class FreezeUnitsAbility : AbilityBase
    {
        public int Value;

        public FreezeUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
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

            Player opponent = PlayerCallerOfAbility.Equals(GameplayManager.CurrentPlayer)?GameplayManager.OpponentPlayer:GameplayManager.CurrentPlayer;

            foreach (BoardUnit unit in opponent.BoardCards)
            {
                unit.Stun(Enumerators.StunType.Freeze, Value);
                CreateVfx(unit.Transform.position, true, 5f);
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
