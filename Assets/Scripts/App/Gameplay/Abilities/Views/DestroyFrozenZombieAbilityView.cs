using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
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
                _targetPosition = _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.TargetUnit).Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position;
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

            float delayBeforeDestroy = 5f;
            float delayAfter = 0;
            Vector3 offset = Vector3.zero;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    offset = effectInfo.offset;
                    soundClipTitle = effectInfo.soundName;
                    delayBeforeSound = effectInfo.delayForSound;
                }

                _targetPosition += offset;
                VfxObject = Object.Instantiate(VfxObject, _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.TargetUnit).Transform, false);
                ParticlesController.RegisterParticleSystem(VfxObject, true, delayBeforeDestroy);
                VfxObject.transform.position = _targetPosition;
            }

            PlaySound(soundClipTitle, delayBeforeSound);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
