using System;
using System.Collections.Generic;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class LocalizationManager : IService, ILocalizationManager
    {
        private readonly Enumerators.Language _defaultLanguage = Enumerators.Language.EN;

        private IDataManager _dataManager;

        private ILoadObjectsManager _loadObjectsManager;

        public event Action<Enumerators.Language> LanguageWasChangedEvent;

        public Dictionary<SystemLanguage, Enumerators.Language> SupportedLanguages { get; private set; }

        public Enumerators.Language CurrentLanguage { get; private set; } = Enumerators.Language.NONE;

        private Dictionary<string, string> _languageData;


        public LocalizationManager()
        {
            _languageData = new Dictionary<string, string>();
        }

        public void ApplyLocalization()
        {
            if (!SupportedLanguages.ContainsKey(Application.systemLanguage))
            {
                if (_dataManager.CachedUserLocalData.AppLanguage == Enumerators.Language.NONE)
                {
                    SetLanguage(_defaultLanguage);
                }
                else
                {
                    SetLanguage(_dataManager.CachedUserLocalData.AppLanguage);
                }
            }
            else
            {
                if (_dataManager.CachedUserLocalData.AppLanguage == Enumerators.Language.NONE)
                {
                    SetLanguage(SupportedLanguages[Application.systemLanguage]);
                }
                else
                {
                    SetLanguage(_dataManager.CachedUserLocalData.AppLanguage);
                }
            }
        }

        public void SetLanguage(Enumerators.Language language, bool forceUpdate = false)
        {
            if (language == CurrentLanguage && !forceUpdate)
                return;

            CurrentLanguage = language;
            _dataManager.CachedUserLocalData.AppLanguage = language;
            _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
            LoadLanguageData(language);

            LanguageWasChangedEvent?.Invoke(CurrentLanguage);
        }

        public string GetUITranslation(string key)
        {
            if (_languageData.ContainsKey(key))
                return _languageData[key];

            return string.Empty;
        }

        public void Dispose()
        {
        }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            FillLanguages();
        }

        public void Update()
        {
        }

        public void LoadLanguageData(Enumerators.Language language)
        {
            TextAsset localizationTextAsset = _loadObjectsManager.GetObjectByPath<TextAsset>("Data/Localization/"+language);
            _languageData = JsonConvert.DeserializeObject<Dictionary<string, string>>(localizationTextAsset.text);
        }

        private void FillLanguages()
        {
            SupportedLanguages = new Dictionary<SystemLanguage, Enumerators.Language>();

            SupportedLanguages.Add(SystemLanguage.Russian, Enumerators.Language.RU);
            SupportedLanguages.Add(SystemLanguage.English, Enumerators.Language.EN);
            SupportedLanguages.Add(SystemLanguage.German, Enumerators.Language.DE);
        }
    }
}
