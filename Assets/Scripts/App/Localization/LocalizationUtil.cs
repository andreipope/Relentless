using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;

namespace Loom.ZombieBattleground.Localization
{
    public static class LocalizationUtil
    {
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
    }
}
