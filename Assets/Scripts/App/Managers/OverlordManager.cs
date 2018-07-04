// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class OverlordManager : IService, IOverlordManager
    {
        public void Init()
        {

        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public void ChangeExperience(Hero hero, int value)
        {
            hero.experience += value;
            CheckLevel(hero);
        }

        private void CheckLevel(Hero hero)
        {
            if (hero.experience > 1000)
                LevelUP(hero);
        }

        private void LevelUP(Hero hero)
        {
            hero.level++;
            hero.experience = 0;
            hero.ValidateSkillLocking();
        }
    }
}