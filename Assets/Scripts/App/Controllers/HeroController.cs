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
    public class HeroController : IController
    {
        public Hero playerHero;

        public void Init()
        {

        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public void ChangeExperience(int value)
        {
            playerHero.experience += value;
            CheckLevel();
        }

        private void CheckLevel()
        {
            if (playerHero.experience > 1000)
                LevelUP();
        }

        private void LevelUP()
        {
            playerHero.level++;
            playerHero.experience = 0;
            playerHero.ValidateSkillLocking();
        }
    }
}