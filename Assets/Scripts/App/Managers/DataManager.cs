#if UNITY_EDITOR
#define DISABLE_DATA_ENCRYPTION
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;

using Newtonsoft.Json;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DataManager : IService, IDataManager
    {
        private readonly DecksDataWithTimestamp _decksDataWithTimestamp = new DecksDataWithTimestamp();

        private ILocalizationManager _localizationManager;

        private ILoadObjectsManager _loadObjectsManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private Dictionary<Enumerators.CacheDataType, string> _cacheDataPaths;

        private DirectoryInfo _dir;

        public DataManager(ConfigData configData)
        {
            FillCacheDataPaths();
            InitCachedData();
            ConfigData = configData;
        }

        private void InitCachedData()
        {
            CachedUserLocalData = new UserLocalData();
            CachedCardsLibraryData = new CardsLibraryData();
            CachedHeroesData = new HeroesData();
            CachedCollectionData = new CollectionData();
            CachedDecksData = new DecksData();
            CachedOpponentDecksData = new OpponentDecksData();
            CachedCreditsData = new CreditsData();
            CachedBuffsTooltipData = new TooltipContentData();
        }

        public TooltipContentData CachedBuffsTooltipData { get; set; }

        public UserLocalData CachedUserLocalData { get; set; }

        public CardsLibraryData CachedCardsLibraryData { get; set; }

        public HeroesData CachedHeroesData { get; set; }

        public CollectionData CachedCollectionData { get; set; }

        public DecksData CachedDecksData { get; set; }

        public OpponentDecksData CachedOpponentDecksData { get; set; }

        public CreditsData CachedCreditsData { get; set; }

        public ConfigData ConfigData { get; set; }

        public BetaConfig BetaConfig { get; set; }

        public long CachedDecksLastModificationTimestamp
        {
            get => _decksDataWithTimestamp.LastModificationTimestamp;
            set => _decksDataWithTimestamp.LastModificationTimestamp = value;
        }

        public async Task LoadRemoteConfig()
        {
            BetaConfig = await _backendFacade.GetBetaConfig(_backendDataControlMediator.UserDataModel.BetaKey);
            if (BetaConfig == null)
                throw new Exception("BetaConfig == null");
        }

        public async Task StartLoadCache()
        {
            Debug.Log("=== Start loading server ==== ");

            int count = Enum.GetNames(typeof(Enumerators.CacheDataType)).Length;
            for (int i = 0; i < count; i++)
            {
                await LoadCachedData((Enumerators.CacheDataType)i);
            }

            CachedCardsLibraryData.FillAllCards();

            // FIXME: remove next line after fetching collection from backend is implemented
            FillFullCollection();

            _localizationManager.ApplyLocalization();

#if DEV_MODE
            CachedUserLocalData.Tutorial = false;
#endif

            GameClient.Get<ISoundManager>().ApplySoundData();
            GameClient.Get<IApplicationSettingsManager>().ApplySettings();

            GameClient.Get<IGameplayManager>().IsTutorial = CachedUserLocalData.Tutorial;

            for (int i = 0; i < count; i++)
            {
                await SaveCache((Enumerators.CacheDataType)i);
            }
        }

        public void DeleteData()
        {
            InitCachedData();
            FileInfo[] files = _dir.GetFiles();

            foreach (FileInfo file in files)
            {
                if (_cacheDataPaths.Values.Any(path => path.EndsWith(file.Name)) ||
                    file.Extension.Equals("dat", StringComparison.InvariantCultureIgnoreCase) ||
                    file.Name.Contains(Constants.VersionFileResolution))
                {
                    file.Delete();
                }
            }

            using (File.Create(_dir + BuildMetaInfo.Instance.ShortVersionName + Constants.VersionFileResolution))
            {
            }

            PlayerPrefs.DeleteAll();
        }

        public Task SaveCache(Enumerators.CacheDataType type)
        {
            Debug.Log("== Saving cache type " + type);
            if (!File.Exists(_cacheDataPaths[type])) File.Create(_cacheDataPaths[type]).Close();

            switch (type)
            {
                case Enumerators.CacheDataType.USER_LOCAL_DATA:
                    File.WriteAllText(_cacheDataPaths[type], SerializeObject(CachedUserLocalData));
                    break;
                case Enumerators.CacheDataType.CARDS_LIBRARY_DATA:
                    File.WriteAllText(_cacheDataPaths[type], SerializeObject(CachedCardsLibraryData));
                    break;
                case Enumerators.CacheDataType.HEROES_DATA:
                    File.WriteAllText(_cacheDataPaths[type], SerializeObject(CachedHeroesData));
                    break;
                case Enumerators.CacheDataType.COLLECTION_DATA:
                    File.WriteAllText(_cacheDataPaths[type], SerializeObject(CachedCollectionData));
                    break;
                case Enumerators.CacheDataType.DECKS_DATA:
                    _decksDataWithTimestamp.DecksData = CachedDecksData;
                    File.WriteAllText(_cacheDataPaths[type], SerializeObject(_decksDataWithTimestamp));
                    break;
                case Enumerators.CacheDataType.DECKS_OPPONENT_DATA:
                    File.WriteAllText(_cacheDataPaths[type], SerializeObject(CachedOpponentDecksData));
                    break;
                case Enumerators.CacheDataType.CREDITS_DATA:
                    File.WriteAllText(_cacheDataPaths[type], SerializeObject(CachedCreditsData));
                    break;
                case Enumerators.CacheDataType.BUFFS_TOOLTIP_DATA:
                    File.WriteAllText(_cacheDataPaths[type], SerializeObject(CachedBuffsTooltipData));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }

        public TooltipContentData.BuffInfo GetBuffInfoByType(string type)
        {
            if (string.IsNullOrEmpty(type))
                return null;

            return CachedBuffsTooltipData.Buffs.Find(x => x.Type.ToLower().Equals(type.ToLower()));
        }

        public TooltipContentData.RankInfo GetRankInfoByType(string type)
        {
            if (string.IsNullOrEmpty(type))
                return null;

            return CachedBuffsTooltipData.Ranks.Find(x => x.Type.ToLower().Equals(type.ToLower()));
        }

        public void Dispose()
        {
        }

        public void Init()
        {
            Debug.Log("Encryption:" + ConfigData.EncryptData);
            Debug.Log("Skip Card Data Backend:" + ConfigData.SkipBackendCardData);

            _localizationManager = GameClient.Get<ILocalizationManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _dir = new DirectoryInfo(Application.persistentDataPath + "/");

            CheckVersion();
            CheckFirstLaunch();
        }

        public void Update()
        {
        }

        private uint GetMaxCopiesValue(Data.Card card, string setName)
        {
            Enumerators.CardRank rank = card.CardRank;
            uint maxCopies;

            if (setName.ToLower().Equals("item"))
            {
                maxCopies = Constants.CardItemMaxCopies;
                return maxCopies;
            }

            switch (rank)
            {
                case Enumerators.CardRank.MINION:
                    maxCopies = Constants.CardMinionMaxCopies;
                    break;
                case Enumerators.CardRank.OFFICER:
                    maxCopies = Constants.CardOfficerMaxCopies;
                    break;
                case Enumerators.CardRank.COMMANDER:
                    maxCopies = Constants.CardCommanderMaxCopies;
                    break;
                case Enumerators.CardRank.GENERAL:
                    maxCopies = Constants.CardGeneralMaxCopies;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return maxCopies;
        }

        private void CheckVersion()
        {
            FileInfo[] files = _dir.GetFiles();
            bool versionMatch = false;
            foreach (FileInfo file in files)
            {
                if (file.Name == BuildMetaInfo.Instance.ShortVersionName + Constants.VersionFileResolution)
                    versionMatch = true;
            }

            if (!versionMatch)
                DeleteData();
        }

        private async Task LoadCachedData(Enumerators.CacheDataType type)
        {
            switch (type)
            {
                case Enumerators.CacheDataType.CARDS_LIBRARY_DATA:
                    try
                    {
                        if (ConfigData.SkipBackendCardData) {
                            throw new Exception("Config Set to Skip Backend Call");
                        }
                        ListCardLibraryResponse listCardLibraryResponse = await _backendFacade.GetCardLibrary();
                        Debug.Log(listCardLibraryResponse.ToString());
                        CachedCardsLibraryData = listCardLibraryResponse.FromProtobuf();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("===== Card Library Not Loaded, loading from cache ===== " + ex);
                        if (File.Exists(_cacheDataPaths[type]))
                            CachedCardsLibraryData =
                                DeserializeObjectFromPath<CardsLibraryData>(_cacheDataPaths[type]);
                    }
                    break;
                case Enumerators.CacheDataType.HEROES_DATA:
                    try
                    {
                        ListHeroesResponse heroesList =
                            await _backendFacade.GetHeroesList(_backendDataControlMediator.UserDataModel.UserId);
                        Debug.Log(heroesList.ToString());
                        CachedHeroesData = JsonConvert.DeserializeObject<HeroesData>(heroesList.ToString());
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("===== Heroes List not Loaded, loading from cache ===== " + ex);
                        if (File.Exists(_cacheDataPaths[type]))
                            CachedHeroesData = DeserializeObjectFromPath<HeroesData>(_cacheDataPaths[type]);
                    }

                    break;
                case Enumerators.CacheDataType.USER_LOCAL_DATA:
                    if (File.Exists(_cacheDataPaths[type]))
                        CachedUserLocalData = DeserializeObjectFromPath<UserLocalData>(_cacheDataPaths[type]);

                    break;
                case Enumerators.CacheDataType.COLLECTION_DATA:
                    try
                    {
                        GetCollectionResponse getCollectionResponse =
                            await _backendFacade.GetCardCollection(_backendDataControlMediator.UserDataModel.UserId);
                        Debug.Log(getCollectionResponse.ToString());
                        CachedCollectionData = getCollectionResponse.FromProtobuf();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("===== Card Collection Not Loaded, loading from cache ===== " + ex);
                        if (File.Exists(_cacheDataPaths[type]))
                            CachedCollectionData = DeserializeObjectFromPath<CollectionData>(_cacheDataPaths[type]);
                    }

                    break;
                case Enumerators.CacheDataType.DECKS_DATA:
                    DecksData localDecksData, remoteDecksData = null;
                    long localDecksDataTimestamp = 0, remoteDecksDataTimestamp = 0;
                    if (File.Exists(_cacheDataPaths[type]))
                    {
                        DecksDataWithTimestamp localDecksDataWithTimestamp =
                            DeserializeObjectFromPath<DecksDataWithTimestamp>(_cacheDataPaths[type]);
                        localDecksData = localDecksDataWithTimestamp.DecksData;
                        localDecksDataTimestamp = localDecksDataWithTimestamp.LastModificationTimestamp;
                    }
                    else
                    {
                        localDecksData = CachedDecksData;
                    }

                    try
                    {
                        ListDecksResponse listDecksResponse =
                            await _backendFacade.GetDecks(_backendDataControlMediator.UserDataModel.UserId);
                        if (listDecksResponse != null)
                        {
                            Debug.Log(listDecksResponse.ToString());

                            remoteDecksData = new DecksData
                            {
                                Decks = listDecksResponse.Decks
                                    .Select(d => JsonConvert.DeserializeObject<Data.Deck>(d.ToString())).ToList()
                            };
                            remoteDecksDataTimestamp = listDecksResponse.LastModificationTimestamp;
                        }
                        else
                        {
                            Debug.Log(" List Deck Response is Null == ");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("===== Deck Data Not Loaded from Backend ===== " + ex);
                    }

                    if (localDecksData != null && remoteDecksData != null)
                    {
                        if (remoteDecksDataTimestamp == localDecksDataTimestamp)
                        {
                            Debug.Log("Remote decks timestamp == local decks timestamp, no sync needed");
                            CachedDecksData = remoteDecksData;
                        }
                        else if (remoteDecksDataTimestamp > localDecksDataTimestamp)
                        {
                            Debug.Log("Remote decks data is newer than local, using remote data");
                            CachedDecksData = remoteDecksData;
                        }
                        else
                        {
                            Debug.Log("Local decks data is newer than remote, synchronizing remote state with local");
                            try
                            {
                                // Remove all remote decks, fingers crossed
                                foreach (Data.Deck remoteDeck in remoteDecksData.Decks)
                                {
                                    await _backendFacade.DeleteDeck(_backendDataControlMediator.UserDataModel.UserId,
                                        remoteDeck.Id, 0);
                                }

                                // Upload local decks
                                foreach (Data.Deck localDeck in localDecksData.Decks)
                                {
                                    long createdDeckId = await _backendFacade.AddDeck(
                                        _backendDataControlMediator.UserDataModel.UserId, localDeck,
                                        localDecksDataTimestamp);
                                    localDeck.Id = createdDeckId;
                                }

                                CachedDecksData = localDecksData;
                            }
                            catch (Exception e)
                            {
                                CachedDecksData = localDecksData;
                                Debug.LogError(
                                    "Catastrophe! Error while synchronizing decks, assuming local deck as a fallback");
                                Debug.LogException(e);
                                throw;
                            }
                        }
                    }
                    else if (remoteDecksData != null)
                    {
                        Debug.Log("Using remote decks data");
                        CachedDecksData = remoteDecksData;
                    }
                    else if (localDecksData != null)
                    {
                        Debug.Log("Using local decks data");
                        CachedDecksData = localDecksData;
                    }

                    break;
                case Enumerators.CacheDataType.DECKS_OPPONENT_DATA:
                    if (File.Exists(_cacheDataPaths[type]))
                        CachedOpponentDecksData = DeserializeObjectFromPath<OpponentDecksData>(_cacheDataPaths[type]);

                    break;
                case Enumerators.CacheDataType.CREDITS_DATA:
                    if (File.Exists(_cacheDataPaths[type]))
                        CachedCreditsData = DeserializeObjectFromPath<CreditsData>(_cacheDataPaths[type]);

                    break;
                case Enumerators.CacheDataType.BUFFS_TOOLTIP_DATA:
                    if (File.Exists(_cacheDataPaths[type]))
                        CachedBuffsTooltipData = DeserializeObjectFromPath<TooltipContentData>(_cacheDataPaths[type]);
                    break;
            }
        }

        private void CheckFirstLaunch()
        {
            if (!File.Exists(Path.Combine(Application.persistentDataPath, Constants.LocalCardsLibraryDataFilePath)))
            {
                CachedCardsLibraryData =
                    JsonConvert.DeserializeObject<CardsLibraryData>(_loadObjectsManager
                        .GetObjectByPath<TextAsset>("Data/card_library_data").text);
                CachedHeroesData =
                    JsonConvert.DeserializeObject<HeroesData>(_loadObjectsManager
                        .GetObjectByPath<TextAsset>("Data/heroes_data").text);
                CachedDecksData =
                    JsonConvert.DeserializeObject<DecksData>(_loadObjectsManager
                        .GetObjectByPath<TextAsset>("Data/decks_data").text);
                CachedOpponentDecksData = JsonConvert.DeserializeObject<OpponentDecksData>(_loadObjectsManager
                    .GetObjectByPath<TextAsset>("Data/opponent_decks_data").text);
                CachedCreditsData =
                    JsonConvert.DeserializeObject<CreditsData>(_loadObjectsManager
                        .GetObjectByPath<TextAsset>("Data/credits_data").text);
                CachedBuffsTooltipData =
                    JsonConvert.DeserializeObject<TooltipContentData>(_loadObjectsManager
                        .GetObjectByPath<TextAsset>("Data/buffs_tooltip_data").text);
            }
        }

        private void FillCacheDataPaths()
        {
            _cacheDataPaths = new Dictionary<Enumerators.CacheDataType, string>
            {
                {
                    Enumerators.CacheDataType.USER_LOCAL_DATA,
                    Path.Combine(Application.persistentDataPath, Constants.LocalUserDataFilePath)
                },
                {
                    Enumerators.CacheDataType.CARDS_LIBRARY_DATA,
                    Path.Combine(Application.persistentDataPath, Constants.LocalCardsLibraryDataFilePath)
                },
                {
                    Enumerators.CacheDataType.HEROES_DATA,
                    Path.Combine(Application.persistentDataPath, Constants.LocalHeroesDataFilePath)
                },
                {
                    Enumerators.CacheDataType.COLLECTION_DATA,
                    Path.Combine(Application.persistentDataPath, Constants.LocalCollectionDataFilePath)
                },
                {
                    Enumerators.CacheDataType.DECKS_DATA,
                    Path.Combine(Application.persistentDataPath, Constants.LocalDecksDataFilePath)
                },
                {
                    Enumerators.CacheDataType.DECKS_OPPONENT_DATA,
                    Path.Combine(Application.persistentDataPath, Constants.LocalOpponentDecksDataFilePath)
                },
                {
                    Enumerators.CacheDataType.CREDITS_DATA,
                    Path.Combine(Application.persistentDataPath, Constants.LocalCreditsDataFilePath)
                },
                {
                    Enumerators.CacheDataType.BUFFS_TOOLTIP_DATA,
                    Path.Combine(Application.persistentDataPath, Constants.LocalBuffsTooltipDataFilePath)
                }
            };
        }

        public string DecryptData(string data)
        {
            if (!ConfigData.EncryptData)
                return data;

            return Utilites.Decrypt(data, Constants.PrivateEncryptionKeyForApp);
        }

        public string EncryptData(string data)
        {
            if (!ConfigData.EncryptData)
                return data;

            return Utilites.Encrypt(data, Constants.PrivateEncryptionKeyForApp);
        }

        private T DeserializeObjectFromPath<T>(string path)
        {
            return JsonConvert.DeserializeObject<T>(DecryptData(File.ReadAllText(path)));
        }

        private string SerializeObject(object obj)
        {
            string data = JsonConvert.SerializeObject(obj, Formatting.Indented);
            return EncryptData(data);
        }

        private void FillFullCollection()
        {
            CachedCollectionData = new CollectionData();
            CachedCollectionData.Cards = new List<CollectionCardData>();

            foreach (Data.CardSet set in CachedCardsLibraryData.Sets)
            {
                foreach (Data.Card card in set.Cards)
                {
                    CachedCollectionData.Cards.Add(
                        new CollectionCardData
                        {
                            Amount = (int) GetMaxCopiesValue(card, set.Name),
                            CardName = card.Name
                        });
                }
            }
        }

        private class DecksDataWithTimestamp
        {
            public long LastModificationTimestamp;

            public DecksData DecksData;
        }
    }
}
