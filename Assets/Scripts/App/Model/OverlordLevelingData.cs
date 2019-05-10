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

    public class EndMatchResults
    {
        public DeckId DeckId { get; }

        public OverlordId OverlordId { get; }

        public int PreviousLevel { get; }

        public long PreviousExperience { get; }

        public int CurrentLevel { get; }

        public long CurrentExperience { get; }

        public bool IsWin { get; }

        public IReadOnlyList<LevelReward> LevelRewards { get; }

        public EndMatchResults(
            DeckId deckId,
            OverlordId overlordId,
            int previousLevel,
            long previousExperience,
            int currentLevel,
            long currentExperience,
            bool isWin,
            IReadOnlyList<LevelReward> levelRewards)
        {
            DeckId = deckId;
            OverlordId = overlordId;
            PreviousLevel = previousLevel;
            PreviousExperience = previousExperience;
            CurrentLevel = currentLevel;
            CurrentExperience = currentExperience;
            IsWin = isWin;
            LevelRewards = levelRewards;
        }
    }


    public class MatchExperienceInfo
    {
        public long ExperienceReceived { get; set; }
    }
}
