using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class FreezeUnitsAbility : AbilityBase
    {
        public int value;

        public FreezeUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.ENTRY)

                return;

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");

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

            Player opponent = playerCallerOfAbility.Equals(_gameplayManager.CurrentPlayer)?_gameplayManager.OpponentPlayer:_gameplayManager.CurrentPlayer;

            foreach (BoardUnit unit in opponent.BoardCards)
            {
                unit.Stun(Enumerators.StunType.FREEZE, value);
                CreateVFX(unit.transform.position, true, 5f);
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
