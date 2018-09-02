using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class LocalizationManager : IService, ILocalizationManager
    {
        private readonly Enumerators.Language _defaultLanguage = Enumerators.Language.En;

        private IDataManager _dataManager;

        public event Action<Enumerators.Language> LanguageWasChangedEvent;

        public Dictionary<SystemLanguage, Enumerators.Language> SupportedLanguages { get; private set; }

        public Enumerators.Language CurrentLanguage { get; private set; } = Enumerators.Language.None;

        public void ApplyLocalization()
        {
            if (!SupportedLanguages.ContainsKey(Application.systemLanguage))
            {
                if (_dataManager.CachedUserLocalData.AppLanguage == Enumerators.Language.None)
                {
                    SetLanguage(_defaultLanguage);
                } else
                {
                    SetLanguage(_dataManager.CachedUserLocalData.AppLanguage);
                }
            } else
            {
                if (_dataManager.CachedUserLocalData.AppLanguage == Enumerators.Language.None)
                {
                    SetLanguage(SupportedLanguages[Application.systemLanguage]);
                } else
                {
                    SetLanguage(_dataManager.CachedUserLocalData.AppLanguage);
                }
            }
        }

        public void SetLanguage(Enumerators.Language language, bool forceUpdate = false)
        {
            if ((language == CurrentLanguage) && !forceUpdate)

                return;

            string languageCode = language.ToString().ToLower();

            // I2.Loc.LocalizationManager.SetLanguageAndCode(I2.Loc.LocalizationManager.GetLanguageFromCode(languageCode), languageCode);
            CurrentLanguage = language;
            _dataManager.CachedUserLocalData.AppLanguage = language;

            if (LanguageWasChangedEvent != null)
            {
                LanguageWasChangedEvent(CurrentLanguage);
            }
        }

        public string GetUITranslation(string key)
        {
            return ""; // I2.Loc.LocalizationManager.GetTermTranslation(key);
        }

        public void Dispose()
        {
        }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();

            FillLanguages();
        }

        public void Update()
        {
        }

        private void FillLanguages()
        {
            SupportedLanguages = new Dictionary<SystemLanguage, Enumerators.Language>();

            SupportedLanguages.Add(SystemLanguage.Russian, Enumerators.Language.Ru);
            SupportedLanguages.Add(SystemLanguage.English, Enumerators.Language.En);
            SupportedLanguages.Add(SystemLanguage.German, Enumerators.Language.De);
        }
    }
}
