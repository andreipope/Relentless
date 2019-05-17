using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DamageTargetAdjustmentsAbilityView : AbilityViewBase<DamageTargetAdjustmentsAbility>
    {
        private BattlegroundController _battlegroundController;

        private ParticlesController _particlesController;

        private List<CardModel> _targetUnits;

        private string _cardName;

        public DamageTargetAdjustmentsAbilityView(DamageTargetAdjustmentsAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
            _particlesController = GameplayManager.GetController<ParticlesController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            _targetUnits = new List<CardModel>();
            if(info != null)
            {
                _targetUnits = (List<CardModel>)info;
            }

            ulong id;

            CardModel unit = null;

            bool isLastUnit = false;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                for (int i = 0; i < _targetUnits.Count; i++)
                {
                    unit = _targetUnits[i];

                    Vector3 targetPosition = Ability.CardKind == Enumerators.CardKind.CREATURE ?
                    _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position :
                    Ability.SelectedPlayer.Transform.position;

                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                    VfxObject = Object.Instantiate(VfxObject);
                    VfxObject.transform.position = _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position;
                    id = ParticlesController.RegisterParticleSystem(VfxObject);
                    ParticleIds.Add(id);

                    isLastUnit = i == _targetUnits.Count - 1;
                    if(isLastUnit)
                    VfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(() => ActionCompleted());
                }
            }
            else
            {
                ActionCompleted();
                for (int i = 0; i < _targetUnits.Count; i++)
                {
                    unit = _targetUnits[i];
                    isLastUnit = i == _targetUnits.Count - 1;
                }
            }
        }

        private void ActionCompleted()
        {
            _cardName = "";
            float delayAfter = 0;
            float delayBeforeDestroy = 3f;
            string soundName = string.Empty;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = null;

                for (int i = 0; i < _targetUnits.Count; i++)
                {
                    if (_battlegroundController.GetCardViewByModel<BoardUnitView>(_targetUnits[i]).GameObject != null)
                    {
                        effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                        if (effectInfo != null)
                        {
                            _cardName = effectInfo.cardName;
                            delayAfter = effectInfo.delayAfterEffect;
                            delayBeforeDestroy = effectInfo.delayBeforeEffect;
                            soundName = effectInfo.soundName;
                        }

                        Vector3 targetPosition = _battlegroundController.GetCardViewByModel<BoardUnitView>(_targetUnits[i]).Transform.position;

                        CreateVfx(targetPosition, true, delayBeforeDestroy);
                    }
                }
            }
            PlaySound(soundName, 0);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
