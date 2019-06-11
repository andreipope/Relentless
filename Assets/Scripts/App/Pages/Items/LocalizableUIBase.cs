using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using I2.Loc;
using TMPro;

namespace Loom.ZombieBattleground
{
    public class LocalizableUIBase : ILocalizableUI
    {
        protected LocalizationControlManager _localizationControlManager;
        
        public List<TextMeshProUGUI> LocalizedTextList { get; private set; }
        
        public void InitializeLocalization()
        {
            _localizationControlManager = GameClient.Get<LocalizationControlManager>();
            LocalizedTextList = new List<TextMeshProUGUI>();
        }
        
        public void AddLocalizedText(TextMeshProUGUI label, Enumerators.LocalizationTerm term)
        {
            label.gameObject.AddComponent<Localize>().Term = term.ToString();
            LocalizedTextList.Add(label);
        }

        public virtual void RegisterLocalizedTextList()
        {
            _localizationControlManager.RegisterTextLabels(this);
        }
        
        public void UnRegisterLocalizedTextList()
        {
            _localizationControlManager.UnRegisterTextLabels(this);
            LocalizedTextList.Clear();
        }
    }
}
