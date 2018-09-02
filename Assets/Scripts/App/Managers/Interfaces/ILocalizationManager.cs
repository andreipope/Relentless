using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public interface ILocalizationManager
    {
        event Action<Enumerators.Language> LanguageWasChangedEvent;

        Dictionary<SystemLanguage, Enumerators.Language> SupportedLanguages { get; }

        Enumerators.Language CurrentLanguage { get; }

        void ApplyLocalization();

        void SetLanguage(Enumerators.Language language, bool forceUpdate = false);

        string GetUITranslation(string key);
    }
}
