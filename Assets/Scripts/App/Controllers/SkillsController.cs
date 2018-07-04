// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class SkillsController : IController
    {
        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;
        private VFXController _vfxController;
        private BattleController _battleController;

        public void Dispose()
        {
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _vfxController = _gameplayManager.GetController<VFXController>();
            _battleController = _gameplayManager.GetController<BattleController>();
        }

        public void Update()
        {
        }

        public void InitializePrefabsForSkills()
        {

        }

        public void DoSkill(Player owner, HeroSkill skill)
        {

        }

        private void SkillParticleActionCompleted(object target)
        {
            //switch (skillType)
            //{
            //    case Enumerators.SetType.WATER:
            //        FreezeAction(target);
            //        break;
            //    case Enumerators.SetType.TOXIC:
            //        ToxicDamageAction(target);
            //        break;
            //    case Enumerators.SetType.FIRE:
            //        FireDamageAction(target);
            //        break;
            //    case Enumerators.SetType.LIFE:
            //        HealAnyAction(target);
            //        break;
            //    case Enumerators.SetType.AIR:
            //        //   CardReturnAction(target);
            //        break;
            //    default:
            //        break;
            //}
        }


        #region actions

        private void FreezeAction(Player owner, HeroSkill skill, object target)
        {
            if (target is BoardUnit)
            {
                var unit = target as BoardUnit;
                unit.Stun(skill.value);
                _vfxController.CreateSkillVFX(Enumerators.SetType.EARTH, unit.transform.position, target, SkillParticleActionCompleted);
            }
            else if (target is Player)
            {

            }
        }

        private void ToxicDamageAction(Player owner, HeroSkill skill, object target)
        {
            AttackWithModifiers(owner, skill, target, Enumerators.SetType.TOXIC, Enumerators.SetType.LIFE);
        }

        private void FireDamageAction(Player owner, HeroSkill skill, object target)
        {
            AttackWithModifiers(owner, skill, target, Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC);
        }

        private void HealAnyAction(Player owner, HeroSkill skill, object target)
        {
            if (target is Player)
            {
                var player = target as Player;

                _battleController.HealPlayerBySkill(owner, skill, player);

                _vfxController.CreateSkillVFX(Enumerators.SetType.EARTH, player.AvatarObject.transform.position, owner, SkillParticleActionCompleted);
            }
            else
            {
                var unit = target as BoardUnit;

                _battleController.HealCreatureBySkill(owner, skill, unit);

                _vfxController.CreateSkillVFX(Enumerators.SetType.EARTH, unit.transform.position, unit, SkillParticleActionCompleted);
            }
        }

        private void HealAction(Player owner, HeroSkill skill)
        {
            _battleController.HealPlayerBySkill(owner, skill, owner);

            _vfxController.CreateSkillVFX(Enumerators.SetType.EARTH, owner.AvatarObject.transform.position - Vector3.right * 2.3f, owner, SkillParticleActionCompleted);
        }

        private void AttackWithModifiers(Player owner, HeroSkill skill, object target, Enumerators.SetType attackType, Enumerators.SetType setType)
        {
            if (target is Player)
            {
                var player = target as Player;
                //TODO additional damage to heros

                _battleController.AttackPlayerBySkill(owner, skill, player);

                _vfxController.CreateSkillVFX(attackType, player.AvatarObject.transform.position, owner, SkillParticleActionCompleted);
            }
            else
            {
                var creature = target as BoardUnit;
                var attackModifier = 0;

                if (creature.Card.libraryCard.cardSetType == setType)
                    attackModifier = 1;

                _battleController.AttackCreatureBySkill(owner, skill, creature, attackModifier);

                _vfxController.CreateSkillVFX(attackType, creature.transform.position, owner, SkillParticleActionCompleted);
            }
        }
        #endregion
    }
}