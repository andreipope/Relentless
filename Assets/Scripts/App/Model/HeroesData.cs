using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.Internal;
using Newtonsoft.Json;

namespace LoomNetwork.CZB.Data
{
    public class HeroesData
    {
        public List<Hero> Heroes;

        [JsonIgnore]
        private bool _casted;

        [JsonIgnore]
        public List<Hero> HeroesParsed
        {
            get
            {
                if (!_casted)
                {
                    CastData();
                }

                return Heroes;
            }
        }

        public void CastData()
        {
            foreach (Hero item in Heroes)
            {
                item.HeroElement = Utilites.CastStringTuEnum<Enumerators.SetType>(item.Element);
                item.CastSkills();
                item.ValidateSkillLocking();
            }

            _casted = true;
        }
    }

    public class Hero
    {
        public int HeroId;

        public string Icon;

        public string Name;

        public string ShortDescription;

        public string LongDescription;

        public string Element;

        public int Experience;

        public int Level;

        [JsonIgnore]
        public Enumerators.SetType HeroElement;

        public List<HeroSkill> Skills;

        public int PrimarySkill;

        public int SecondarySkill;

        public string FullName => $"{Name}, {ShortDescription}";

        public void ValidateSkillLocking()
        {
            int skillId = Level % 4;
            for (int i = 0; i < skillId; i++)
            {
                Skills[i].Unlocked = true;
            }
        }

        public void CastSkills()
        {
            foreach (HeroSkill skill in Skills)
            {
                skill.CastData();
            }
        }
    }

    public class HeroSkill
    {
        public string Title;

        public string Skill;

        public string IconPath;

        public string Description;

        public string SkillTargets;

        public string ElementTargets;

        public int Cooldown;

        public int InitialCooldown;

        public int Value;

        public int Attack;

        public int Health;

        [JsonIgnore]
        public bool Unlocked;

        [JsonIgnore]
        public Enumerators.OverlordSkill OverlordSkill;

        [JsonIgnore]
        public List<Enumerators.SkillTargetType> SkillTargetTypes;

        public List<Enumerators.SetType> ElementTargetTypes;

        public void CastData()
        {
            if (!string.IsNullOrEmpty(Skill))
            {
                OverlordSkill = Utilites.CastStringTuEnum<Enumerators.OverlordSkill>(Skill);
            }

            if (!string.IsNullOrEmpty(SkillTargets))
            {
                SkillTargetTypes = Utilites.CastList<Enumerators.SkillTargetType>(SkillTargets);
            }
            else
            {
                SkillTargetTypes = new List<Enumerators.SkillTargetType>();
            }

            if (!string.IsNullOrEmpty(ElementTargets))
            {
                ElementTargetTypes = Utilites.CastList<Enumerators.SetType>(ElementTargets);
            }
            else
            {
                ElementTargetTypes = new List<Enumerators.SetType>();
            }
        }
    }
}
