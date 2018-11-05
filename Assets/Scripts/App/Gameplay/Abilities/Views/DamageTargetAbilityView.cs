using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DamageTargetAbilityView : AbilityViewBase<DamageTargetAbility>
    {
        private BattlegroundController _battlegroundController;

        private Vector3 _targetPosition;

        public DamageTargetAbilityView(DamageTargetAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                _targetPosition = Ability.AffectObjectType == Enumerators.AffectObjectType.Character ?
                _battlegroundController.GetBoardUnitViewByModel(Ability.TargetUnit).Transform.position :
                Ability.TargetPlayer.AvatarObject.transform.position;

                float duration = 3f;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                VfxObject = Object.Instantiate(VfxObject);
                if (Ability.AbilityEffectType == Enumerators.AbilityEffectType.TARGET_ROCK)
                {
                    SetRotation();
                    VfxObject.transform.position = _battlegroundController.GetBoardUnitViewByModel(Ability.AbilityUnitOwner).Transform.position - VfxObject.transform.up;
                    _targetPosition -= VfxObject.transform.up * 2.5f;
                    VfxObject.transform.DOMove(_targetPosition, 2.9f).OnComplete(ActionCompleted);
                    duration = 6f;
                }
                else
                {
                    VfxObject.transform.position = Utilites.CastVfxPosition(_battlegroundController.GetBoardUnitViewByModel(Ability.AbilityUnitOwner).Transform.position);
                    _targetPosition = Utilites.CastVfxPosition(_targetPosition);
                    VfxObject.transform.DOMove(_targetPosition, 0.5f).OnComplete(ActionCompleted);
                    
                }
                ParticleIds.Add(ParticlesController.RegisterParticleSystem(VfxObject, true, duration));
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
                _targetPosition = VfxObject.transform.position;
                
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = _targetPosition;

                ParticlesController.RegisterParticleSystem(VfxObject, true);
            }

            Ability.InvokeVFXAnimationEnded();
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, true, 5f);
        }

        private float AngleBetweenVector3(Vector3 from, Vector3 target)
        {
            Vector3 diference = target - from;
            float sign = (target.x < from.x) ? 1.0f : -1.0f;
            return Vector3.Angle(Vector3.up, diference) * sign;
        }

        private void SetRotation()
        {
            float angle = AngleBetweenVector3(_battlegroundController.GetBoardUnitViewByModel(Ability.AbilityUnitOwner).Transform.position, _targetPosition);
            VfxObject.transform.eulerAngles = new Vector3(VfxObject.transform.eulerAngles.x, VfxObject.transform.eulerAngles.y, angle);
        }
    }
}
