using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public class Hero
    {
        public Enumerators.ElementType element;
        public string name;
        public HeroSkill skill;
        public int heroId;

        public Hero(int heroId)
        {
            this.heroId = heroId;
        }
    }
}