using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class HeroesData
    {
        public List<Hero> Heroes { get; private set; }

        public HeroesData(List<Hero> heroes)
<<<<<<< HEAD
        {
            Heroes = heroes ?? throw new ArgumentNullException(nameof(heroes));
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            foreach (Hero item in Heroes)
            {
                item.ValidateSkillLocking();
            }
=======
        {
            Heroes = heroes ?? throw new ArgumentNullException(nameof(heroes));
>>>>>>> content-development
        }
    }

    public class Hero
    {
        public int HeroId { get; private set; }
<<<<<<< HEAD

        public string Icon { get; private set; }

        public string Name { get; private set; }

        public string ShortDescription { get; private set; }

        public string LongDescription { get; private set; }

        public long Experience { get; set; }

        public int Level { get; set; }

        [JsonProperty("Element")]
        public Enumerators.SetType HeroElement { get; private set; }

        public List<HeroSkill> Skills { get; private set; }

        public int PrimarySkill { get; set; }

        public int SecondarySkill { get; set; }
=======

        public string Icon { get; private set; }

        public string Name { get; private set; }

        public string ShortDescription { get; private set; }

        public string LongDescription { get; private set; }

        public long Experience { get; set; }

        public int Level { get; set; }

        [JsonProperty("Element")]
        public Enumerators.SetType HeroElement { get; private set; }

        public List<HeroSkill> Skills { get; private set; }

        public Enumerators.OverlordSkill PrimarySkill;

        public Enumerators.OverlordSkill  SecondarySkill;
>>>>>>> content-development

        public string FullName => $"{Name}, {ShortDescription}";

        public Hero(
            int heroId,
            string icon,
            string name,
            string shortDescription,
            string longDescription,
            long experience,
            int level,
            Enumerators.SetType heroElement,
            List<HeroSkill> skills,
<<<<<<< HEAD
            int primarySkill,
            int secondarySkill)
        {
            HeroId = heroId;
            Icon = icon;
            Name = name;
            ShortDescription = shortDescription;
            LongDescription = longDescription;
            Experience = experience;
            Level = level;
            HeroElement = heroElement;
            Skills = skills ?? new List<HeroSkill>();
            PrimarySkill = primarySkill;
            SecondarySkill = secondarySkill;
        }

        public void ValidateSkillLocking()
=======
            Enumerators.OverlordSkill primaryAbility,
            Enumerators.OverlordSkill secondaryAbility)
>>>>>>> content-development
        {
            HeroId = heroId;
            Icon = icon;
            Name = name;
            ShortDescription = shortDescription;
            LongDescription = longDescription;
            Experience = experience;
            Level = level;
            HeroElement = heroElement;
            Skills = skills ?? new List<HeroSkill>();
            PrimarySkill = primaryAbility;
            SecondarySkill = secondaryAbility;
        }
<<<<<<< HEAD
=======

        public HeroSkill GetSkill(Enumerators.OverlordSkill skill)
        {
            return Skills.Find(x => x.OverlordSkill == skill);
        }


        public HeroSkill GetSkill(int id)
        {
            return Skills.Find(x => x.Id == id);
        }
>>>>>>> content-development
    }

    public class HeroSkill
    {
<<<<<<< HEAD
=======
        public int Id { get; private set; }

>>>>>>> content-development
        public string Title { get; private set; }

        public string IconPath { get; private set; }

        public string Description { get; private set; }

        public int Cooldown { get; private set; }

        public int InitialCooldown { get; private set; }

        public int Value { get; private set; }

        public int Attack { get; private set; }

        public int Count { get; private set; }

        [JsonProperty("Skill")]
        public Enumerators.OverlordSkill OverlordSkill { get; private set; }

        [JsonProperty("SkillTargets")]
        public List<Enumerators.SkillTargetType> SkillTargetTypes { get; private set; }

<<<<<<< HEAD
        [JsonProperty("UnitStatus")]
=======
        [JsonProperty("TargetUnitSpecialStatus")]
>>>>>>> content-development
        public Enumerators.UnitStatusType TargetUnitStatusType { get; private set; }

        [JsonProperty("ElementTargets")]
        public List<Enumerators.SetType> ElementTargetTypes { get; private set; }

<<<<<<< HEAD
        [JsonIgnore]
        public bool Unlocked { get; set; }

        public HeroSkill(
=======
        public bool Unlocked { get; set; }

        public bool CanSelectTarget { get; private set; }

        public HeroSkill(
            int id,
>>>>>>> content-development
            string title,
            string iconPath,
            string description,
            int cooldown,
            int initialCooldown,
            int value,
            int attack,
            int count,
            Enumerators.OverlordSkill overlordSkill,
            List<Enumerators.SkillTargetType> skillTargetTypes,
            Enumerators.UnitStatusType targetUnitStatusType,
<<<<<<< HEAD
            List<Enumerators.SetType> elementTargetTypes)
        {
=======
            List<Enumerators.SetType> elementTargetTypes,
            bool unlocked,
            bool canSelectTarget)
        {
            Id = id;
>>>>>>> content-development
            Title = title;
            IconPath = iconPath;
            Description = description;
            Cooldown = cooldown;
            InitialCooldown = initialCooldown;
            Value = value;
            Attack = attack;
            Count = count;
            OverlordSkill = overlordSkill;
            SkillTargetTypes = skillTargetTypes ?? new List<Enumerators.SkillTargetType>();
            TargetUnitStatusType = targetUnitStatusType;
            ElementTargetTypes = elementTargetTypes ?? new List<Enumerators.SetType>();
<<<<<<< HEAD
=======
            CanSelectTarget = canSelectTarget;
            Unlocked = unlocked;
>>>>>>> content-development
        }
    }
}
