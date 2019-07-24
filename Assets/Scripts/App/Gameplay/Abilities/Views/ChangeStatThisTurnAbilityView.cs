using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStatThisTurnAbilityView : AbilityViewBase<ChangeStatThisTurnAbility>
    {
        private BattlegroundController _battlegroundController;
        public ChangeStatThisTurnAbilityView(ChangeStatThisTurnAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            float delayBeforeDestroy = 3f;
            float delayAfter = 0;
            string soundName = string.Empty;
            float delaySound = 0;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);
                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    soundName = effectInfo.soundName;
                    delaySound = effectInfo.delayForSound;
                }

                Vector3 targetPosition = VfxObject.transform.position;
                CreateVfx(targetPosition, true, delayBeforeDestroy, true);
                VfxObject.transform.eulerAngles = Ability.PlayerCallerOfAbility.IsLocalPlayer ? Vector3.zero : new Vector3(140, 0, 0);
                PlaySound(soundName, delaySound);
            }

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }
    }
}
