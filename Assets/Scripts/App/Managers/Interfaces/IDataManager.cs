using UnityEngine;
using GrandDevs.CZB.Data;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public interface IDataManager
    {
        UserLocalData CachedUserLocalData { get; set; }
        CardsLibraryData CachedCardsLibraryData { get; set; }
        HeroesData CachedHeroesData { get; set; }
        CollectionData CachedCollectionData { get; set; }
        DecksData CachedDecksData { get; set; }
        OpponentDecksData CachedOpponentDecksData { get; set; }
        ActionData CachedActionsLibraryData { get; set; }

        void StartLoadCache();
        void SaveAllCache();
        void SaveCache(Enumerators.CacheDataType type);

        Sprite GetSpriteFromTexture(Texture2D texture);
    }
}