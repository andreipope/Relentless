using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public interface IOverlordExperienceManager
    {
        OverlordExperienceManager.ExperienceInfo MatchExperienceInfo { get; }
        OverlordExperienceManager.ExperienceInfo OpponentMatchExperienceInfo { get; }

        void ChangeExperience(OverlordModel overlord, int value);
        int GetRequiredExperienceForNewLevel(OverlordModel overlord);
        void ReportExperienceAction(OverlordModel overlord, Enumerators.ExperienceActionType actionType);
        void ReportExperienceAction(Enumerators.ExperienceActionType actionType, bool isOpponent);
        OverlordExperienceManager.LevelReward GetLevelReward(OverlordModel overlord);

        void InitializeExperienceInfoInMatch(OverlordModel overlord);
        void InitializeOpponentExperienceInfoInMatch(OverlordModel overlord);
        Task ApplyExperienceFromMatch(OverlordModel overlord);
        Task GetLevelAndRewards(OverlordModel overlord);
        void ApplyExperience(OverlordModel overlord, int experience);
    }
}
