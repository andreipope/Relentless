using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DestroyFrozenZombieAbility : AbilityBase
    {
        public DestroyFrozenZombieAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetUnitStatusType = ability.TargetUnitStatusType;

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/IceBoltVFX");
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Vector3 targetPosition = BattlegroundController.GetBoardUnitViewByModel(TargetUnit).Transform.position;

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = Utilites.CastVfxPosition(BattlegroundController.GetBoardUnitViewByModel(AbilityUnitOwner).Transform.position);
                targetPosition = Utilites.CastVfxPosition(targetPosition);
                VfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(ActionCompleted);
                ParticleIds.Add(ParticlesController.RegisterParticleSystem(VfxObject));
            }
        }

        private void ActionCompleted()
        {
            TargetUnit.Die();

            Vector3 targetPosition = VfxObject.transform.position;

            ClearParticles();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/IceBolt_Impact");

            VfxObject = Object.Instantiate(VfxObject);
            VfxObject.transform.position = targetPosition;
            ParticlesController.RegisterParticleSystem(VfxObject, true);
        }
    }
}
