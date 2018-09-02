using System;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public static class SetTypeUtility
    {
        public static CardSet GetCardSet(IDataManager dataManager, Enumerators.SetType setType)
        {
            int setIndex = dataManager.CachedCardsLibraryData.sets.FindIndex(s => s.name.Equals(setType.ToString(), StringComparison.InvariantCultureIgnoreCase));
            return dataManager.CachedCardsLibraryData.sets[setIndex];
        }

        public static Enumerators.SetType GetCardSetType(IDataManager dataManager, int setIndex)
        {
            string setName = dataManager.CachedCardsLibraryData.sets[setIndex].name;
            return (Enumerators.SetType)Enum.Parse(typeof(Enumerators.SetType), setName, true);
        }
    }
}
