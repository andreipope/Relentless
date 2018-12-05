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

        AIDecksData CachedAiDecksData { get; set; }

        CreditsData CachedCreditsData { get; set; }

        ConfigData ConfigData { get; set; }

        BetaConfig BetaConfig { get; set; }

        Task StartLoadCache();

        Task SaveCache(Enumerators.CacheDataType type);

        TooltipContentData.CardTypeInfo GetCardTypeInfo(Enumerators.CardType cardType);

        TooltipContentData.GameMechanicInfo GetGameMechanicInfo(Enumerators.GameMechanicDescriptionType gameMechanic);

        TooltipContentData.RankInfo GetCardRankInfo(Enumerators.CardRank rank);

        void DeleteData();

        string DecryptData(string data);

        string EncryptData(string data);

        string SerializeToJson(object obj, bool indented = false);

        T DeserializeFromJson<T>(string json);
    }
}
