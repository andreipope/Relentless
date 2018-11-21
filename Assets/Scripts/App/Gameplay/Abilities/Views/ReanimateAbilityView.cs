using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReanimateAbilityView : AbilityViewBase<ReanimateAbility>
    {
        private BattlegroundController _battlegroundController;

        private IUIManager _uiManager;

        public ReanimateAbilityView(ReanimateAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            float delayAfter = 0f;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                float beforeDelay = 3f;
                float delayBeforeUnitActivate = 0f;

                BoardUnitView unit = info as BoardUnitView;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);
                VfxObject = Object.Instantiate(VfxObject, unit.Transform, false);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    beforeDelay = effectInfo.delayBeforeEffect;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeUnitActivate = effectInfo.delayForChangeState;
                }

                VfxObject.transform.localPosition = Vector3.zero;
                ParticlesController.RegisterParticleSystem(VfxObject, true, beforeDelay);
                VfxObject.SetActive(false);
                unit.ChangeModelVisibility(false);

                InternalTools.DoActionDelayed(() =>
                {
                    VfxObject.SetActive(true);
                }, delayAfter);

                InternalTools.DoActionDelayed(() =>
                {
                    unit.ChangeModelVisibility(true);
                }, delayBeforeUnitActivate);
            }

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
