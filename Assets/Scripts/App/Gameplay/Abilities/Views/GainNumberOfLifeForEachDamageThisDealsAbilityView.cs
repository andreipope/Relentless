using DG.Tweening;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GainNumberOfLifeForEachDamageThisDealsAbilityView : AbilityViewBase<GainNumberOfLifeForEachDamageThisDealsAbility>
    {
        private BattlegroundController _battlegroundController;

        public GainNumberOfLifeForEachDamageThisDealsAbilityView(GainNumberOfLifeForEachDamageThisDealsAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            ActionCompleted();
        }

        private void ActionCompleted()
        {
            ClearParticles();

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = Utilites.CastVfxPosition(_battlegroundController.GetBoardUnitViewByModel(Ability.AbilityUnitOwner).Transform.position);

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);
                CreateVfx(targetPosition, true, 5f);
            }

            Ability.InvokeVFXAnimationEnded();
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
