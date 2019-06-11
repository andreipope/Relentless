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
        
        //Calling this method at Init
        public void InitializeLocalization()
        {
            _localizationControlManager = GameClient.Get<LocalizationControlManager>();
            LocalizedTextList = new List<TextMeshProUGUI>();
        }
        
        public void AddLocalizedComponent(TextMeshProUGUI label, Enumerators.LocalizationTerm term)
        {
            Localize localize = label.gameObject.GetComponent<Localize>();
            if(localize == null)
            {
                localize = label.gameObject.AddComponent<Localize>();
            }
            localize.Term = term.ToString();
        }
        
        public void AddLabelToTextList(TextMeshProUGUI label)
        {
            LocalizedTextList.Add(label);
        }

        //Override this method to add text label to list then calling this method when showing UI
        public virtual void RegisterLocalizedTextList()
        {
            _localizationControlManager.RegisterTextLabels(this);
        }
        
        //Calling this method when hiding an UI
        public void UnRegisterLocalizedTextList()
        {
            _localizationControlManager.UnRegisterTextLabels(this);
            LocalizedTextList.Clear();
        }
    }
}
