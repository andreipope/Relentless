using DG.Tweening;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DestroyFrozenZombieAbilityView : AbilityViewBase<DestroyFrozenZombieAbility>
    {
        private BattlegroundController _battlegroundController;

        private Vector3 _targetPosition;

        public DestroyFrozenZombieAbilityView(DestroyFrozenZombieAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                _targetPosition = _battlegroundController.GetBoardUnitViewByModel(Ability.TargetUnit).Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/IceBoltVFX");//(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = _battlegroundController.GetBoardUnitViewByModel(Ability.AbilityUnitOwner).Transform.position;
                VfxObject.transform.DOMove(_targetPosition, 0.5f).OnComplete(ActionCompleted);
                ParticleIds.Add(ParticlesController.RegisterParticleSystem(VfxObject));
            }
            else
            {
                ActionCompleted();
            }
        }

        private void ActionCompleted()
        {
            ClearParticles();

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);
                _targetPosition.y += 0.34f;
                VfxObject = Object.Instantiate(VfxObject, _battlegroundController.GetBoardUnitViewByModel(Ability.TargetUnit).Transform, false);
                VfxObject.transform.position = _targetPosition;
            }

            Ability.InvokeVFXAnimationEnded();
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
