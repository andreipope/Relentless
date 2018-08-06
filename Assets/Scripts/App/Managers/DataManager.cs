// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using LoomNetwork.Internal;
using LoomNetwork.CZB.Data;
using Deck = LoomNetwork.CZB.Data.Deck;

namespace LoomNetwork.CZB
{
    public class DataManager : IService, IDataManager
    {
        private IAppStateManager _appStateManager;
        private ILocalizationManager _localizationManager;
        private ILoadObjectsManager _loadObjectsManager;


        private Dictionary<Enumerators.CacheDataType, string> _cacheDataPathes;

        public event Action OnLoadCacheCompletedEvent;

        public UserLocalData CachedUserLocalData { get; set; }
		public CardsLibraryData CachedCardsLibraryData { get; set; }
        public HeroesData CachedHeroesData { get; set; }
        public CollectionData CachedCollectionData { get; set; }
        public DecksData CachedDecksData { get; set; }
        public OpponentDecksData CachedOpponentDecksData { get; set; }

        public ActionData CachedActionsLibraryData { get; set;}

        public CreditsData CachedCreditsData { get; set; }

        private int _currentDeckIndex;
		private int _currentAIDeckIndex;

        private DirectoryInfo dir;

        public int CurrentDeckInd
		{
			get { return _currentDeckIndex; }
            set { _currentDeckIndex = value; }
		}

		public int CurrentAIDeckInd
		{
			get { return _currentAIDeckIndex; }
		}

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
        }

        public void Dispose()
        {
            SaveAllCache();
        }

        public void Init()
        {
            _appStateManager = GameClient.Get<IAppStateManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            dir = new DirectoryInfo(Application.persistentDataPath + "/");

            CheckVersion();
            CheckFirstLaunch();
            FillCacheDataPathes();
        }

        public async void StartLoadCache()
        {
            // TODO : Remove creating guest user from here
            await LoomManager.Instance.SetUser();
            
            
            Debug.Log("=== Start loading server ==== ");
            int count = Enum.GetNames(typeof(Enumerators.CacheDataType)).Length;
            for (int i = 0; i < count; i++)
                LoadCachedData((Enumerators.CacheDataType)i);

            
            //await GetCardLibraryData();
            await GetDeckData();
            //await GetCollectionData();
            await GetHeroesData();
            
            CachedCardsLibraryData.FillAllCards();

            CachedOpponentDecksData.ParseData();
            CachedActionsLibraryData.ParseData();

            _localizationManager.ApplyLocalization();

            if (Constants.DEV_MODE)
                CachedUserLocalData.tutorial = false;

            GameClient.Get<IGameplayManager>().IsTutorial = CachedUserLocalData.tutorial;


            OnLoadCacheCompletedEvent?.Invoke();
        }

        private async Task GetHeroesData()
        {
            try
            {
                var heroesList = await LoomManager.Instance.GetHeroesList(LoomManager.UserId);
                CustomDebug.Log(heroesList.ToString());
                CachedHeroesData = JsonConvert.DeserializeObject<HeroesData>(heroesList.ToString());
            }
            catch (Exception ex)
            {
                CustomDebug.LogError("===== Heroes List not Loaded ===== " + ex);
            }
        }

        private async Task GetCollectionData()
        {
            try
            {
                var cardCollection = await LoomManager.Instance.GetCardCollection(LoomManager.UserId);
                CustomDebug.Log(cardCollection.ToString());
                CachedCollectionData = JsonConvert.DeserializeObject<CollectionData>(cardCollection.ToString());
            }
            catch (Exception ex)
            {
                CustomDebug.LogError("===== Card Collection Not Loaded ===== " + ex);
            }
        }

        private async Task GetCardLibraryData()
        {
            try
            {
                var cardLibrary = await LoomManager.Instance.GetCardLibrary();
                CustomDebug.Log(cardLibrary.ToString());
                CachedCardsLibraryData = JsonConvert.DeserializeObject<CardsLibraryData>(cardLibrary.ToString());
            }
            catch (Exception ex)
            {
                CustomDebug.LogError("===== Card Library Not Loaded ===== " + ex);
            }
        }
        
        private async Task GetDeckData()
        {
            try
            {
                var listDecksResponse = await LoomManager.Instance.GetDecks(LoomManager.UserId);
                if (listDecksResponse != null)
                {
                    CustomDebug.Log(listDecksResponse.ToString());
                    CachedDecksData = JsonConvert.DeserializeObject<DecksData>(listDecksResponse.ToString());
                }
                else
                    CustomDebug.Log(" List Deck Response is Null == ");
            }
            catch (Exception ex)
            {
                CustomDebug.LogError("===== Deck Data Not Loaded from Backed ===== " + ex + " == Load from Resources ==");
                // TODO : Removed code loading deck data from Resources
                //CachedDecksData = JsonConvert.DeserializeObject<DecksData>(Resources.Load("Data/decks_data").ToString());
            }
        }

        public void Update()
        {

        }

        public void SaveAllCache()
        {
            
            Debug.Log("== Saving all cache calledd === ");
            int count = Enum.GetNames(typeof(Enumerators.CacheDataType)).Length;
            for (int i = 0; i < count; i++)
                SaveCache((Enumerators.CacheDataType)i);
        }

        private void CheckVersion()
        {
            var files = dir.GetFiles();
            bool versionMatch = false;
            foreach (var file in files)
                if (file.Name == Constants.CURRENT_VERSION)
                    versionMatch = true;

            if (!versionMatch)
            {
                foreach (var file in files)
                    if (file.Name.Contains("json") || file.Name.Contains("dat") || file.Name.Contains("ver"))
                        file.Delete();
                File.Create(dir + Constants.CURRENT_VERSION);
            }
        }

        public void SaveCache(Enumerators.CacheDataType type)
        {
            if (!File.Exists(_cacheDataPathes[type]))
                File.Create(_cacheDataPathes[type]).Close();
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
                        File.WriteAllText(_cacheDataPathes[type], SerializeObject(CachedDecksData));
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
                case Enumerators.CacheDataType. CREDITS_DATA:
                    {
                        File.WriteAllText(_cacheDataPathes[type], SerializeObject(CachedCreditsData));
                    }
                    break;
                default: break;
            }
        }


        public Sprite GetSpriteFromTexture(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2f);
        }

        private void LoadCachedData(Enumerators.CacheDataType type)
        {
            switch (type)
            {
                case Enumerators.CacheDataType.CARDS_LIBRARY_DATA:
                    {
                        if (File.Exists(_cacheDataPathes[type]))
                            CachedCardsLibraryData = DeserializeObjectFromPath<CardsLibraryData>(_cacheDataPathes[type]);
                    }
                    break;
                /*case Enumerators.CacheDataType.HEROES_DATA:
					{
						if (File.Exists(_cacheDataPathes[type]))
                            CachedHeroesData = DeserializeObjectFromPath<HeroesData>(_cacheDataPathes[type]);
					}
					break;
					*/
                case Enumerators.CacheDataType.USER_LOCAL_DATA:
                    {
                        if (File.Exists(_cacheDataPathes[type]))
                            CachedUserLocalData = DeserializeObjectFromPath<UserLocalData>(_cacheDataPathes[type]);
                    }
                    break;
                case Enumerators.CacheDataType.COLLECTION_DATA:
                    {
                        if (File.Exists(_cacheDataPathes[type]))
                            CachedCollectionData = DeserializeObjectFromPath<CollectionData>(_cacheDataPathes[type]);
                    }
                    break;
                /*case Enumerators.CacheDataType.DECKS_DATA:
                    {
                        if (File.Exists(_cacheDataPathes[type]))
                            CachedDecksData = DeserializeObjectFromPath<DecksData>(_cacheDataPathes[type]);
                    }
                    break;*/
                case Enumerators.CacheDataType.DECKS_OPPONENT_DATA:
                    {
                        if (File.Exists(_cacheDataPathes[type]))
                            CachedOpponentDecksData = DeserializeObjectFromPath<OpponentDecksData>(_cacheDataPathes[type]);
                    }
                    break;
                case Enumerators.CacheDataType.OPPONENT_ACTIONS_LIBRARY_DATA:
                    {
                        if (File.Exists(_cacheDataPathes[type]))
                            CachedActionsLibraryData = DeserializeObjectFromPath<ActionData>(_cacheDataPathes[type]);
                    }
                    break;
                case Enumerators.CacheDataType.CREDITS_DATA:
                    {
                        if (File.Exists(_cacheDataPathes[type]))
                            CachedCreditsData = DeserializeObjectFromPath<CreditsData>(_cacheDataPathes[type]);
                    }
                    break;
                default: break;
            }
        }

        private void CheckFirstLaunch()
        {
            if (!File.Exists(Path.Combine(Application.persistentDataPath, Constants.LOCAL_CARDS_LIBRARY_DATA_FILE_PATH)))
            {
                CachedCardsLibraryData = JsonConvert.DeserializeObject<CardsLibraryData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/card_library_data").text);
                CachedHeroesData = JsonConvert.DeserializeObject<HeroesData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/heroes_data").text);
                //CachedCollectionData = JsonConvert.DeserializeObject<CollectionData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/collection_data").text);
                //CachedDecksData = JsonConvert.DeserializeObject<DecksData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/decks_data").text);
                CachedOpponentDecksData = JsonConvert.DeserializeObject<OpponentDecksData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/opponent_decks_data").text);
                CachedActionsLibraryData = JsonConvert.DeserializeObject<ActionData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/action_data").text);
                CachedCreditsData = JsonConvert.DeserializeObject<CreditsData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/credits_data").text);

                var collectionLibrary = _loadObjectsManager.GetObjectByPath<TextAsset>("Data/collection_data");
                if (collectionLibrary == null)
                    FillFullCollection();
                else
                    CachedCollectionData = JsonConvert.DeserializeObject<CollectionData>(collectionLibrary.text);
            }
        }

        private void FillCacheDataPathes()
        {
            _cacheDataPathes = new Dictionary<Enumerators.CacheDataType, string>();
            _cacheDataPathes.Add(Enumerators.CacheDataType.USER_LOCAL_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_USER_DATA_FILE_PATH));
			_cacheDataPathes.Add(Enumerators.CacheDataType.CARDS_LIBRARY_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_CARDS_LIBRARY_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.HEROES_DATA, Path.Combine(Application.persistentDataPath , Constants.LOCAL_HEROES_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.COLLECTION_DATA, Path.Combine(Application.persistentDataPath , Constants.LOCAL_COLLECTION_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.DECKS_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_DECKS_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.DECKS_OPPONENT_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_OPPONENT_DECKS_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.OPPONENT_ACTIONS_LIBRARY_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_OPPONENT_ACTIONS_LIBRARY_DATA_FILE_PATH));
            _cacheDataPathes.Add(Enumerators.CacheDataType.CREDITS_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_CREDITS_DATA_FILE_PATH));
        }

        private T DeserializeObjectFromPath<T>(string path)
        {
            if(Constants.DATA_ENCRYPTION_ENABLED)
                return JsonConvert.DeserializeObject<T>(Utilites.Decrypt(File.ReadAllText(path), Constants.PRIVATE_ENCRYPTION_KEY_FOR_APP));
            else
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }

        private string SerializeObject(object obj)
        {
            if (Constants.DATA_ENCRYPTION_ENABLED)
                return Utilites.Encrypt(JsonConvert.SerializeObject(obj, Formatting.Indented), Constants.PRIVATE_ENCRYPTION_KEY_FOR_APP);
            else
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        private void FillFullCollection()
        {
            CachedCollectionData = new CollectionData();
            CachedCollectionData.cards = new List<CollectionCardData>();

            foreach (var set in CachedCardsLibraryData.sets)
            {
                foreach (var card in set.cards)
                {
                    CachedCollectionData.cards.Add(new CollectionCardData()
                    {
                        amount = (int)GetMaxCopiesValue(card, set.name),
                        cardName = card.name
                    });
                }
            }
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
    }
}