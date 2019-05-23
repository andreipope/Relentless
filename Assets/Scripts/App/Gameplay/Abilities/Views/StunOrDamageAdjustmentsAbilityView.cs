using DG.Tweening;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class StunOrDamageAdjustmentsAbilityView : AbilityViewBase<StunOrDamageAdjustmentsAbility>
    {
        private BattlegroundController _battlegroundController;

        public StunOrDamageAdjustmentsAbilityView(StunOrDamageAdjustmentsAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                Vector3 targetPosition = _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.TargetUnit).Transform.position;
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = Utilites.CastVfxPosition(_battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position);
                targetPosition = Utilites.CastVfxPosition(targetPosition);
                VfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(ActionCompleted);
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
                Vector3 targetPosition = _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.TargetUnit).Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = targetPosition;
                ParticlesController.RegisterParticleSystem(VfxObject, true, 5);
            }

            Ability.InvokeVFXAnimationEnded();
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
