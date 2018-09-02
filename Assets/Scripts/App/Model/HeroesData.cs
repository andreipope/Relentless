// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using Loom.Newtonsoft.Json;
using LoomNetwork.CZB.Common;
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
            get
            {
                if (!_casted)
                {
                    CastData();
                }

                return heroes;
            }
        }

        public void CastData()
        {
            foreach (Hero item in heroes)
            {
                item.heroElement = Utilites.CastStringTuEnum<Enumerators.SetType>(item.element);
                item.CastSkills();
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

        public string shortDescription;

        public string longDescription;

        public string element;

        public int experience;

        public int level;

        [JsonIgnore]
        public Enumerators.SetType heroElement;

        public List<HeroSkill> skills;

        public int primarySkill;

        public int secondarySkill;

        public string FullName => $"{name}, {shortDescription}";

        public void ValidateSkillLocking()
        {
            int skillId = level % 4;
            for (int i = 0; i < skillId; i++)
            {
                skills[i].unlocked = true;
            }
        }

        public void CastSkills()
        {
            foreach (HeroSkill skill in skills)
            {
                skill.CastData();
            }
        }
    }

    public class HeroSkill
    {
        public string title;

        public string skill;

        public string iconPath;

        public string description;

        public string skillTargets;

        public string elementTargets;

        public int cooldown;

        public int initialCooldown;

        public int value;

        public int attack;

        public int health;

        [JsonIgnore]
        public bool unlocked;

        [JsonIgnore]
        public Enumerators.OverlordSkill overlordSkill;

        [JsonIgnore]
        public List<Enumerators.SkillTargetType> skillTargetTypes;

        public List<Enumerators.SetType> elementTargetTypes;

        public void CastData()
        {
            if (!string.IsNullOrEmpty(skill))
            {
                overlordSkill = Utilites.CastStringTuEnum<Enumerators.OverlordSkill>(skill);
            }

            if (!string.IsNullOrEmpty(skillTargets))
            {
                skillTargetTypes = Utilites.CastList<Enumerators.SkillTargetType>(skillTargets);
            } else
            {
                skillTargetTypes = new List<Enumerators.SkillTargetType>();
            }

            if (!string.IsNullOrEmpty(elementTargets))
            {
                elementTargetTypes = Utilites.CastList<Enumerators.SetType>(elementTargets);
            } else
            {
                elementTargetTypes = new List<Enumerators.SetType>();
            }
        }
    }
}
