using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class DestroyUnitByTypeAbility : AbilityBase
    {
        public DestroyUnitByTypeAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetCardType = ability.TargetCardType;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
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

            BattlegroundController.DestroyBoardUnit(TargetUnit);
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }
    }
}
