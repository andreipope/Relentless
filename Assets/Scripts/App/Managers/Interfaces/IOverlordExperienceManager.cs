using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public interface IOverlordExperienceManager
    {
        ExperienceInfo PlayerMatchExperienceInfo { get; }

        ExperienceInfo OpponentMatchExperienceInfo { get; }

        void InitializeMatchExperience(OverlordModel playerOverlord, OverlordModel opponentOverlord);

        void ReportExperienceAction(Enumerators.ExperienceActionType actionType, ExperienceInfo experienceInfo);

        long GetRequiredExperienceForLevel(int level);

        Task<ExperienceDeltaInfo> UpdateLevelAndExperience(OverlordModel overlordModel);
        Task<(int? notificationId, ExperienceDeltaInfo experienceDeltaInfo, bool isWin)> GetExperienceDeltaInfoFromEndMatchNotification();
    }
}
