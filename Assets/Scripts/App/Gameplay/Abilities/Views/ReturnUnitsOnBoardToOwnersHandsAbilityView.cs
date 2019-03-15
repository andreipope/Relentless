using DG.Tweening;
using Loom.ZombieBattleground.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReturnUnitsOnBoardToOwnersHandsAbilityView : AbilityViewBase<ReturnUnitsOnBoardToOwnersHandsAbility>
    {
        private BattlegroundController _battlegroundController;

        private List<BoardUnitModel> _units;

        public ReturnUnitsOnBoardToOwnersHandsAbilityView(ReturnUnitsOnBoardToOwnersHandsAbility ability) : base(ability)
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

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                Vector3 targetPosition = Ability.AffectObjectType == Enumerators.AffectObjectType.Character ?
                _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(Ability.TargetUnit).Transform.position :
                Ability.TargetPlayer.AvatarObject.transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = Utilites.CastVfxPosition(_battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position);
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
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);
                foreach (var unit in _units)
                {
                    CreateVfx(_battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit).Transform.position, true, 3f, true);
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
