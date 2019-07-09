using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SummonAbilityView : AbilityViewBase<SummonsAbility>
    {
        private BattlegroundController _battlegroundController;
        private ICameraManager _cameraManager;

        public SummonAbilityView(SummonsAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
            _cameraManager = GameClient.Get<ICameraManager>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            Debug.LogWarning("On ability action called = " + Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact));
            float delayBeforeDestroy = 3f;
            float delayAfter = 0;
            string soundName = string.Empty;
            float delaySound = 0;
            Vector3 offset = Vector3.zero;

            BoardUnitView unitView = _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.CardModel);

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                unitView.GameObject.SetActive(false);

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);
                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    soundName = effectInfo.soundName;
                    delaySound = effectInfo.delayForSound;
                    offset = effectInfo.offset;
                }


                Vector3 targetPosition = unitView.Transform.position + offset;
                CreateVfx(targetPosition, true, delayBeforeDestroy, true);
                PlaySound(soundName, delaySound);

                // camera shake
                Transform cameraVFXObj = VfxObject.transform.Find("!!Null Camera shake");
                Transform cameraGroupTransform = _cameraManager.GetGameplayCameras();
                cameraGroupTransform.SetParent(cameraVFXObj);
            }

            InternalTools.DoActionDelayed(() =>
            {
                unitView.GameObject.SetActive(true);
                unitView.battleframeAnimator.Play(0, -1, 1);

                Transform cameraGroupTransform = _cameraManager.GetGameplayCameras();
                cameraGroupTransform.SetParent(null);
                cameraGroupTransform.position = Vector3.zero;

                Ability.InvokeVFXAnimationEnded();

            }, delayAfter);
        }
    }
}
