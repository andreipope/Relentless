using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public class OverlordLevelingData
    {
        public List<LevelReward> Rewards { get; }

        public List<ExperienceAction> ExperienceActions { get; }

        public int Fixed { get; }

        public int ExperienceStep { get; }

        public int GooRewardStep { get; }

        public int MaxLevel { get; }

        public OverlordLevelingData(
            List<LevelReward> rewards,
            List<ExperienceAction> experienceActions,
            int @fixed,
            int experienceStep,
            int gooRewardStep,
            int maxLevel)
        {
            Rewards = rewards;
            ExperienceActions = experienceActions;
            Fixed = @fixed;
            ExperienceStep = experienceStep;
            GooRewardStep = gooRewardStep;
            MaxLevel = maxLevel;
        }
    }

    public abstract class LevelReward
    {
        public int Level { get; }

        protected LevelReward(int level)
        {
            Level = level;
        }
    }

    public class UnitRewardItem : LevelReward
    {
        public Enumerators.CardRank Rank { get; }

        public int Count { get; }

        public UnitRewardItem(int level, Enumerators.CardRank rank, int count) : base(level)
        {
            Rank = rank;
            Count = count;
        }
    }

    public class OverlordSkillRewardItem : LevelReward
    {
        public int SkillIndex { get; }

        public OverlordSkillRewardItem(int level, int skillIndex) : base(level)
        {
            SkillIndex = skillIndex;
        }
    }

    public class ExperienceAction
    {
        public Enumerators.ExperienceActionType Action { get; }

        public int Experience { get; }

        public ExperienceAction(Enumerators.ExperienceActionType action, int experience)
        {
            Action = action;
            Experience = experience;
        }
    }

    public class ExperienceInfo
    {
        public int LevelAtBegin { get; }

        public long ExperienceAtBegin { get; }

        public long ExperienceReceived { get; set; }

        public List<LevelReward> GotRewards { get; }

        public ExperienceInfo(int levelAtBegin, long experienceAtBegin)
        {
            LevelAtBegin = levelAtBegin;
            ExperienceAtBegin = experienceAtBegin;
        }
    }
}
