using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public interface IDataManager
    {
        UserLocalData CachedUserLocalData { get; set; }

        CardsLibraryData CachedCardsLibraryData { get; set; }

        HeroesData CachedHeroesData { get; set; }

        CollectionData CachedCollectionData { get; set; }

        DecksData CachedDecksData { get; set; }

        OpponentDecksData CachedOpponentDecksData { get; set; }

        CreditsData CachedCreditsData { get; set; }

        ConfigData ConfigData { get; set; }

        BetaConfig BetaConfig { get; set; }

        Task LoadRemoteConfig();

        Task StartLoadCache();

        Task SaveCache(Enumerators.CacheDataType type);

        TooltipContentData.CardTypeInfo GetCardTypeInfo(Enumerators.CardType cardType);

        TooltipContentData.BuffInfo GetCardBuffInfo(Enumerators.BuffType buffType);

        TooltipContentData.RankInfo GetCardRankInfo(Enumerators.CardRank rank);

        void DeleteData();

        string DecryptData(string data);

        string EncryptData(string data);
    }
}
