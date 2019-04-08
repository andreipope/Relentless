using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DamageTargetAbilityView : AbilityViewBase<DamageTargetAbility>
    {
        private float _delayBeforeMove;
        private float _delayBeforeDestroyMoved;
        private float _delayAfterImpact;
        private float _delayBeforeDestroyImpact;

        private BattlegroundController _battlegroundController;

        public DamageTargetAbilityView(DamageTargetAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            SetDelays();

            float durationOfMoving = 0.5f;
            Vector3 offset = Vector3.zero;
            Vector3 localOffset = Vector3.zero;
            bool isRotate = false;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                Vector3 targetPosition = Ability.AffectObjectType == Enumerators.AffectObjectType.Character ?
                _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(Ability.TargetUnit).Transform.position :
                Ability.TargetPlayer.AvatarObject.transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    durationOfMoving = effectInfo.delayAfterEffect;
                    _delayBeforeDestroyMoved = effectInfo.delayBeforeEffect;
                    _delayBeforeMove = effectInfo.delayForChangeState;
                    delayBeforeSound = effectInfo.delayForSound;
                    offset = effectInfo.offset;
                    localOffset = effectInfo.localOffset;
                    soundClipTitle = effectInfo.soundName;
                    delayBeforeSound = effectInfo.delayForSound;
                    isRotate = effectInfo.isRotate;
                }

                if (isRotate)
                {
                    SetRotation(targetPosition);
                }

                targetPosition += VfxObject.transform.up * localOffset.y;
                targetPosition += VfxObject.transform.right * localOffset.x;
                targetPosition += VfxObject.transform.forward * localOffset.z;

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position;
                InternalTools.DoActionDelayed(() =>
                {
                    VfxObject.transform.DOMove(targetPosition, durationOfMoving).OnComplete(ActionCompleted);
                    ParticleIds.Add(ParticlesController.RegisterParticleSystem(VfxObject));
                }, _delayBeforeMove);

                PlaySound(soundClipTitle, 0);
            }
            else
            {
                ActionCompleted();
            }
        }

        private void ActionCompleted()
        {
            InternalTools.DoActionDelayed(ClearParticles, _delayBeforeDestroyMoved);

            soundClipTitle = string.Empty;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = Ability.AffectObjectType == Enumerators.AffectObjectType.Character ?
                _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(Ability.TargetUnit).Transform.position :
                Ability.TargetPlayer.AvatarObject.transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    _delayAfterImpact = effectInfo.delayAfterEffect;
                    _delayBeforeDestroyImpact = effectInfo.delayBeforeEffect;
                    soundClipTitle = effectInfo.soundName;
                    delayBeforeSound = effectInfo.delayForSound;
                }

                CreateVfx(targetPosition, true, _delayBeforeDestroyImpact, true);

                PlaySound(soundClipTitle, delayBeforeSound);
            }

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, _delayAfterImpact);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }

        private void SetDelays()
        {
            _delayBeforeMove = 0;
            _delayAfterImpact = 0;
            _delayBeforeDestroyImpact = 0;
            _delayBeforeDestroyMoved = 0;
        }

        private float AngleBetweenVector3(Vector3 from, Vector3 target)
        {
            Vector3 diference = target - from;
            float sign = (target.x < from.x) ? 1.0f : -1.0f;
            return Vector3.Angle(Vector3.up, diference) * sign;
        }

        private void SetRotation(Vector3 targetPosition)
        {
            float angle = AngleBetweenVector3(_battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position, targetPosition);
            VfxObject.transform.eulerAngles = new Vector3(VfxObject.transform.eulerAngles.x, VfxObject.transform.eulerAngles.y, angle);
        }
    }
}
