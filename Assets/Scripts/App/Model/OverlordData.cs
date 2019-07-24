using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public class OverlordData
    {
        public List<OverlordUserInstance> Overlords { get; }

        public OverlordData(List<OverlordUserInstance> overlords)
        {
            Overlords = overlords ?? throw new ArgumentNullException(nameof(overlords));
        }

        public OverlordUserInstance GetOverlordById(OverlordId id)
        {
            return Overlords.Single(model => model.Prototype.Id == id);
        }
    }

    public class OverlordUserInstance
    {
        public OverlordPrototype Prototype { get; }

        public OverlordUserData UserData { get; }

        public IReadOnlyList<OverlordSkillUserInstance> Skills { get; }

        public OverlordUserInstance(OverlordPrototype prototype, OverlordUserData userData, IReadOnlyList<OverlordSkillUserInstance> skills)
        {
            Prototype = prototype;
            UserData = userData;
            Skills = skills;
        }

        public OverlordSkillUserInstance GetSkill(Enumerators.Skill skill)
        {
            return Skills.FirstOrDefault(x => x.Prototype.Skill == skill);
        }
    }

    public class OverlordUserData
    {
        public int Level { get; set; }

        public long Experience { get; set; }

        public OverlordUserData(int level, long experience)
        {
            Level = level;
            Experience = experience;
        }
    }

    public class OverlordPrototype
    {
        public OverlordId Id { get; }

        public string Icon { get; }

        public string Name { get; }

        public string ShortDescription { get; }

        public string LongDescription { get; }

        public Enumerators.Faction Faction { get; }

        public IReadOnlyList<OverlordSkillPrototype> Skills { get; }

        public int InitialDefense { get; }

        public string FullName => $"{Name}, {ShortDescription}";

        public OverlordPrototype(
            OverlordId id,
            string icon,
            string name,
            string shortDescription,
            string longDescription,
            Enumerators.Faction faction,
            IReadOnlyList<OverlordSkillPrototype> skills,
            int initialDefense)
        {
            Id = id;
            Icon = icon;
            Name = name;
            ShortDescription = shortDescription;
            LongDescription = longDescription;
            Faction = faction;
            Skills = skills ?? new List<OverlordSkillPrototype>();
            InitialDefense = initialDefense;
        }

        public OverlordSkillPrototype GetSkill(Enumerators.Skill skill)
        {
            return Skills.FirstOrDefault(x => x.Skill == skill);
        }
    }

    public class OverlordSkillUserInstance {
        public OverlordSkillPrototype Prototype { get; }

        public OverlordSkillUserData UserData { get; }

        public OverlordSkillUserInstance(OverlordSkillPrototype prototype, OverlordSkillUserData userData)
        {
            Prototype = prototype;
            UserData = userData;
        }
    }

    public class OverlordSkillUserData
    {
        public bool IsUnlocked { get; set; }

        public OverlordSkillUserData(bool isUnlocked)
        {
            IsUnlocked = isUnlocked;
        }
    }

    public class OverlordSkillPrototype
    {
        public SkillId Id { get; }

        public string Title { get; }

        public string IconPath { get; }

        public string Description { get; }

        public int Cooldown { get; }

        public int InitialCooldown { get; }

        public int Value { get; }

        public int Damage { get; }

        public int Count { get; }

        public Enumerators.Skill Skill { get; }

        public List<Enumerators.SkillTarget> SkillTargets { get; }

        public Enumerators.UnitSpecialStatus TargetUnitSpecialStatus { get; }

        public List<Enumerators.Faction> TargetFactions { get; }

        public bool CanSelectTarget { get; }

        public bool SingleUse { get; }

        public OverlordSkillPrototype(
            SkillId id,
            string title,
            string iconPath,
            string description,
            int cooldown,
            int initialCooldown,
            int value,
            int damage,
            int count,
            Enumerators.Skill skill,
            List<Enumerators.SkillTarget> skillTargets,
            Enumerators.UnitSpecialStatus targetUnitSpecialStatus,
            List<Enumerators.Faction> targetFactions,
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
            Damage = damage;
            Count = count;
            Skill = skill;
            SkillTargets = skillTargets ?? new List<Enumerators.SkillTarget>();
            TargetUnitSpecialStatus = targetUnitSpecialStatus;
            TargetFactions = targetFactions ?? new List<Enumerators.Faction>();
            CanSelectTarget = canSelectTarget;
            SingleUse = singleUse;
        }
    }
}
