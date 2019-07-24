using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Gameplay;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DistractAndChangeStatAbilityView : AbilityViewBase<DistractAndChangeStatAbility>
    {
        private float _delayBeforeMove;
        private float _delayBeforeDestroyMoved;
        private float _delayAfterImpact;
        private float _delayBeforeDestroyImpact;

        private BattlegroundController _battlegroundController;

        public DistractAndChangeStatAbilityView(DistractAndChangeStatAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            Debug.LogWarning("On ability action called === ");
            SetDelays();

            float durationOfMoving = 0.5f;
            Vector3 offset = Vector3.zero;
            Vector3 localOffset = Vector3.zero;
            bool isRotate = false;

            soundClipTitle = string.Empty;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                //Debug.LogWarning("affect type =  " + Ability.AffectObjectType);
                //Debug.LogWarning("target pos = " + Ability.TargetPlayer.AvatarObject.transform.position);
                Vector3 targetPosition = Ability.AffectObjectType == Enumerators.AffectObjectType.Character ?
                _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.TargetUnit).Transform.position :
                Ability.TargetPlayer.AvatarObject.transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    _delayAfterImpact = effectInfo.delayAfterEffect;
                    _delayBeforeDestroyImpact = effectInfo.delayBeforeEffect;
                    soundClipTitle = effectInfo.soundName;
                    delayBeforeSound = effectInfo.delayForSound;
                    offset = effectInfo.offset;
                }

                CreateVfx(targetPosition + offset, true, _delayBeforeDestroyImpact, true);

                Transform cameraVFXObj = VfxObject.transform.Find("!! Camera shake");
                cameraVFXObj.transform.position = Vector3.zero;
                Transform cameraGroupTransform = GameClient.Get<ICameraManager>().GetGameplayCameras();
                cameraGroupTransform.SetParent
                (
                   cameraVFXObj
                );
                cameraGroupTransform.localPosition = VfxObject.transform.position * -1;
                Ability.VFXAnimationEnded += () =>
                {
                    cameraGroupTransform.SetParent(null);
                    cameraGroupTransform.position = Vector3.zero;
                };

                PlaySound(soundClipTitle, delayBeforeSound);
            }

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, _delayAfterImpact);
        }


        private void SetDelays()
        {
            _delayBeforeMove = 0;
            _delayAfterImpact = 0;
            _delayBeforeDestroyImpact = 0;
            _delayBeforeDestroyMoved = 0;
        }
    }
}
