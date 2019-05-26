using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public interface IOverlordExperienceManager
    {
        MatchExperienceInfo PlayerMatchMatchExperienceInfo { get; }

        MatchExperienceInfo OpponentMatchMatchExperienceInfo { get; }

        void InitializeMatchExperience(OverlordModel playerOverlord, OverlordModel opponentOverlord);

        void ReportExperienceAction(Enumerators.ExperienceActionType actionType, MatchExperienceInfo matchExperienceInfo);

        long GetRequiredExperienceForLevel(int level);

        Task<(int? notificationId, EndMatchResults endMatchResults)> GetEndMatchResultsFromEndMatchNotification();
    }
}
