using GrandDevs.CZB.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace GrandDevs.CZB
{
    public class LocalizationManager : IService, ILocalizationManager
    {
        public event Action<Enumerators.Language> LanguageWasChangedEvent;

        private IDataManager _dataManager;

        private Enumerators.Language _defaultLanguage = Enumerators.Language.EN,
                                     _currentLanguage = Enumerators.Language.NONE;


        private Dictionary<SystemLanguage, Enumerators.Language> _languages;

        public Dictionary<SystemLanguage, Enumerators.Language> SupportedLanguages { get { return _languages; } }

        public Enumerators.Language CurrentLanguage { get { return _currentLanguage; } }


        public void Dispose()
        {
        }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();

            FillLanguages();
        }

        public void ApplyLocalization()
        {
            if (!_languages.ContainsKey(Application.systemLanguage))
            {
                if (_dataManager.CachedUserLocalData.appLanguage == Enumerators.Language.NONE)
                    SetLanguage(_defaultLanguage);
                else
                {
                    SetLanguage(_dataManager.CachedUserLocalData.appLanguage);
                }
            }
            else
            {
                if (_dataManager.CachedUserLocalData.appLanguage == Enumerators.Language.NONE)
                    SetLanguage(_languages[Application.systemLanguage]);
                else
                    SetLanguage(_dataManager.CachedUserLocalData.appLanguage);
            }
        }


        public void Update()
        {
        }

        public void SetLanguage(Enumerators.Language language, bool forceUpdate = false)
        {
            if (language == CurrentLanguage && !forceUpdate)
                return;

            string languageCode = language.ToString().ToLower();

            I2.Loc.LocalizationManager.SetLanguageAndCode(I2.Loc.LocalizationManager.GetLanguageFromCode(languageCode), languageCode);

            _currentLanguage = language;
            _dataManager.CachedUserLocalData.appLanguage = language;

            if (LanguageWasChangedEvent != null)
                LanguageWasChangedEvent(_currentLanguage);
        }

        public string GetUITranslation(string key)
        {
            return I2.Loc.LocalizationManager.GetTermTranslation(key);
        }


        private void FillLanguages()
        {
            _languages = new Dictionary<SystemLanguage, Enumerators.Language>();

            _languages.Add(SystemLanguage.Russian, Enumerators.Language.RU);
            _languages.Add(SystemLanguage.English, Enumerators.Language.EN);
            _languages.Add(SystemLanguage.German, Enumerators.Language.DE);
        }
    }
}