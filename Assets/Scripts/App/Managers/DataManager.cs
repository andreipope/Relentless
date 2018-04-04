using GrandDevs.CZB.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using GrandDevs.Internal;
using FullSerializer;
using GrandDevs.CZB.Data;
using CCGKit;


namespace GrandDevs.CZB
{
    public class DataManager : IService, IDataManager
    {
        private IAppStateManager _appStateManager;
        private ILocalizationManager _localizationManager;


		private Dictionary<Enumerators.CacheDataType, string> _cacheDataPathes;

        public UserLocalData CachedUserLocalData { get; set; }
		public CardsLibraryData CachedCardsLibraryData { get; set; }
        public HeroesData CachedHeroesData { get; set; }
        public CollectionData CachedCollectionData { get; set; }
        public DecksData CachedDecksData { get; set; }

        private int _currentDeckIndex;
		private int _currentAIDeckIndex;

		private fsSerializer serializer = new fsSerializer();

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
        }

        public void Dispose()
        {
            SaveAllCache();
        }

        public void Init()
        {
            _appStateManager = GameClient.Get<IAppStateManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();

            dir = new DirectoryInfo(Application.persistentDataPath + "/");

            CheckVersion();
            CheckFirstLaunch();
            FillCacheDataPathes();

            GameNetworkManager.Instance.Initialize();
        }

        public void StartLoadCache()
        {
            int count = Enum.GetNames(typeof(Enumerators.CacheDataType)).Length;
            for (int i = 0; i < count; i++)
                LoadCachedData((Enumerators.CacheDataType)i);

            _localizationManager.ApplyLocalization();

            GameManager.Instance.tutorial = CachedUserLocalData.tutorial;
        }

        public void Update()
        {

        }

        public void SaveAllCache()
        {
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
                case Enumerators.CacheDataType.HEROES_DATA:
					{
						if (File.Exists(_cacheDataPathes[type]))
                            CachedHeroesData = DeserializeObjectFromPath<HeroesData>(_cacheDataPathes[type]);
					}
					break;
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
                case Enumerators.CacheDataType.DECKS_DATA:
                    {
                        if (File.Exists(_cacheDataPathes[type]))
                            CachedDecksData = DeserializeObjectFromPath<DecksData>(_cacheDataPathes[type]);
                    }
                    break;
                default: break;
            }
        }

        private void CheckFirstLaunch()
        {
            if (!File.Exists(Path.Combine(Application.persistentDataPath, Constants.LOCAL_CARDS_LIBRARY_DATA_FILE_PATH)))
            {
                CachedCardsLibraryData = JsonConvert.DeserializeObject<CardsLibraryData>(Resources.Load("Data/card_library_data").ToString());
                CachedHeroesData = JsonConvert.DeserializeObject<HeroesData>(Resources.Load("Data/heroes_data").ToString());
                CachedCollectionData = JsonConvert.DeserializeObject<CollectionData>(Resources.Load("Data/collection_data").ToString());
                CachedDecksData = JsonConvert.DeserializeObject<DecksData>(Resources.Load("Data/decks_data").ToString());
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
    }
}