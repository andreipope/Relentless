using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public interface IOverlordExperienceManager
    {
        OverlordExperienceManager.ExperienceInfo PlayerMatchExperienceInfo { get; }
        OverlordExperienceManager.ExperienceInfo OpponentMatchExperienceInfo { get; }

        int GetRequiredExperienceForNewLevel(OverlordModel overlord);
        void ReportExperienceAction(Enumerators.ExperienceActionType actionType, bool isOpponent);
        OverlordExperienceManager.LevelReward GetLevelReward(OverlordModel overlord);

        void InitializeExperienceInfoInMatch(OverlordModel overlord);
        void InitializeOpponentExperienceInfoInMatch(OverlordModel overlord);
        Task GetLevelAndRewards(OverlordModel overlord);
    }
}
