using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AddGooVialsAbilityView : AbilityViewBase<AddGooVialsAbility>
    {
        private BattlegroundController _battlegroundController;

        private IUIManager _uiManager;

        public AddGooVialsAbilityView(AddGooVialsAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
            _uiManager = GameClient.Get<IUIManager>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                PlayerManaBarItem manaBarItem = Ability.PlayerCallerOfAbility.IsLocalPlayer ?
                    _uiManager.GetPage<GameplayPage>().PlayerManaBar :
                    _uiManager.GetPage<GameplayPage>().OpponentManaBar;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                manaBarItem.SetViaGooPrefab(VfxObject);
            }

            Ability.InvokeVFXAnimationEnded();
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
