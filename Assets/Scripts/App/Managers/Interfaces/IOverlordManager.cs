using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public interface IOverlordManager
    {
        void ChangeExperience(Hero hero, int value);
        int GetRequiredXPForNewLevel(Hero hero);
        void ReportXPAction(Hero hero, Enumerators.XPActionType actionType);
    }
}
