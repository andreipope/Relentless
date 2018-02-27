using GrandDevs.CZB.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GrandDevs.Internal;
using FullSerializer;
using CCGKit;

namespace GrandDevs.CZB
{
    public class DataManager : IService, IDataManager
    {
        private IAppStateManager _appStateManager;
        private ILocalizationManager _localizationManager;

        private Dictionary<Enumerators.CacheDataType, string> _cacheDataPathes;

        public UserLocalData CachedUserLocalData { get; set; }

		private int _currentDeckIndex;
		private int _currentAIDeckIndex;

		private fsSerializer serializer = new fsSerializer();

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
        }

        public void Dispose()
        {
            SaveAllCache();
        }

        public void Init()
        {
            _appStateManager = GameClient.Get<IAppStateManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();

            FillCacheDataPathes();
        }

        public void StartLoadCache()
        {

            int count = Enum.GetNames(typeof(Enumerators.CacheDataType)).Length;
            for (int i = 0; i < count; i++)
                LoadCachedData((Enumerators.CacheDataType)i);

            _localizationManager.ApplyLocalization();

            LoadCCGData();
            //FillDummyData();
            CreateHeroesData();

            //_appStateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }

        public void Update()
        {

        }

        private void LoadCCGData()
        {
			var defaultDeckTextAsset = Resources.Load<TextAsset>("DefaultDeck");
			if (defaultDeckTextAsset != null)
			{
				GameManager.Instance.defaultDeck = JsonUtility.FromJson<Deck>(defaultDeckTextAsset.text);
			}

			var decksPath = Application.persistentDataPath + "/decks.json";
            List<Deck> decks = new List<Deck>();
			if (File.Exists(decksPath))
			{
				var file = new StreamReader(decksPath);
				var fileContents = file.ReadToEnd();
				var data = fsJsonParser.Parse(fileContents);
				object deserialized = null;
				serializer.TryDeserialize(data, typeof(List<Deck>), ref deserialized).AssertSuccessWithoutWarnings();
				file.Close();
                decks = deserialized as List<Deck>;
				GameManager.Instance.playerDecks = decks;
			}

            List<Deck> decksToRemove = new List<Deck>();
            foreach(Deck deck in GameManager.Instance.playerDecks)
            {
                if (deck.GetNumCards() > 30)
                    decksToRemove.Add(deck);
            }
            foreach (Deck deck in decksToRemove)
            {
                GameManager.Instance.playerDecks.Remove(deck);
            }
            decksToRemove.Clear();

            GameNetworkManager.Instance.Initialize();

			if (decks != null && decks.Count > 0)
			{
				/*_currentDeckIndex = PlayerPrefs.GetInt("default_deck");
				if (_currentDeckIndex < decks.Count)
				{
					PlayerPrefs.SetInt("default_deck", _currentDeckIndex);
				}
				else
				{
					PlayerPrefs.SetInt("default_deck", 0);
				} */
				_currentAIDeckIndex = PlayerPrefs.GetInt("default_ai_deck");
				if (_currentAIDeckIndex < decks.Count)
				{
					PlayerPrefs.SetInt("default_ai_deck", _currentAIDeckIndex);
				}
				else
				{
					PlayerPrefs.SetInt("default_ai_deck", 0);
				}

                //GameManager.Instance.currentDeckId = _currentDeckIndex;
            }
        }

        private void CreateHeroesData()
        {
            GameManager.Instance.heroes.Add(new Hero() { element = Enumerators.ElementType.FIRE, name = "Pyro Zombie Hero" });
            GameManager.Instance.heroes.Add(new Hero() { element = Enumerators.ElementType.EARTH, name = "Golem Zombie Hero" });
        }

        private void FillDummyData()
        {
            Debug.Log(GameManager.Instance.config.cardSets);
            Deck deck;
            for(int i = 0; i < 3; i++)
            {
                deck = new Deck();
                deck.name = "Deck " + i;
                for (int m = 0; m < (10*i + UnityEngine.Random.Range(10, 20)); m++)
                {
                    deck.AddCard(GameManager.Instance.config.cards[UnityEngine.Random.Range(0, GameManager.Instance.config.cards.Count)]);
                }
                GameManager.Instance.playerDecks.Add(deck);
            }
        }

        public void SaveAllCache()
        {
            int count = Enum.GetNames(typeof(Enumerators.CacheDataType)).Length;
            for (int i = 0; i < count; i++)
                SaveCache((Enumerators.CacheDataType)i);
        }

        public void SaveCache(Enumerators.CacheDataType type)
        {
            switch (type)
            {
                case Enumerators.CacheDataType.USER_LOCAL_DATA:
                    {
                        if (!File.Exists(_cacheDataPathes[type]))
                            File.Create(_cacheDataPathes[type]).Close();

                        File.WriteAllText(_cacheDataPathes[type], SerializeObject(CachedUserLocalData));
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
                case Enumerators.CacheDataType.USER_LOCAL_DATA:
                    {
                        if (File.Exists(_cacheDataPathes[type]))
                            CachedUserLocalData = DeserializeObjectFromPath<UserLocalData>(_cacheDataPathes[type]);
                    }
                    break;
                default: break;
            }
        }


        private void FillCacheDataPathes()
        {
            _cacheDataPathes = new Dictionary<Enumerators.CacheDataType, string>();
            _cacheDataPathes.Add(Enumerators.CacheDataType.USER_LOCAL_DATA, Path.Combine(Application.persistentDataPath, Constants.LOCAL_USER_DATA_FILE_PATH));
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
                return Utilites.Encrypt(JsonConvert.SerializeObject(obj), Constants.PRIVATE_ENCRYPTION_KEY_FOR_APP);
            else
                return JsonConvert.SerializeObject(obj);
        }
    }
}