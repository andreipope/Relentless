using System;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public static class SetTypeUtility
    {
        public static Faction GetCardFaction(IDataManager dataManager, Enumerators.Faction faction)
        {
            return dataManager.CachedCardsLibraryData.Factions.First(s => s.Name == faction);
        }

        public static Enumerators.Faction GetCardFaction(IDataManager dataManager, int setIndex)
        {
            return dataManager.CachedCardsLibraryData.Factions[setIndex].Name;
        }
    }
}
