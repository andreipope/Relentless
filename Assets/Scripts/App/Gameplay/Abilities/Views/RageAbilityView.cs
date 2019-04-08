using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class RageAbilityView : AbilityViewBase<RageAbility>
    {
        private BattlegroundController _battlegroundController;

        public RageAbilityView(RageAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            bool state = true;
            if(info != null)
            {
                state = (bool)info;
            }

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                if (state)
                {
                    BoardUnitView viewOwner = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(Ability.AbilityUnitOwner);
                    Vector3 targetPosition = viewOwner.Transform.position;

                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                    VfxObject = Object.Instantiate(VfxObject, viewOwner.Transform, false);
                    VfxObject.transform.localPosition = Vector3.up * 0.3f;
                    ParticlesController.RegisterParticleSystem(VfxObject, false);

                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.RAGE_LOOP, Constants.ZombiesSoundVolume, isLoop:true);
                }
                else
                {
                    Object.Destroy(VfxObject);
                    GameClient.Get<ISoundManager>().StopPlaying(Enumerators.SoundType.RAGE_LOOP);
                }
            }

            Ability.InvokeVFXAnimationEnded();
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, true, 5f);
        }
    }
}
