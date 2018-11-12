using DG.Tweening;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TakeSwingToUnitsAbilityView : AbilityViewBase<TakeSwingToUnitsAbility>
    {
        private BattlegroundController _battlegroundController;

        public TakeSwingToUnitsAbilityView(TakeSwingToUnitsAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            ActionCompleted();
        }

        private void ActionCompleted()
        {
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                foreach (BoardUnitView unit in Ability.PlayerCallerOfAbility.BoardCards)
                {
                    Vector3 targetPosition = unit.Transform.position;
                    VfxObject = Object.Instantiate(VfxObject);
                    VfxObject.transform.SetParent(unit.Transform, false);
                    VfxObject.transform.localPosition = Vector3.zero;
                    ParticlesController.RegisterParticleSystem(VfxObject, true, 6f);
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
