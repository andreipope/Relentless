using DG.Tweening;
using Loom.ZombieBattleground.Common;
using System;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AttackOverlordAbilityView : AbilityViewBase<AttackOverlordAbility>
    {
        private BattlegroundController _battlegroundController;

        public AttackOverlordAbilityView(AttackOverlordAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                Vector3 targetPosition = Ability.AffectObjectType == Enumerators.AffectObjectType.Character ?
                _battlegroundController.GetBoardUnitViewByModel(Ability.TargetUnit).Transform.position :
                Ability.TargetPlayer.AvatarObject.transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                VfxObject = UnityEngine.Object.Instantiate(VfxObject);
                VfxObject.transform.position = Utilites.CastVfxPosition(_battlegroundController.GetBoardUnitViewByModel(Ability.AbilityUnitOwner).Transform.position);
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

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = Vector3.zero;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                foreach (Enumerators.Target target in Ability.TargetTypes)
                {
                    switch (target)
                    {
                        case Enumerators.Target.OPPONENT:
                            CreateVfx(Utilites.CastVfxPosition(Ability.GetOpponentOverlord().AvatarObject.transform.position), true, 5f, true);
                            break;
                        case Enumerators.Target.PLAYER:
                            CreateVfx(Utilites.CastVfxPosition(Ability.PlayerCallerOfAbility.AvatarObject.transform.position), true, 5f, true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(target), target, null);
                    }
                }
            }

            Ability.InvokeVFXAnimationEnded();
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
