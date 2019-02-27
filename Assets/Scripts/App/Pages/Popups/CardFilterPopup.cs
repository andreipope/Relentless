using System;
using System.Linq;
using System.Collections.Generic;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.Experimental.PlayerLoop;

namespace Loom.ZombieBattleground
{
    public class CardFilterPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        
        public event Action<CardFilterData> ActionPopupHiding;
        
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private Button _buttonClose,
                       _buttonSave,
                       _buttonSelectNone,
                       _buttonSelectAll,
                       _buttonElement,
                       _buttonRank,
                       _buttonType,
                       _buttonGooCost;

        private Dictionary<Enumerators.SetType, Button> _buttonElementsDictionary;
        
        private readonly List<Enumerators.SetType> _availableSetTypeList = new List<Enumerators.SetType>()
        {
            Enumerators.SetType.FIRE,
            Enumerators.SetType.WATER,
            Enumerators.SetType.EARTH,
            Enumerators.SetType.AIR,
            Enumerators.SetType.LIFE,
            Enumerators.SetType.TOXIC,
            Enumerators.SetType.ITEM
        };

        public CardFilterData FilterData;

        #region IUIPopup

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _buttonElementsDictionary = new Dictionary<Enumerators.SetType, Button>();
            FilterData = new CardFilterData(_availableSetTypeList);      
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;

            _buttonElementsDictionary.Clear();
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/CardFilterPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false); 
            
            _buttonClose = Self.transform.Find("Scaler/Button_Close").GetComponent<Button>();                        
            _buttonClose.onClick.AddListener(ButtonCloseHandler);
            _buttonClose.onClick.AddListener(PlayClickSound);
            
            _buttonSave = Self.transform.Find("Scaler/Button_Save").GetComponent<Button>();                        
            _buttonSave.onClick.AddListener(ButtonSaveHandler);
            _buttonSave.onClick.AddListener(PlayClickSound);
            
            _buttonSelectNone = Self.transform.Find("Scaler/Button_SelectNone").GetComponent<Button>();                        
            _buttonSelectNone.onClick.AddListener(ButtonSelectNoneHandler);
            _buttonSelectNone.onClick.AddListener(PlayClickSound);
            
            _buttonSelectAll = Self.transform.Find("Scaler/Button_SelectAll").GetComponent<Button>();                        
            _buttonSelectAll.onClick.AddListener(ButtonSelectAllHandler);
            _buttonSelectAll.onClick.AddListener(PlayClickSound);
            
            _buttonElement = Self.transform.Find("Scaler/Button_Element").GetComponent<Button>();                        
            _buttonElement.onClick.AddListener(ButtonElementHandler);
            _buttonElement.onClick.AddListener(PlayClickSound);
            
            _buttonRank = Self.transform.Find("Scaler/Button_Rank").GetComponent<Button>();                        
            _buttonRank.onClick.AddListener(ButtonRankHandler);
            _buttonRank.onClick.AddListener(PlayClickSound);
            
            _buttonType = Self.transform.Find("Scaler/Button_Type").GetComponent<Button>();                        
            _buttonType.onClick.AddListener(ButtonTypeHandler);
            _buttonType.onClick.AddListener(PlayClickSound);
            
            _buttonGooCost = Self.transform.Find("Scaler/Button_GooCost").GetComponent<Button>();                        
            _buttonGooCost.onClick.AddListener(ButtonGooCostHandler);
            _buttonGooCost.onClick.AddListener(PlayClickSound);

            _buttonElementsDictionary.Clear();
            foreach(Enumerators.SetType setType in _availableSetTypeList)
            {
                Button buttonElementIcon = Self.transform.Find("Scaler/Tab_Element/Group_ElementIcons/Button_element_"+setType.ToString().ToLower()).GetComponent<Button>();
                buttonElementIcon.onClick.AddListener
                (
                    ()=> ButtonElementIconHandler(setType)
                );
                buttonElementIcon.onClick.AddListener(PlayClickSound);

                _buttonElementsDictionary.Add(setType, buttonElementIcon);
            }

            FilterData.Reset();     
        }
        
        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        #endregion

        #region Buttons Handlers

        private void ButtonCloseHandler()
        {
            _uiManager.HidePopup<CardFilterPopup>();
        }
        
        private void ButtonSaveHandler()
        {            
            if (!CheckIfAnyElementSelected())
            {
                OpenAlertDialog("No element selected!\nPlease select atleast one.");
            }
            else
            {
                _uiManager.HidePopup<CardFilterPopup>();
                ActionPopupHiding?.Invoke(FilterData);
            }
        }
        
        private void ButtonElementIconHandler(Enumerators.SetType setType)
        {
            ToggleSelectedSetType(setType);            
        }

        private void ButtonSelectNoneHandler()
        {
            foreach(Enumerators.SetType setType in _availableSetTypeList)
            {
                SetSelectedSetType(setType, false);
            }
        }
        
        private void ButtonSelectAllHandler()
        {
            foreach(Enumerators.SetType setType in _availableSetTypeList)
            {
                SetSelectedSetType(setType, true);
            }
        }
        
        private void ButtonElementHandler()
        {

        }
        
        private void ButtonRankHandler()
        {

        }
        
        private void ButtonTypeHandler()
        {

        }
        
        private void ButtonGooCostHandler()
        {

        }

        #endregion

        private void SetSelectedSetType(Enumerators.SetType setType, bool status)
        {
            FilterData.SetTypeDictionary[setType] = status;
            UpdateSetTypeButtonDisplay(setType);
        }
        
        private void ToggleSelectedSetType(Enumerators.SetType setType)
        {
            FilterData.SetTypeDictionary[setType] = !FilterData.SetTypeDictionary[setType];
            UpdateSetTypeButtonDisplay(setType);
        }
        
        private void UpdateSetTypeButtonDisplay(Enumerators.SetType setType)
        {
            _buttonElementsDictionary[setType].GetComponent<Image>().color =
                FilterData.SetTypeDictionary[setType] ? Color.white : Color.gray;
        }
        
        private bool CheckIfAnyElementSelected()
        {
            bool selected = false;
            foreach (Enumerators.SetType setType in _availableSetTypeList)
            {
                if (FilterData.SetTypeDictionary[setType])
                {
                    selected = true;
                    break;
                }
            }
            return selected;            
        }

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
        
        public void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }
        
        public class CardFilterData
        {            
            public Dictionary<Enumerators.SetType, bool> SetTypeDictionary;
            
            public CardFilterData(List<Enumerators.SetType> availableSetTypeList)
            {
                SetTypeDictionary = new Dictionary<Enumerators.SetType, bool>();
                foreach(Enumerators.SetType setType in availableSetTypeList)
                {
                    SetTypeDictionary.Add(setType, true);
                }
            }
            
            public List<Enumerators.SetType> GetFilterSetTypeList()
            {
                List<Enumerators.SetType> setTypeList = new List<Enumerators.SetType>();
                foreach (KeyValuePair<Enumerators.SetType, bool> kvp in SetTypeDictionary)
                {
                    if(kvp.Value)
                        setTypeList.Add(kvp.Key);
                }
                return setTypeList;
            }

            public void Reset()
            {
                for(int i=0; i<SetTypeDictionary.Count;++i)
                {
                    KeyValuePair<Enumerators.SetType, bool> kvp = SetTypeDictionary.ElementAt(i);
                    SetTypeDictionary[kvp.Key] = true;
                }
            }
        }

    }
}