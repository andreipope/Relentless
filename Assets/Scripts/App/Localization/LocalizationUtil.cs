using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using log4net;
using Loom.ZombieBattleground.Common;
using I2LocalizationManager = I2.Loc.LocalizationManager;

namespace Loom.ZombieBattleground.Localization
{
    public static class LocalizationUtil
    {
        private static readonly ILog Log = Logging.GetLog(nameof(LocalizationUtil));
        
        private static Dictionary<LocalizationTerm, LocalizedString> LocalizedStringDictionary = new Dictionary<LocalizationTerm, LocalizedString>();

        public static readonly Dictionary<Enumerators.Language, string> IsoLanguageCodeToFullLanguageNameMap = new Dictionary<Enumerators.Language, string>
        {
            { Enumerators.Language.EN, "English" },
            { Enumerators.Language.ZH_CN, "Chinese" },
            { Enumerators.Language.KO, "Korean" },
            { Enumerators.Language.JA, "Japanese" },
            { Enumerators.Language.ES, "Spanish" },
            { Enumerators.Language.TH, "Thai" }
        };

        public static string GetLocalizedString(LocalizationTerm term, string fallbackText = "")
        {
            if( !LocalizedStringDictionary.ContainsKey(term) )
            {
                LocalizedStringDictionary.Add
                (
                    term,
                    new LocalizedString(term.ToString())
                );
            }
            try
            {
                return LocalizedStringDictionary[term].ToString().Replace("\\n", "\n");
            }
            catch
            {
                return fallbackText;
            }
        }

        public static void SetLanguage(Enumerators.Language language)
        {
            string fullLanguageName = GetFullLanguageName(language);
            if (I2LocalizationManager.HasLanguage(fullLanguageName))
            {
                I2LocalizationManager.CurrentLanguage = fullLanguageName;
                Log.Info($"Change game's language to {I2LocalizationManager.CurrentLanguage}");
            }
            else
            {
                Log.Info($"{nameof(I2LocalizationManager)} not contain language {language}"); 
            }
        }
        
        private static string GetFullLanguageName(Enumerators.Language language)
        {
            return IsoLanguageCodeToFullLanguageNameMap.ContainsKey(language) ?
                IsoLanguageCodeToFullLanguageNameMap[language] :
                "";
        }
    }
}
