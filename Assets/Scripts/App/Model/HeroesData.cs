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
        public List<Hero> Heroes { get; }

        public HeroesData(List<Hero> heroes)
        {
            Heroes = heroes ?? throw new ArgumentNullException(nameof(heroes));
        }
    }

    public class Hero
    {
        public int HeroId { get; }

        public string Icon { get; }

        public string Name { get; }

        public string ShortDescription { get; }

        public string LongDescription { get; }

        public long Experience { get; set; }

        public int Level { get; set; }

        [JsonProperty("Element")]
        public Enumerators.Faction HeroElement { get; private set; }

        public List<HeroSkill> Skills { get; }

        public Enumerators.OverlordSkill PrimarySkill;

        public Enumerators.OverlordSkill  SecondarySkill;

        public string FullName => $"{Name}, {ShortDescription}";

        public Hero(
            int heroId,
            string icon,
            string name,
            string shortDescription,
            string longDescription,
            long experience,
            int level,
            Enumerators.Faction heroElement,
            List<HeroSkill> skills,

            Enumerators.OverlordSkill primaryAbility,
            Enumerators.OverlordSkill secondaryAbility)
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

        public HeroSkill GetSkill(Enumerators.OverlordSkill skill)
        {
            return Skills.Find(x => x.OverlordSkill == skill);
        }

        public HeroSkill GetSkill(int index)
        {
            return Skills[index];
        }
    }

    public class HeroSkill
    {
        public int Id { get; }

        public string Title { get; }

        public string IconPath { get; }

        public string Description { get; }

        public int Cooldown { get; }

        public int InitialCooldown { get; }

        public int Value { get; }

        public int Attack { get; }

        public int Count { get; }

        [JsonProperty("Skill")]
        public Enumerators.OverlordSkill OverlordSkill { get; private set; }

        [JsonProperty("SkillTargets")]
        public List<Enumerators.SkillTargetType> SkillTargetTypes { get; private set; }

        [JsonProperty("TargetUnitSpecialStatus")]
        public Enumerators.UnitStatus TargetUnitStatusType { get; private set; }

        [JsonProperty("ElementTargets")]
        public List<Enumerators.Faction> ElementTargetTypes { get; private set; }

        public bool Unlocked { get; set; }

        public bool CanSelectTarget { get; }

        public bool SingleUse { get; }

        public HeroSkill(
            int id,
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
            Enumerators.UnitStatus targetUnitStatusType,
            List<Enumerators.Faction> elementTargetTypes,
            bool unlocked,
            bool canSelectTarget,
            bool singleUse)
        {
            Id = id;
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
            ElementTargetTypes = elementTargetTypes ?? new List<Enumerators.Faction>();
            CanSelectTarget = canSelectTarget;
            Unlocked = unlocked;
            SingleUse = singleUse;
        }
    }
}
