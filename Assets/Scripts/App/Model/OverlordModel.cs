using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data {
    public class OverlordModel
    {
        public int OverlordId { get; }

        public string Icon { get; }

        public string Name { get; }

        public string ShortDescription { get; }

        public string LongDescription { get; }

        public long Experience { get; set; }

        public int Level { get; set; }

        public Enumerators.Faction Faction { get; }

        public List<OverlordSkillData> Skills { get; }

        public Enumerators.Skill PrimarySkill { get; set; }

        public Enumerators.Skill SecondarySkill { get; set; }

        public string FullName => $"{Name}, {ShortDescription}";

        public OverlordModel(
            int overlordId,
            string icon,
            string name,
            string shortDescription,
            string longDescription,
            long experience,
            int level,
            Enumerators.Faction faction,
            List<OverlordSkillData> skills,
            Enumerators.Skill primarySkill,
            Enumerators.Skill secondarySkill)
        {
            OverlordId = overlordId;
            Icon = icon;
            Name = name;
            ShortDescription = shortDescription;
            LongDescription = longDescription;
            Experience = experience;
            Level = level;
            Faction = faction;
            Skills = skills ?? new List<OverlordSkillData>();
            PrimarySkill = primarySkill;
            SecondarySkill = secondarySkill;
        }

        public OverlordSkillData GetSkill(Enumerators.Skill skill)
        {
            return Skills.Find(x => x.Skill == skill);
        }

        public OverlordSkillData GetSkill(int index)
        {
            return Skills[index];
        }
    }
}