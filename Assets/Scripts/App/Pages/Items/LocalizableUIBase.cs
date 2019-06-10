using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
