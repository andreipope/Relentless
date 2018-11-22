using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AddGooVialsAbilityView : AbilityViewBase<AddGooVialsAbility>
    {
        private BattlegroundController _battlegroundController;

        private IUIManager _uiManager;

        private string _cardName;

        public AddGooVialsAbilityView(AddGooVialsAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
            _uiManager = GameClient.Get<IUIManager>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            _cardName = "";
            float delayAfter = 0;
            float delayBeforeDestroy = 3f;
            string soundName = string.Empty;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                PlayerManaBarItem manaBarItem = Ability.PlayerCallerOfAbility.IsLocalPlayer ?
                    _uiManager.GetPage<GameplayPage>().PlayerManaBar :
                    _uiManager.GetPage<GameplayPage>().OpponentManaBar;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    _cardName = effectInfo.cardName;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    soundName = effectInfo.soundName;
                }

                manaBarItem.SetViaGooPrefab(VfxObject);

                if (!string.IsNullOrEmpty(soundName))
                {
                    SoundManager.PlaySound(Enumerators.SoundType.SPELLS, soundName, Constants.SfxSoundVolume, Enumerators.CardSoundType.NONE);
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
