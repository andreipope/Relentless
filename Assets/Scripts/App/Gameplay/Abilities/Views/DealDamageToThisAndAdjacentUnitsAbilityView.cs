using DG.Tweening;
using Loom.ZombieBattleground.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DealDamageToThisAndAdjacentUnitsAbilityView : AbilityViewBase<DealDamageToThisAndAdjacentUnitsAbility>
    {
        private BattlegroundController _battlegroundController;

        private List<BoardUnitModel> _units;

        public DealDamageToThisAndAdjacentUnitsAbilityView(DealDamageToThisAndAdjacentUnitsAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
            _units = new List<BoardUnitModel>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            if(info != null)
            {
                _units = (List<BoardUnitModel>)info;
            }
            ActionCompleted();        
        }

        private void ActionCompleted()
        {
            ClearParticles();
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = Vector3.zero;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);
                foreach (var unit in _units)
                {
                    targetPosition = Utilites.CastVfxPosition(_battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit).Transform.position);
                    CreateVfx(targetPosition, true, 5f, true);
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
