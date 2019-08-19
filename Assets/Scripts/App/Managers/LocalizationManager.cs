using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using Loom.ZombieBattleground.Localization;

namespace Loom.ZombieBattleground
{
    public class LocalizationManager : IService, ILocalizationManager
    {
        private readonly Enumerators.Language _defaultLanguage = Enumerators.Language.EN;

        private IDataManager _dataManager;

        public event Action<Enumerators.Language> LanguageWasChangedEvent;

        public Dictionary<SystemLanguage, Enumerators.Language> SupportedLanguages { get; private set; }

        public Enumerators.Language CurrentLanguage { get; private set; } = Enumerators.Language.NONE;

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
            LocalizationUtil.SetLanguage(language);

            LanguageWasChangedEvent?.Invoke(CurrentLanguage);
        }

        public string GetUITranslation(string key)
        {
            if(Enum.TryParse(key, out LocalizationTerm localizationTerm ))
            {
                return LocalizationUtil.GetLocalizedString
                (
                     localizationTerm
                );
            }
            
            return "";
        }
        
        public string GetUITranslation(LocalizationTerm term)
        {
            return LocalizationUtil.GetLocalizedString(term);
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

            SupportedLanguages.Add(SystemLanguage.English, Enumerators.Language.EN);
            SupportedLanguages.Add(SystemLanguage.Chinese, Enumerators.Language.ZH_CN);
            SupportedLanguages.Add(SystemLanguage.Korean, Enumerators.Language.KO);
            SupportedLanguages.Add(SystemLanguage.Japanese, Enumerators.Language.JA);
            // Spanish and Thai are not currently translated
            //SupportedLanguages.Add(SystemLanguage.Spanish, Enumerators.Language.ES);
            //SupportedLanguages.Add(SystemLanguage.Thai, Enumerators.Language.TH);
        }
    }
}
