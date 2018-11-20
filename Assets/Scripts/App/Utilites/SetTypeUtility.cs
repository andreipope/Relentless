using System;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public static class SetTypeUtility
    {
        public static CardSet GetCardSet(IDataManager dataManager, Enumerators.SetType setType)
        {
            return dataManager.CachedCardsLibraryData.Sets.First(s => s.Name == setType);
        }

        public static Enumerators.SetType GetCardSetType(IDataManager dataManager, int setIndex)
        {
            return dataManager.CachedCardsLibraryData.Sets[setIndex].Name;
        }
    }
}
