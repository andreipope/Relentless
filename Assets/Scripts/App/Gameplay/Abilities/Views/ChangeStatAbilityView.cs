using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStatAbilityView : AbilityViewBase<ChangeStatAbility>
    {
        private BattlegroundController _battlegroundController;

        private CardModel _targetedUnit;

        public ChangeStatAbilityView(ChangeStatAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            _targetedUnit = info as CardModel;
            _targetedUnit.IsPlayable = false;
            ActionCompleted();
        }

        private void ActionCompleted()
        {
            float delayBeforeDestroy = 3f;
            float delayAfter = 0;

            string soundName = string.Empty;
            float soundDelay = 0;

            Vector3 offset;

            offset = Vector3.zero;

            Transform unitTransform = _battlegroundController.GetCardViewByModel<BoardUnitView>(_targetedUnit).Transform;

            Vector3 targetPosition = unitTransform.position;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact) && !_targetedUnit.IsDead)
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    offset = effectInfo.offset;
                    soundName = effectInfo.soundName;
                    soundDelay = effectInfo.delayForSound;
                }

                CreateVfx(targetPosition, true, delayBeforeDestroy);
                VfxObject.transform.SetParent(unitTransform, false);
                VfxObject.transform.localPosition = offset;                
                if(effectInfo != null && effectInfo.cardName == "Stapler")
                {
                    VfxObject.transform.Find(_targetedUnit.HasHeavy ? "Heavy" : "Normal").gameObject.SetActive(true);
                }
            }

            PlaySound(soundName, soundDelay);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
