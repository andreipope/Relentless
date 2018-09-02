using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class TakeDamageAtEndOfTurnToThis : AbilityBase
    {
        public int Value;

        public TakeDamageAtEndOfTurnToThis(Enumerators.CardKind cardKind, AbilityData ability)
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

        public override void Action(object info = null)
        {
            base.Action(info);

            BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, AbilityUnitOwner);
            CreateVfx(AbilityUnitOwner.Transform.position, true, 5f);
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void OnEndTurnEventHandler()
        {
            base.OnEndTurnEventHandler();

            if (!GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))

                return;

            if (AbilityCallType != Enumerators.AbilityCallType.End)

                return;

            Action();
        }
    }
}
