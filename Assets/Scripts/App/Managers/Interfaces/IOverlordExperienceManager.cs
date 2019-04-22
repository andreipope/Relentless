using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public interface IOverlordExperienceManager
    {
        OverlordExperienceManager.ExperienceInfo MatchExperienceInfo { get; }

        void ChangeExperience(OverlordModel overlord, int value);
        int GetRequiredExperienceForNewLevel(OverlordModel overlord);
        void ReportExperienceAction(OverlordModel overlord, Enumerators.ExperienceActionType actionType);
        OverlordExperienceManager.LevelReward GetLevelReward(OverlordModel overlord);

        void InitializeExperienceInfoInMatch(OverlordModel overlord);
        Task ApplyExperienceFromMatch(OverlordModel overlord);
        void ApplyExperience(OverlordModel overlord, int experience);
    }
}
