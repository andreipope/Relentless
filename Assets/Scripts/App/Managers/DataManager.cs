// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Loom.Newtonsoft.Json;
using LoomNetwork.CZB.BackendCommunication;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Protobuf;
using LoomNetwork.Internal;
using UnityEngine;
using Card = LoomNetwork.CZB.Data.Card;
using CardSet = LoomNetwork.CZB.Data.CardSet;
using Deck = LoomNetwork.CZB.Data.Deck;

namespace LoomNetwork.CZB
{
    public class DataManager : IService, IDataManager
    {
        public event Action OnLoadCacheCompletedEvent;

        private readonly DecksDataWithTimestamp _decksDataWithTimestamp = new DecksDataWithTimestamp();

        private IAppStateManager _appStateManager;

        private ILocalizationManager _localizationManager;

        private ILoadObjectsManager _loadObjectsManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private Dictionary<Enumerators.CacheDataType, string> _cacheDataPathes;

        private DirectoryInfo dir;

        public DataManager()
        {
            CachedUserLocalData = new UserLocalData();
            CachedCardsLibraryData = new CardsLibraryData();
            CachedHeroesData = new HeroesData();
            CachedCollectionData = new CollectionData();
            CachedDecksData = new DecksData();
            CachedOpponentDecksData = new OpponentDecksData();
            CachedActionsLibraryData = new ActionData();
            CachedCreditsData = new CreditsData();
            CachedBuffsTooltipData = new TooltipContentData();
        }

        public UserLocalData CachedUserLocalData { get; set; }

        public CardsLibraryData CachedCardsLibraryData { get; set; }

        public HeroesData CachedHeroesData { get; set; }

        public CollectionData CachedCollectionData { get; set; }

        public DecksData CachedDecksData { get; set; }

        public OpponentDecksData CachedOpponentDecksData { get; set; }

        public TooltipContentData CachedBuffsTooltipData { get; set; }

        public ActionData CachedActionsLibraryData { get; set; }

        public CreditsData CachedCreditsData { get; set; }

        public BetaConfig BetaConfig { get; set; }

        public int CurrentDeckInd { get; set; }

        public int CurrentAIDeckInd { get; }

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
            CachedOpponentDecksData.ParseData();
            CachedActionsLibraryData.ParseData();

            _localizationManager.ApplyLocalization();

#if DEV_MODE
            CachedUserLocalData.tutorial = false;
#endif

            GameClient.Get<IGameplayManager>().IsTutorial = CachedUserLocalData.tutorial;
            OnLoadCacheCompletedEvent?.Invoke();
        }

        public async Task SaveAllCache()
        {
            Debug.Log("== Saving all cache calledd === ");
            int count = Enum.GetNames(typeof(Enumerators.CacheDataType)).Length;
            for (int i = 0; i < count; i++)
            {
                await SaveCache((Enumerators.CacheDataType)i);
            }
        }

        public void DeleteData()
        {
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                if (file.Name.Contains("json") || file.Name.Contains("dat") || file.Name.Contains(Constants.VERSION_FILE_RESOLUTION))
                {
                    file.Delete();
                }
            }

            using (File.Create(dir + BuildMetaInfo.Instance.ShortVersionName + Constants.VERSION_FILE_RESOLUTION))
            {
            }

            PlayerPrefs.DeleteAll();
        }

        public Task SaveCache(Enumerators.CacheDataType type)
        {
            Debug.Log("== Saving cache type " + type);
            if (!File.Exists(_cacheDataPathes[type]))
            {
                File.Create(_cacheDataPathes[type]).Close();
            }

            switch (type)
            {
                case Enumerators.CacheDataType.USER_LOCAL_DATA:
                {
                    File.WriteAllText(_cacheDataPathes[type], SerializeObject(CachedUserLocalData));
                }

                    break;

                case Enumerators.CacheDataType.CARDS_LIBRARY_DATA:
                {
                    File.WriteAllText(_cacheDataPathes[type], SerializeObject(CachedCardsLibraryData));
                }

                    break;
                case Enumerators.CacheDataType.HEROES_DATA:
                {
                    File.WriteAllText(_cacheDataPathes[type], SerializeObject(CachedHeroesData));
                }

                    break;

                case Enumerators.CacheDataType.COLLECTION_DATA:
                {
                    File.WriteAllText(_cacheDataPathes[type], SerializeObject(CachedCollectionData));
                }

                    break;
                case Enumerators.CacheDataType.DECKS_DATA:
                {
                    _decksDataWithTimestamp.DecksData = CachedDecksData;
                    File.WriteAllText(_cacheDataPathes[type], SerializeObject(_decksDataWithTimestamp));
                }

                    break;
                case Enumerators.CacheDataType.DECKS_OPPONENT_DATA:
                {
                    File.WriteAllText(_cacheDataPathes[type], SerializeObject(CachedOpponentDecksData));
                }

                    break;
                case Enumerators.CacheDataType.OPPONENT_ACTIONS_LIBRARY_DATA:
                {
                    File.WriteAllText(_cacheDataPathes[type], SerializeObject(CachedActionsLibraryData));
                }

                    break;
                case Enumerators.CacheDataType.CREDITS_DATA:
                {
                    File.WriteAllText(_cacheDataPathes[type], SerializeObject(CachedCreditsData));
                }

                    break;
                case Enumerators.CacheDataType.BUFFS_TOOLTIP_DATA:
                {
                    File.WriteAllText(_cacheDataPathes[type], SerializeObject(CachedBuffsTooltipData));
                }

                    break;
            }

            return Task.CompletedTask;
        }

        public TooltipContentData.BuffInfo GetBuffInfoByType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return null;
            }

            return CachedBuffsTooltipData.buffs.Find(x => x.type.ToLower().Equals(type.ToLower()));
        }

        public TooltipContentData.RankInfo GetRankInfoByType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return null;
            }

            return CachedBuffsTooltipData.ranks.Find(x => x.type.ToLower().Equals(type.ToLower()));
        }

        public void Dispose()
        {
            // SaveAllCache();
        }

        public void Init()
        {
            _appStateManager = GameClient.Get<IAppStateManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            dir = new DirectoryInfo(Application.persistentDataPath + "/");

            CheckVersion();
            CheckFirstLaunch();
            FillCacheDataPathes();
        }

        public void Update()
        {
        }

        public uint GetMaxCopiesValue(Card card, string setName)
        {
            Enumerators.CardRank rank = card.cardRank;
            uint maxCopies = 0;

            if (setName.ToLower().Equals("item"))
            {
                maxCopies = Constants.CARD_ITEM_MAX_COPIES;
                return maxCopies;
            }

            switch (rank)
            {
                case Enumerators.CardRank.MINION:
                    maxCopies = Constants.CARD_MINION_MAX_COPIES;
                    break;
                case Enumerators.CardRank.OFFICER:
                    maxCopies = Constants.CARD_OFFICER_MAX_COPIES;
                    break;
                case Enumerators.CardRank.COMMANDER:
                    maxCopies = Constants.CARD_COMMANDER_MAX_COPIES;
                    break;
                case Enumerators.CardRank.GENERAL:
                    maxCopies = Constants.CARD_GENERAL_MAX_COPIES;
                    break;
            }

            return maxCopies;
        }

        private void CheckVersion()
        {
            FileInfo[] files = dir.GetFiles();
            bool versionMatch = false;
            foreach (FileInfo file in files)
            {
                if (file.Name == BuildMetaInfo.Instance.ShortVersionName + Constants.VERSION_FILE_RESOLUTION)
                {
                    versionMatch = true;
                }
            }

            if (!versionMatch)
            {
                DeleteData();
            }
        }

        private async Task LoadCachedData(Enumerators.CacheDataType type)
        {
            switch (type)
            {
                case Enumerators.CacheDataType.CARDS_LIBRARY_DATA:
                {
                    // if (File.Exists(_cacheDataPathes[type]))
                    // CachedCardsLibraryData = DeserializeObjectFromPath<CardsLibraryData>(_cacheDataPathes[type]);
                    try
                    {
                        ListCardLibraryResponse listCardLibraryResponse = await _backendFacade.GetCardLibrary();
                        Debug.Log(listCardLibraryResponse.ToString());
                        CachedCardsLibraryData = listCardLibraryResponse.FromProtobuf();
                    } catch (Exception ex)
                    {
                        Debug.LogError("===== Card Library Not Loaded, loading from cache ===== " + ex);
                        if (File.Exists(_cacheDataPathes[type]))
                        {
                            CachedCardsLibraryData = DeserializeObjectFromPath<CardsLibraryData>(_cacheDataPathes[type]);
                        }
                    }
                }

                    break;
                case Enumerators.CacheDataType.HEROES_DATA:
                {
                    // if (File.Exists(_cacheDataPathes[type]))
                    // CachedHeroesData = DeserializeObjectFromPath<HeroesData>(_cacheDataPathes[type]);
                    try
                    {
                        ListHeroesResponse heroesList = await _backendFacade.GetHeroesList(_backendDataControlMediator.UserDataModel.UserId);
                        Debug.Log(heroesList.ToString());
                        CachedHeroesData = JsonConvert.DeserializeObject<HeroesData>(heroesList.ToString());
                    } catch (Exception ex)
                    {
                        Debug.LogError("===== Heroes List not Loaded, loading from cache ===== " + ex);
                        if (File.Exists(_cacheDataPathes[type]))
                        {
                            CachedHeroesData = DeserializeObjectFromPath<HeroesData>(_cacheDataPathes[type]);
                        }
                    }
                }

                    break;
                case Enumerators.CacheDataType.USER_LOCAL_DATA:
                {
                    if (File.Exists(_cacheDataPathes[type]))
                    {
                        CachedUserLocalData = DeserializeObjectFromPath<UserLocalData>(_cacheDataPathes[type]);
                    }
                }

                    break;
                case Enumerators.CacheDataType.COLLECTION_DATA:
                {
                    // if (File.Exists(_cacheDataPathes[type]))
                    // CachedCollectionData = DeserializeObjectFromPath<CollectionData>(_cacheDataPathes[type]);
                    try
                    {
                        GetCollectionResponse getCollectionResponse = await _backendFacade.GetCardCollection(_backendDataControlMediator.UserDataModel.UserId);
                        Debug.Log(getCollectionResponse.ToString());
                        CachedCollectionData = getCollectionResponse.FromProtobuf();
                    } catch (Exception ex)
                    {
                        Debug.LogError("===== Card Collection Not Loaded, loading from cache ===== " + ex);
                        if (File.Exists(_cacheDataPathes[type]))
                        {
                            CachedCollectionData = DeserializeObjectFromPath<CollectionData>(_cacheDataPathes[type]);
                        }
                    }
                }

                    break;
                case Enumerators.CacheDataType.DECKS_DATA:
                {
                    // if (File.Exists(_cacheDataPathes[type]))
                    // CachedDecksData = DeserializeObjectFromPath<DecksData>(_cacheDataPathes[type]);

                    // TODO: add code to sync local and remote decks
                    DecksData localDecksData = null, remoteDecksData = null;
                    long localDecksDataTimestamp = 0, remoteDecksDataTimestamp = 0;
                    if (File.Exists(_cacheDataPathes[type]))
                    {
                        DecksDataWithTimestamp localDecksDataWithTimestamp = DeserializeObjectFromPath<DecksDataWithTimestamp>(_cacheDataPathes[type]);
                        localDecksData = localDecksDataWithTimestamp.DecksData;
                        localDecksDataTimestamp = localDecksDataWithTimestamp.LastModificationTimestamp;
                    } else
                    {
                        localDecksData = CachedDecksData;
                    }

                    try
                    {
                        ListDecksResponse listDecksResponse = await _backendFacade.GetDecks(_backendDataControlMediator.UserDataModel.UserId);
                        if (listDecksResponse != null)
                        {
                            Debug.Log(listDecksResponse.ToString());

                            // remoteDecksData = JsonConvert.DeserializeObject<DecksData>(listDecksResponse.Decks.ToString());
                            remoteDecksData = new DecksData { decks = listDecksResponse.Decks.Select(d => JsonConvert.DeserializeObject<Deck>(d.ToString())).ToList() };
                            remoteDecksDataTimestamp = listDecksResponse.LastModificationTimestamp;
                        } else
                        {
                            Debug.Log(" List Deck Response is Null == ");
                        }
                    } catch (Exception ex)
                    {
                        Debug.LogError("===== Deck Data Not Loaded from Backend ===== " + ex);
                    }

                    if ((localDecksData != null) && (remoteDecksData != null))
                    {
                        if (remoteDecksDataTimestamp == localDecksDataTimestamp)
                        {
                            Debug.Log("Remote decks timestamp == local decks timestamp, no sync needed");
                            CachedDecksData = remoteDecksData;
                        } else if (remoteDecksDataTimestamp > localDecksDataTimestamp)
                        {
                            Debug.Log("Remote decks data is newer than local, using remote data");
                            CachedDecksData = remoteDecksData;
                        } else
                        {
                            Debug.Log("Local decks data is newer than remote, synchronizing remote state with local");
                            try
                            {
                                // Remove all remote decks, fingers crossed
                                foreach (Deck remoteDeck in remoteDecksData.decks)
                                {
                                    await _backendFacade.DeleteDeck(_backendDataControlMediator.UserDataModel.UserId, remoteDeck.id, 0);
                                }

                                // Upload local decks
                                foreach (Deck localDeck in localDecksData.decks)
                                {
                                    long createdDeckId = await _backendFacade.AddDeck(_backendDataControlMediator.UserDataModel.UserId, localDeck, localDecksDataTimestamp);
                                    localDeck.id = createdDeckId;
                                }

                                CachedDecksData = localDecksData;
                            } catch (Exception e)
                            {
                                CachedDecksData = localDecksData;
                                Debug.LogError("Catastrophy! Error while synchronizing decks, assuming local deck as a fallback");
                                Debug.LogException(e);
                                throw;
                            }
                        }
                    } else if (remoteDecksData != null)
                    {
                        Debug.Log("Using remote decks data");
                        CachedDecksData = remoteDecksData;
                    } else if (localDecksData != null)
                    {
                        Debug.Log("Using local decks data");
                        CachedDecksData = localDecksData;
                    }
                }

                    break;
                case Enumerators.CacheDataType.DECKS_OPPONENT_DATA:
                {
                    if (File.Exists(_cacheDataPathes[type]))
                    {
                        CachedOpponentDecksData = DeserializeObjectFromPath<OpponentDecksData>(_cacheDataPathes[type]);
                    }
                }

                    break;
                case Enumerators.CacheDataType.OPPONENT_ACTIONS_LIBRARY_DATA:
                {
                    if (File.Exists(_cacheDataPathes[type]))
                    {
                        CachedActionsLibraryData = DeserializeObjectFromPath<ActionData>(_cacheDataPathes[type]);
                    }
                }

                    break;
                case Enumerators.CacheDataType.CREDITS_DATA:
                {
                    if (File.Exists(_cacheDataPathes[type]))
                    {
                        CachedCreditsData = DeserializeObjectFromPath<CreditsData>(_cacheDataPathes[type]);
                    }
                }

                    break;
                case Enumerators.CacheDataType.BUFFS_TOOLTIP_DATA:
                {
                    if (File.Exists(_cacheDataPathes[type]))
                    {
                        CachedBuffsTooltipData = DeserializeObjectFromPath<TooltipContentData>(_cacheDataPathes[type]);
                    }
                }

                    break;
            }
        }

        private void CheckFirstLaunch()
        {
            if (!File.Exists(Path.Combine(Application.persistentDataPath, Constants.LOCAL_CARDS_LIBRARY_DATA_FILE_PATH)))
            {
                CachedCardsLibraryData = JsonConvert.DeserializeObject<CardsLibraryData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/card_library_data").text);
                CachedHeroesData = JsonConvert.DeserializeObject<HeroesData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/heroes_data").text);

                // CachedCollectionData = JsonConvert.DeserializeObject<CollectionData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/collection_data").text);
                CachedDecksData = JsonConvert.DeserializeObject<DecksData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/decks_data").text);
                CachedOpponentDecksData = JsonConvert.DeserializeObject<OpponentDecksData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/opponent_decks_data").text);
                CachedActionsLibraryData = JsonConvert.DeserializeObject<ActionData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/action_data").text);
                CachedCreditsData = JsonConvert.DeserializeObject<CreditsData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/credits_data").text);
                CachedBuffsTooltipData = JsonConvert.DeserializeObject<TooltipContentData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/buffs_tooltip_data").text);

                /*var collectionLibrary = _loadObjectsManager.GetObjectByPath<TextAsset>("Data/collection_data");
                if (collectionLibrary == null)
                    FillFullCollection();
                else
                    CachedCollectionData = JsonConvert.DeserializeObject<CollectionData>(collectionLibrary.text);*/
            }
        }

        private void FillCacheDataPathes()
        {
            _cacheDataPathes = new Dictionary<Enumerators.CacheDataType, string>();
            _cacheDataPathes.Add(Enumerators.CacheDataType.USER_LOCAL_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_USER_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.CARDS_LIBRARY_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_CARDS_LIBRARY_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.HEROES_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_HEROES_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.COLLECTION_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_COLLECTION_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.DECKS_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_DECKS_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.DECKS_OPPONENT_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_OPPONENT_DECKS_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.OPPONENT_ACTIONS_LIBRARY_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_OPPONENT_ACTIONS_LIBRARY_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.CREDITS_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_CREDITS_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.BUFFS_TOOLTIP_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_BUFFS_TOOLTIP_DATA_FILE_PATH));
        }

        private T DeserializeObjectFromPath<T>(string path)
        {
            if (Constants.DATA_ENCRYPTION_ENABLED)
            {
                return JsonConvert.DeserializeObject<T>(Utilites.Decrypt(File.ReadAllText(path), Constants.PRIVATE_ENCRYPTION_KEY_FOR_APP));
            }

            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }

        private string SerializeObject(object obj)
        {
            if (Constants.DATA_ENCRYPTION_ENABLED)
            {
                return Utilites.Encrypt(JsonConvert.SerializeObject(obj, Formatting.Indented), Constants.PRIVATE_ENCRYPTION_KEY_FOR_APP);
            }

            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        private void FillFullCollection()
        {
            CachedCollectionData = new CollectionData();
            CachedCollectionData.cards = new List<CollectionCardData>();

            foreach (CardSet set in CachedCardsLibraryData.sets)
            {
                foreach (Card card in set.cards)
                {
                    CachedCollectionData.cards.Add(new CollectionCardData { amount = (int)GetMaxCopiesValue(card, set.name), cardName = card.name });
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
