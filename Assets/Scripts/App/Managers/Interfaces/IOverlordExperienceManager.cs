using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public interface IOverlordExperienceManager
    {
        OverlordExperienceManager.ExperienceInfo MatchExperienceInfo { get; }

        void ChangeExperience(Hero hero, int value);
        int GetRequiredExperienceForNewLevel(Hero hero);
        void ReportExperienceAction(Hero hero, Enumerators.ExperienceActionType actionType);
        OverlordExperienceManager.LevelReward GetLevelReward(Hero hero);

        void InitializeExperienceInfoInMatch(Hero hero);
        void ApplyExperienceFromMatch(Hero hero);
    }
}
