using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
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
            float delayAfter = 0;
            float delayBeforeDestroy = 3f;
            float delaySound = 0;
            string soundName = string.Empty;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    soundName = effectInfo.soundName;
                    delaySound = effectInfo.delayForSound;
                }

                foreach (CardModel unit in Ability.PlayerCallerOfAbility.CardsOnBoard)
                {
                    BoardUnitView unitView = _battlegroundController.GetCardViewByModel<BoardUnitView>(unit);
                    Vector3 targetPosition = unitView.Transform.position;
                    VfxObject = Object.Instantiate(VfxObject);
                    VfxObject.transform.SetParent(unitView.Transform, false);
                    VfxObject.transform.localPosition = Vector3.zero;
                    ParticlesController.RegisterParticleSystem(VfxObject, true, delayBeforeDestroy);
                }
            }

            if (!string.IsNullOrEmpty(soundName))
            {
                PlaySound(soundName, delaySound);
            }

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
