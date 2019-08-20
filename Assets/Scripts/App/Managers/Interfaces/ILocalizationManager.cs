using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Localization;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public interface ILocalizationManager
    {
        event Action<Enumerators.Language> LanguageWasChangedEvent;

        Dictionary<SystemLanguage, Enumerators.Language> SupportedLanguages { get; }

        Enumerators.Language CurrentLanguage { get; }

        void ApplyLocalization();

        Task SetLanguage(Enumerators.Language language, bool forceUpdate = false);

        string GetUITranslation(string key, string fallbackText);
        
        string GetUITranslation(LocalizationTerm term, string fallbackText);
    }
}
