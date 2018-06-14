using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB.Data
{
    public class HeroesData
    {
        public List<Hero> heroes;

        public HeroesData()
        {
        }
    }

    public class Hero
    {
        public int heroId;
        public string icon;
        public string name;
        public Enumerators.SetType element;
        public HeroSkill skill;

        public Hero()
        {
        }
    }

    public class HeroSkill
    {
        public string title;
        public Enumerators.SkillTargetType skillTargetType;
        public int cost;
        public int value;

        public HeroSkill()
        {

        }
    }
}