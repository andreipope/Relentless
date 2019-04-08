using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data {
    public class OverlordSkillData
    {
        public int Id { get; }

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

        public bool Unlocked { get; set; }

        public bool CanSelectTarget { get; }

        public bool SingleUse { get; }

        public OverlordSkillData(
            int id,
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
            Damage = damage;
            Count = count;
            Skill = skill;
            SkillTargets = skillTargets ?? new List<Enumerators.SkillTarget>();
            TargetUnitSpecialStatus = targetUnitSpecialStatus;
            TargetFactions = targetFactions ?? new List<Enumerators.Faction>();
            CanSelectTarget = canSelectTarget;
            Unlocked = unlocked;
            SingleUse = singleUse;
        }
    }
}