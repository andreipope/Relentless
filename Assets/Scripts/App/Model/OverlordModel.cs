using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data {
    public class OverlordModel
    {
        public OverlordId Id { get; }

        public string Icon { get; }

        public string Name { get; }

        public string ShortDescription { get; }

        public string LongDescription { get; }

        public long Experience { get; set; }

        public int Level { get; set; }

        public Enumerators.Faction Faction { get; }

        public List<OverlordSkill> Skills { get; }

        public int InitialDefense { get; }

        public string FullName => $"{Name}, {ShortDescription}";

        public OverlordModel(
            OverlordId id,
            string icon,
            string name,
            string shortDescription,
            string longDescription,
            long experience,
            int level,
            Enumerators.Faction faction,
            List<OverlordSkill> skills,
            int initialDefense)
        {
            Id = id;
            Icon = icon;
            Name = name;
            ShortDescription = shortDescription;
            LongDescription = longDescription;
            Experience = experience;
            Level = level;
            Faction = faction;
            Skills = skills ?? new List<OverlordSkill>();
            InitialDefense = initialDefense;
        }

        public OverlordSkill GetSkill(Enumerators.Skill skill)
        {
            return Skills.Find(x => x.Skill == skill);
        }
    }
}
