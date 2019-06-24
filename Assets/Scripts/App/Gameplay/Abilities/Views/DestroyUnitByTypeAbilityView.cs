using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DestroyUnitByTypeAbilityView : AbilityViewBase<DestroyUnitByTypeAbility>
    {
        private BattlegroundController _battlegroundController;

        private GameObject _camerasObject;

        private GameObject _backgroundCameraObject;

        public DestroyUnitByTypeAbilityView(DestroyUnitByTypeAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();

            _backgroundCameraObject = GameObject.Find("GamePlayCameras");
            _camerasObject = GameObject.Find("Cameras");
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

            float delayAfter = 0f;
            float beforeDelay = 3f;
            float delayBeforeUnitActivate = 0f;

            soundClipTitle = string.Empty;
            delayBeforeSound = 0;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.TargetUnit).Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    beforeDelay = effectInfo.delayBeforeEffect;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeUnitActivate = effectInfo.delayForChangeState;
                    soundClipTitle = effectInfo.soundName;
                    delayBeforeSound = effectInfo.delayForSound;
                }

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = targetPosition + new Vector3(0.35f, 0f, 0);
                ParticlesController.RegisterParticleSystem(VfxObject, true, beforeDelay);

                InternalTools.DoActionDelayed(() =>
                {
                    Vector3 strength = Vector3.right * 0.25f;
                    if (_camerasObject != null)
                    {
                        _camerasObject.transform.DOShakePosition(.1f, strength, 10, 0, false, false);
                    }
                    if (_backgroundCameraObject != null)
                    {
                        _backgroundCameraObject.transform.DOShakePosition(.4f, strength, 10, 0, false, false);
                    }
                }, delayBeforeUnitActivate);
            }

            PlaySound(soundClipTitle, delayBeforeSound);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, true, 5f);
        }
    }
}
