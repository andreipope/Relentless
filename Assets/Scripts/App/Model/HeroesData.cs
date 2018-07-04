// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using Newtonsoft.Json;
using LoomNetwork.Internal;

namespace LoomNetwork.CZB.Data
{
    public class HeroesData
    {
        public List<Hero> heroes;
        [JsonIgnore]
        private bool _casted;
        [JsonIgnore]
        public List<Hero> Heroes
        {
            get {
                if (!_casted)
                    CastData();
                return heroes; }
        }

        public HeroesData()
        {

        }

        public void CastData()
        {
            foreach (var item in heroes)
            {
                item.heroElement = Utilites.CastStringTuEnum<Enumerators.SetType>(item.element);
                item.ValidateSkillLocking();
            }
            _casted = true;           
        }
    }

    public class Hero
    {
        public int heroId;
        public string icon;
        public string name;
        public string element;
        public int experience;
        public int level;
        [JsonIgnore]
        public Enumerators.SetType heroElement;
        public List<HeroSkill> skills;
        public int primarySkill;
        public int secondarySkill;

        public Hero()
        {

        }

        public void ValidateSkillLocking()
        {
            int skillId = level % 4;
            for(int i = 0; i < skillId; i++)
                skills[i].unlocked = true;
        }
    }

    public class HeroSkill
    {
        public string title;
        public Enumerators.SkillTargetType skillTargetType;
        public int cooldown;
        public int initialCooldown;
        public int value;

        [JsonIgnore]
        public bool unlocked;

        public HeroSkill()
        {

        }
    }
}