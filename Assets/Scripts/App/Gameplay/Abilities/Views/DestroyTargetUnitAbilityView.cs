using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DestroyTargetUnitAbilityView : AbilityViewBase<DestroyTargetUnitAbility>
    {
        private BattlegroundController _battlegroundController;


        public DestroyTargetUnitAbilityView(DestroyTargetUnitAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            BoardUnitView targetUnitView = _battlegroundController.GetCardViewByModel<BoardUnitView>(info as CardModel);

            float delayAfter = 0;
            float delaySound = 0;
            float delayChangeState = 0;
            string soundName = string.Empty;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                float delayBeforeDestroy = 3f;

                Vector3 targetPosition = targetUnitView.Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    delayChangeState = effectInfo.delayForChangeState;
                    delayAfter = effectInfo.delayAfterEffect;
                    soundName = effectInfo.soundName;
                    delaySound = effectInfo.delayForSound;
                }

                InternalTools.DoActionDelayed(() =>
                {
                    targetUnitView.ChangeModelVisibility(false);
                }, delayChangeState);

                CreateVfx(targetPosition, true, delayBeforeDestroy, true);
            }

            PlaySound(soundName, 0);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
