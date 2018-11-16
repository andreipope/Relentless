using DG.Tweening;
using Loom.ZombieBattleground.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DamageTargetAdjustmentsAbilityView : AbilityViewBase<DamageTargetAdjustmentsAbility>
    {
        private BattlegroundController _battlegroundController;

        private ParticlesController _particlesController;

        private List<BoardUnitView> _targetUnits;

        public DamageTargetAdjustmentsAbilityView(DamageTargetAdjustmentsAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
            _particlesController = GameplayManager.GetController<ParticlesController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            _targetUnits = new List<BoardUnitView>();
            if(info != null)
            {
                _targetUnits = (List<BoardUnitView>)info;
            }

            ulong id;

            BoardUnitView unit = null;

            bool isLastUnit = false;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                for (int i = 0; i < _targetUnits.Count; i++)
                {
                    unit = _targetUnits[i];

                    Vector3 targetPosition = Ability.CardKind == Enumerators.CardKind.CREATURE ?
                    _battlegroundController.GetBoardUnitViewByModel(Ability.AbilityUnitOwner).Transform.position :
                    Ability.SelectedPlayer.Transform.position;

                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                    VfxObject = Object.Instantiate(VfxObject);
                    VfxObject.transform.position = _battlegroundController.GetBoardUnitViewByModel(Ability.AbilityUnitOwner).Transform.position;
                    id = ParticlesController.RegisterParticleSystem(VfxObject);
                    ParticleIds.Add(id);

                    isLastUnit = i == _targetUnits.Count - 1;

                    VfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(() => ActionCompleted(unit.Transform.position, isLastUnit, id));

                }
            }
            else
            {
                for (int i = 0; i < _targetUnits.Count; i++)
                {
                    unit = _targetUnits[i];
                    isLastUnit = i == _targetUnits.Count - 1;
                    ActionCompleted(unit.Transform.position, isLastUnit);
                }
            }
        }

        private void ActionCompleted(Vector3 position, bool isLastUnit, ulong id = ulong.MaxValue)
        {
            if (id != ulong.MaxValue)
            {
                _particlesController.DestroyParticle(id);
            }

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                CreateVfx(targetPosition, true, 5f);
            }

            if (isLastUnit)
            {
                Ability.InvokeVFXAnimationEnded();
            }
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
