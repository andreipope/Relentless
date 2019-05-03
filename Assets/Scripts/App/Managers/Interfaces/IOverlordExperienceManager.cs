using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public interface IOverlordExperienceManager
    {
        ExperienceInfo PlayerMatchExperienceInfo { get; }
        ExperienceInfo OpponentMatchExperienceInfo { get; }

        long GetRequiredExperienceForNewLevel(OverlordModel overlord);
        void ReportExperienceAction(Enumerators.ExperienceActionType actionType, bool isOpponent);
        LevelReward GetLevelReward(OverlordModel overlord);

        void InitializeExperienceInfoInMatch(OverlordModel overlord);
        void InitializeOpponentExperienceInfoInMatch(OverlordModel overlord);
        Task GetLevelAndRewards(OverlordModel overlord);
    }
}
