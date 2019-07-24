using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class FillBoardByUnitsAbilityView : AbilityViewBase<FillBoardByUnitsAbility>
    {
        private BattlegroundController _battlegroundController;

        public FillBoardByUnitsAbilityView(FillBoardByUnitsAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            float delayAfter = 0;
            float delaySound = 0;
            string soundName = string.Empty;
            Vector3 offset = Vector3.zero;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                float delayBeforeDestroy = 3f;

                Vector3 targetPosition = Vector3.zero;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    delayAfter = effectInfo.delayAfterEffect;
                    soundName = effectInfo.soundName;
                    delaySound = effectInfo.delayForSound;
                    offset = effectInfo.offset;
                }

                targetPosition = targetPosition + offset;
                targetPosition *= Ability.PlayerCallerOfAbility.IsLocalPlayer ? 1 : -1;

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
