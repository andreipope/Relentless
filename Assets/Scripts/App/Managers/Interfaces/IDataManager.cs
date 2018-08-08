// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Common;
using System;

namespace LoomNetwork.CZB
{
    public interface IDataManager
    {
        event Action OnLoadCacheCompletedEvent;

        UserLocalData CachedUserLocalData { get; set; }
        CardsLibraryData CachedCardsLibraryData { get; set; }
        HeroesData CachedHeroesData { get; set; }
        CollectionData CachedCollectionData { get; set; }
        DecksData CachedDecksData { get; set; }
        OpponentDecksData CachedOpponentDecksData { get; set; }
        ActionData CachedActionsLibraryData { get; set; }
        CreditsData CachedCreditsData { get; set; }

        void StartLoadCache();
        void SaveAllCache();
        void SaveCache(Enumerators.CacheDataType type);

        Sprite GetSpriteFromTexture(Texture2D texture);

        BuffInfo GetBuffInfoByType(string type);
    }
}