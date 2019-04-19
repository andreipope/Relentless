using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground
{
    public interface IDataManager
    {
        UserLocalData CachedUserLocalData { get; set; }

        CardsLibraryData CachedCardsLibraryData { get; set; }

        OverlordData CachedOverlordData { get; set; }

        CollectionData CachedCollectionData { get; set; }

        DecksData CachedDecksData { get; set; }

        AIDecksData CachedAiDecksData { get; set; }

        CreditsData CachedCreditsData { get; set; }

        ConfigData ConfigData { get; set; }

        UserInfo UserInfo { get; set; }

        GetVersionsResponse CachedVersions { get; set; }

        ZbVersion ZbVersion { get; }

        Task StartLoadCache();

        Task SaveCache(Enumerators.CacheDataType type);

        TooltipContentData.CardTypeInfo GetCardTypeInfo(Enumerators.CardType cardType);

        TooltipContentData.GameMechanicInfo GetGameMechanicInfo(Enumerators.GameMechanicDescription gameMechanic);

        TooltipContentData.RankInfo GetCardRankInfo(Enumerators.CardRank rank);

        string DecryptData(string data);

        string EncryptData(string data);

        string SerializeToJson(object obj, bool indented = false);

        T DeserializeFromJson<T>(string json);

        string GetPersistentDataPath(string fileName);

        Task LoadZbVersionData();
    }
}
