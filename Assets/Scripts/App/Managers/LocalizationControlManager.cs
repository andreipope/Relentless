using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using I2.Loc;
using log4net;

using LocalizationManager = I2.Loc.LocalizationManager;

namespace Loom.ZombieBattleground
{
    public class LocalizationControlManager : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(LocalizationControlManager));
        
        public enum Language
        {
            None = -1,
            English = 0,
            Thai = 1,
            Japanese = 2,
        }
        
        public event Action<Language> LanguageWasChangedEvent;
        
        public Language CurrentLanguage { get; private set; } = Language.None;

        private TMP_FontAsset[] _fontAssets;

        private TMP_FontAsset _currentFont;
        
        private List<TextMeshProUGUI> _registeredLabelList;

        private ILoadObjectsManager _loadObjectsManager;        
        
        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            
            _registeredLabelList = new List<TextMeshProUGUI>();
            _fontAssets = new TMP_FontAsset[3];
            _fontAssets[0] = _loadObjectsManager.GetObjectByPath<TMP_FontAsset>("FontAssets/BevanSDF");
            _fontAssets[1] = _loadObjectsManager.GetObjectByPath<TMP_FontAsset>("FontAssets/THSarabunNewSDF");
            _fontAssets[2] = _loadObjectsManager.GetObjectByPath<TMP_FontAsset>("FontAssets/JapaneseFontSDF");
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            if (_registeredLabelList != null)
            {
                _registeredLabelList.Clear();
                _registeredLabelList = null;
            }           
        }
        
        public void ApplyLocalization()
        {
            LoadCacheLanguage();
            Language cacheLanguage = Language.English;
            SetLanguage(cacheLanguage, true);
        }

        public void SetLanguage(Language language, bool forceUpdate = false)
        {
            if (language == CurrentLanguage && !forceUpdate)
                return;

            bool wipeText = (language != CurrentLanguage);            
            
            string languageStr = language.ToString();
            
            if(I2.Loc.LocalizationManager.HasLanguage(languageStr))
            {
                CurrentLanguage = language;
                RemoveNullLabelFromList();
                if (wipeText)
                {
                    WipeAllText();
                }
                UpdateFont();
                I2.Loc.LocalizationManager.CurrentLanguage = language.ToString();  
                
                LanguageWasChangedEvent?.Invoke(CurrentLanguage);
                SaveCacheLanguage();
                Log.Info($"Change game's language to {CurrentLanguage.ToString()}");                 
            }
            else
            {
                Log.Info($"LocalizationManager not contain language {languageStr}"); 
            }
        }
        
        public void RegisterTextLabels(ILocalizableUI localizableUI)
        {
            if(
                _registeredLabelList == null || 
                localizableUI.LocalizedTextList == null
            )
            {
                Log.Info($"Label list is null");
                return;
            }
            
            _currentFont = _fontAssets[(int)CurrentLanguage];            
            
            foreach(TextMeshProUGUI label in localizableUI.LocalizedTextList)
            {
                if(!_registeredLabelList.Contains(label))
                {
                    label.font = _currentFont;
                    _registeredLabelList.Add(label);
                }
            }
        }
        
        public void UnRegisterTextLabels(ILocalizableUI localizableUI)
        {
            if(
                _registeredLabelList == null || 
                localizableUI.LocalizedTextList == null
            )
            {
                Log.Info($"Label list is null");
                return;
            }
            
            foreach (TextMeshProUGUI label in localizableUI.LocalizedTextList)
            {
                _registeredLabelList.Remove(label);
            }
        }
        
        private void LoadCacheLanguage()
        {
            //_dataManager.CachedUserLocalData.AppLanguage
        }
        
        private void SaveCacheLanguage()
        {
            //_dataManager.CachedUserLocalData.AppLanguage
        }

        private void RemoveNullLabelFromList()
        {
            for(int i = 0; i < _registeredLabelList.Count; ++i)
            {
                if (_registeredLabelList[i] == null)
                {
                    _registeredLabelList.RemoveAt(i);
                    --i;
                }
            }
        }

        private void UpdateFont()
        {
            _currentFont = _fontAssets[(int)CurrentLanguage];
            foreach(TextMeshProUGUI item in _registeredLabelList)
            {
                item.font = _currentFont;
            }
        }
        
        private void WipeAllText()
        {
            foreach(TextMeshProUGUI item in _registeredLabelList)
            {
                item.text = "";
            }
        }
    }
}