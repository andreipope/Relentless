using DG.Tweening;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class FreezeUnitsAbilityView : AbilityViewBase<FreezeUnitsAbility>
    {
        private BattlegroundController _battlegroundController;

        private Player _opponent;

        public FreezeUnitsAbilityView(FreezeUnitsAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            Debug.LogError(2222);
            if(info != null)
            {
                _opponent = info as Player;
            }
            ActionCompleted();
        }

        private void ActionCompleted()
        {
            ClearParticles();
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                Vector3 position = Vector3.zero;
                foreach (BoardUnitView unit in _opponent.BoardCards)
                {
                    position = Utilites.CastVfxPosition(unit.Transform.position);
                    CreateVfx(position, true, 5f, true);
                }
            }

            Ability.InvokeVFXAnimationEnded();
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, true, 5f);
        }
    }
}
