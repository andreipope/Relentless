// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Common;
using System;
using System.Threading.Tasks;

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

        Task StartLoadCache();
        Task SaveAllCache();
        Task SaveCache(Enumerators.CacheDataType type);

        TooltipContentData.BuffInfo GetBuffInfoByType(string type);
        TooltipContentData.RankInfo GetRankInfoByType(string type);

		void DeleteData();
    }
}