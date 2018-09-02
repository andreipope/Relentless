using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class HeroWeaponAbility : AbilityBase
    {
        public int Health = 1;

        public int Damage = 1;

        public HeroWeaponAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Health = ability.Health;
            Damage = ability.Damage;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/HealingTouchVFX");
            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }
    }
}
