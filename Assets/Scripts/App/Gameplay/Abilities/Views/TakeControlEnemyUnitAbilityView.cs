using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
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
            List<BoardUnitModel> units = (List<BoardUnitModel>)info;

            float delayBeforeDestroy = 3f;
            float delayAfter = 0;

            string soundName = string.Empty;
            float soundDelay = 0;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 offset;

                Enumerators.VisualEffectType effectType = Enumerators.VisualEffectType.Impact;

                foreach (BoardUnitModel unit in units)
                {
                    offset = Vector3.zero;

                    switch (unit.InitialUnitType)
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

                    Transform unitTransform = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit).Transform;

                    Vector3 targetPosition = unitTransform.position;

                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(effectType).Path);

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
