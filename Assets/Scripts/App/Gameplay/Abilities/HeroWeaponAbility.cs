using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class HeroWeaponAbility : AbilityBase
    {
        public int Defense { get; }

        public int Damage { get; }

        public HeroWeaponAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Defense = ability.Defense;
            Damage = ability.Damage;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX");
            Action();
        }
    }
}
