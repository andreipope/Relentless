using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class MassiveDamageAbilityView : AbilityViewBase<MassiveDamageAbility>
    {
        public MassiveDamageAbilityView(MassiveDamageAbility ability) : base(ability)
        {
        }

        protected override void OnAbilityAction(object info = null)
        {
            foreach (Enumerators.AbilityTargetType target in Ability.AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                        CreateVfx(Vector3.up * 1.5f);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:
                        foreach (BoardUnitView cardPlayer in Ability.PlayerCallerOfAbility.BoardCards)
                        {
                            CreateVfx(cardPlayer.Transform.position);
                        }

                        break;
                }
            }
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            int playerPos = Ability.PlayerCallerOfAbility.IsLocalPlayer ? 1 : -1;

            switch (Ability.AbilityEffectType)
            {
                case Enumerators.AbilityEffectType.MASSIVE_WATER_WAVE:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/ToxicMassiveAllVFX");
                    break;
                case Enumerators.AbilityEffectType.MASSIVE_FIRE:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellMassiveFireVFX");
                    break;
                case Enumerators.AbilityEffectType.MASSIVE_LIGHTNING:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/LightningVFX");
                    pos = Vector3.up * 0.5f;
                    break;
                case Enumerators.AbilityEffectType.MASSIVE_TOXIC_ALL:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/ToxicMassiveAllVFX");
                    pos = Vector3.zero;
                    break;
            }

            pos = Utilites.CastVfxPosition(pos * playerPos);
            ClearParticles();

            base.CreateVfx(pos, true, 5f);
        }
    }
}
