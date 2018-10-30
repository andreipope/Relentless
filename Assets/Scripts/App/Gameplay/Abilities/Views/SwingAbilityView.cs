using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SwingAbilityView : AbilityViewBase<SwingAbility>
    {
        private BattlegroundController _battlegroundController;

        public SwingAbilityView(SwingAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                BoardUnitModel unit = (BoardUnitModel)info;

                Vector3 targetPosition = _battlegroundController.GetBoardUnitViewByModel(unit).Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = targetPosition;
                ParticlesController.RegisterParticleSystem(VfxObject, true, 5);
            }

            float delay = 0f;

            switch (Ability.AbilityEffectType)
            {
                case Enumerators.AbilityEffectType.SWING_LIGHTNING:
                    delay = 1.5f;
                    break;
                default:
                    delay = 0;
                    break;
            }
            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delay);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, true, 5f);
        }
    }
}
