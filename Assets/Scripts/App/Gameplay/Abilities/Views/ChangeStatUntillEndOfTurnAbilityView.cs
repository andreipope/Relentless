using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStatUntillEndOfTurnAbilityView : AbilityViewBase<ChangeStatUntillEndOfTurnAbility>
    {
        private const float DELAY_AFTER_IMPACT_FRESH_MEAT = 1f;

        private const float DELAY_DESTROY_IMPACT_FRESH_MEAT = 5f;

        private float _delayAfterImpact;

        private float _delayDestroyImpact;

        private BattlegroundController _battlegroundController;

        public ChangeStatUntillEndOfTurnAbilityView(ChangeStatUntillEndOfTurnAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            SetDelays();

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                Vector3 targetPosition = _battlegroundController.GetBoardUnitViewByModel(Ability.TargetUnit).Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = _battlegroundController.GetBoardUnitViewByModel(Ability.AbilityUnitOwner).Transform.position;
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
                Vector3 targetPosition = Ability.PlayerCallerOfAbility.AvatarObject.transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);
                CreateVfx(targetPosition, true, _delayDestroyImpact, true);


                if(Ability.AbilityEffectType == Enumerators.AbilityEffectType.CHANGE_STAT_FRESH_MEAT && !Ability.PlayerCallerOfAbility.IsLocalPlayer)
                {
                    VfxObject.transform.eulerAngles = new Vector3(180, VfxObject.transform.eulerAngles.y, VfxObject.transform.eulerAngles.z);
                }
            }

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, _delayAfterImpact);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }

        private void SetDelays()
        {
            _delayAfterImpact = 0;
            _delayDestroyImpact = 3f;

            switch (Ability.AbilityEffectType)
            {
                case Enumerators.AbilityEffectType.CHANGE_STAT_FRESH_MEAT:
                    _delayAfterImpact = DELAY_AFTER_IMPACT_FRESH_MEAT;
                    _delayDestroyImpact = DELAY_DESTROY_IMPACT_FRESH_MEAT;
                    break;
                default:
                    break;
            }
        }
    }
}
