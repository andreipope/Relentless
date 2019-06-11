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
            Chinese = 1,
            Korean = 2,            
            Japanese = 3,
            Spanish = 4,
            Thai = 5,
        }
        
        private readonly float[] LineSpacing = new float[]
        {
            -26.7f,
            0f,
            0f,
            0f,
            0f,
            0f,
        };
        
        private readonly float[] CharSpacing = new float[]
        {
            -0.71f,
            -0.87f,
            -1.5f,
            -3.25f,
            -0.71f,
            -4.21f,
        };
        
        private readonly FontStyles[] FontStyle = new FontStyles[]
        {
            FontStyles.Bold,
            FontStyles.Bold,
            FontStyles.Bold,
            FontStyles.Bold,
            FontStyles.Bold,
            FontStyles.Bold,
        };

        public event Action<Language> LanguageWasChangedEvent;
        
        public Language CurrentLanguage { get; private set; } = Language.None;

        private TMP_FontAsset[] _fontAssets;

        private FontDecorationData _fontDecorationData;
        
        private List<TextMeshProUGUI> _registeredLabelList;

        private ILoadObjectsManager _loadObjectsManager;        
        
        public class FontDecorationData
        {
            public TMP_FontAsset FontAsset;
            public float LineSpacing;
            public float CharacterSpacing;
            public FontStyles FontStyles;
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            
            _registeredLabelList = new List<TextMeshProUGUI>();
            _fontAssets = new TMP_FontAsset[6];
            _fontAssets[0] = _loadObjectsManager.GetObjectByPath<TMP_FontAsset>("FontAssets/EN_Bevan");
            _fontAssets[1] = _loadObjectsManager.GetObjectByPath<TMP_FontAsset>("FontAssets/CH_NotoSansRegular_Common1");
            _fontAssets[2] = _loadObjectsManager.GetObjectByPath<TMP_FontAsset>("FontAssets/KR_NotosansRegular_1");
            _fontAssets[3] = _loadObjectsManager.GetObjectByPath<TMP_FontAsset>("FontAssets/JP_TogaliteRegular_KanaPunctuation");
            _fontAssets[4] = _loadObjectsManager.GetObjectByPath<TMP_FontAsset>("FontAssets/EN_Bevan");
            _fontAssets[5] = _loadObjectsManager.GetObjectByPath<TMP_FontAsset>("FontAssets/TH_Krungthep");

            _fontDecorationData = new FontDecorationData();
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
                UpdateStyleData();
                UpdateStyle();
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

            UpdateStyleData();          
            
            foreach(TextMeshProUGUI label in localizableUI.LocalizedTextList)
            {
                if(!_registeredLabelList.Contains(label))
                {                    
                    UpdateStyle(label);
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

        private void UpdateStyleData()
        {
            _fontDecorationData.FontAsset = _fontAssets[(int)CurrentLanguage];
            _fontDecorationData.LineSpacing = LineSpacing[(int)CurrentLanguage];
            _fontDecorationData.CharacterSpacing = CharSpacing[(int)CurrentLanguage];
            _fontDecorationData.FontStyles = FontStyle[(int)CurrentLanguage];
        }

        private void UpdateStyle()
        {
            foreach(TextMeshProUGUI item in _registeredLabelList)
            {
                UpdateStyle(item);
            }
        }
        
        private void UpdateStyle(TextMeshProUGUI item)
        {
            if (item == null)
                return;
                
            item.font = _fontDecorationData.FontAsset;
            item.lineSpacing = _fontDecorationData.LineSpacing;
            item.characterSpacing = _fontDecorationData.CharacterSpacing;
            item.fontStyle = _fontDecorationData.FontStyles;
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