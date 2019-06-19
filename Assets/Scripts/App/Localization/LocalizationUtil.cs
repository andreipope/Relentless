using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using log4net;
using I2LocalizationManager = I2.Loc.LocalizationManager;

namespace Loom.ZombieBattleground.Localization
{
    public static class LocalizationUtil
    {
        private static readonly ILog Log = Logging.GetLog(nameof(LocalizationUtil));
        
        public enum Language
        {
            None = -1,
            English = 0,
            Chinese = 1,
            Korean = 2,            
            Japanese = 3,
            Spanish = 4,
            Thai = 5,
        }
        
        public static event Action<Language> LanguageWasChangedEvent;
        
        public static Language CurrentLanguage { get; private set; } = Language.None;
        
        private static Dictionary<LocalizationTerm, LocalizedString> LocalizedStringDictionary = new Dictionary<LocalizationTerm, LocalizedString>();
    
        public static string GetLocalizedString(LocalizationTerm term)
        {
            if( !LocalizedStringDictionary.ContainsKey(term) )
            {
                LocalizedStringDictionary.Add
                (
                    term,
                    new LocalizedString(term.ToString())
                );
            }
            return LocalizedStringDictionary[term].ToString().Replace("\\n", "\n");
        }

        public static void SetLanguage(Language language, bool forceUpdate = false)
        {
            if (language == CurrentLanguage && !forceUpdate)
                return;
                
            SetLanguage(language.ToString());
        }

        private static void SetLanguage(string language)
        {            
            if (I2LocalizationManager.HasLanguage(language))
            {
                I2LocalizationManager.CurrentLanguage = language;
                LanguageWasChangedEvent?.Invoke(CurrentLanguage);
                SaveCacheLanguage();
                Log.Info($"Change game's language to {CurrentLanguage.ToString()}");
            }
            else
            {
                Log.Info($"{nameof(I2LocalizationManager)} not contain language {language}"); 
            }
        }
        
        public static void ApplyLocalization()
        {
            LoadCacheLanguage();
            CurrentLanguage = Language.English;
            SetLanguage(CurrentLanguage, true);
        }
        
        private static void LoadCacheLanguage()
        {
            //TODO : Load selected language from cache data
            //_dataManager.CachedUserLocalData.AppLanguage
        }
        
        private static void SaveCacheLanguage()
        {
            //TODO : Save selected language to cache data
            //_dataManager.CachedUserLocalData.AppLanguage
        }
    }
}
