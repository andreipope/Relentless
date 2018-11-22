using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SwingAbilityView : AbilityViewBase<SwingAbility>
    {
        private BattlegroundController _battlegroundController;

        private string _cardName;

        public SwingAbilityView(SwingAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            _cardName = "";
            float delayAfter = 0;
            float delayBeforeDestroy = 3f;
            string soundName = string.Empty;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                BoardUnitModel unit = (BoardUnitModel)info;

                Vector3 targetPosition = _battlegroundController.GetBoardUnitViewByModel(unit).Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    _cardName = effectInfo.cardName;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    soundName = effectInfo.soundName;
                }

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = targetPosition;
                ParticlesController.RegisterParticleSystem(VfxObject, true, delayBeforeDestroy);
            }

            if (!string.IsNullOrEmpty(soundName))
            {
                SoundManager.PlaySound(Enumerators.SoundType.CARDS, soundName, Constants.SfxSoundVolume, Enumerators.CardSoundType.NONE);
            }

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, true, 5f);
        }
    }
}
