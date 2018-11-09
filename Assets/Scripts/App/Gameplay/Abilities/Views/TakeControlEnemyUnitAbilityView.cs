using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TakeControlEnemyUnitAbilityView : AbilityViewBase<TakeControlEnemyUnitAbility>
    {
        private BattlegroundController _battlegroundController;

        public TakeControlEnemyUnitAbilityView(TakeControlEnemyUnitAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Enumerators.VisualEffectType effectType = Enumerators.VisualEffectType.Impact;

                switch (Ability.TargetUnit.InitialUnitType)
                {
                    case Enumerators.CardType.FERAL:
                        effectType = Enumerators.VisualEffectType.Impact_Feral;
                        break;
                    case Enumerators.CardType.HEAVY:
                        effectType = Enumerators.VisualEffectType.Impact_Heavy;
                        break;
                    default:
                        effectType = Enumerators.VisualEffectType.Impact;
                        break;
                }

                if (!Ability.AbilityData.HasVisualEffectType(effectType))
                {
                    effectType = Enumerators.VisualEffectType.Impact;
                }

                Vector3 targetPosition = _battlegroundController.GetBoardUnitViewByModel(Ability.TargetUnit).Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(effectType).Path);
                CreateVfx(targetPosition, true);
            }

            Ability.InvokeVFXAnimationEnded();
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
