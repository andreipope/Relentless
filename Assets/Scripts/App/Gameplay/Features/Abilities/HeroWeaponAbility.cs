using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrandDevs.CZB.Common;
using UnityEngine;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class HeroWeaponAbility : AbilityBase
    {
        public int health = 1;
        public int damage = 1;

        public HeroWeaponAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.health = ability.health;
            this.damage = ability.damage;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");
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

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            playerCallerOfAbility.AddWeapon(cardOwnerOfAbility);
            playerCallerOfAbility.CurrentBoardWeapon.InitWeapon(damage, health, playerCallerOfAbility, abilityTargetTypes);

            if (!playerCallerOfAbility.AlreadyAttackedInThisTurn)
                playerCallerOfAbility.CurrentBoardWeapon.ActivateWeapon(!(playerCallerOfAbility.IsLocalPlayer));
        }
    }
}