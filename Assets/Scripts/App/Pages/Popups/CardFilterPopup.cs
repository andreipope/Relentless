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

        private List<Button> _buttonGooCostList;
        
        public readonly List<Enumerators.SetType> AllAvailableSetTypeList = new List<Enumerators.SetType>()
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
        
        public enum TAB
        {
            NONE = -1,
            ELEMENT = 0,
            RANK = 1,
            TYPE = 2,
            GOO_COST = 3
        }
        
        private TAB _tab;
        
        private GameObject[] _tabObjects;
        
        public event Action<TAB> EventChangeTab;

        #region IUIPopup

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _buttonElementsDictionary = new Dictionary<Enumerators.SetType, Button>();
            FilterData = new CardFilterData(AllAvailableSetTypeList);
            _buttonGooCostList = new List<Button>();
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
            _buttonGooCostList.Clear();
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
            foreach(Enumerators.SetType setType in AllAvailableSetTypeList)
            {
                Button buttonElementIcon = Self.transform.Find("Scaler/Tab_Element/Group_ElementIcons/Button_element_"+setType.ToString().ToLower()).GetComponent<Button>();
                buttonElementIcon.onClick.AddListener
                (
                    ()=> ButtonElementIconHandler(setType)
                );
                buttonElementIcon.onClick.AddListener(PlayClickSound);

                _buttonElementsDictionary.Add(setType, buttonElementIcon);
            }

            _buttonGooCostList.Clear();
            for(int i=0;i<11;++i)
            {
                int gooIndex = i;
                Button button = Self.transform.Find("Scaler/Tab_GooCost/Group_GooIcons/Button_element_goo_" + i).GetComponent<Button>();
                button.onClick.AddListener(() =>
                    {
                        FilterData.GooCostList[gooIndex] = !FilterData.GooCostList[gooIndex];
                        UpdateGooCostButtonDisplay(gooIndex);
                    });
                button.onClick.AddListener(PlayClickSound);
                _buttonGooCostList.Add(button);
            }

            _tabObjects = new GameObject[]
            {
                Self.transform.Find("Scaler/Tab_Element").gameObject,
                Self.transform.Find("Scaler/Tab_Rank").gameObject,
                Self.transform.Find("Scaler/Tab_Type").gameObject,
                Self.transform.Find("Scaler/Tab_GooCost").gameObject
            };

            FilterData.Reset();
            LoadTabs();  
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
                return;
            }
            
            if (!CheckIfAnyGooCostSelected())
            {
                OpenAlertDialog("No goo cost selected!\nPlease select atleast one.");
                return;
            }
            
            _uiManager.HidePopup<CardFilterPopup>();
            ActionPopupHiding?.Invoke(FilterData);            
        }
        
        private void ButtonElementIconHandler(Enumerators.SetType setType)
        {
            ToggleSelectedSetType(setType);            
        }

        private void ButtonSelectNoneHandler()
        {
            switch (_tab)
            {
                case TAB.ELEMENT:
                    foreach (Enumerators.SetType setType in AllAvailableSetTypeList)
                        SetSelectedSetType(setType, false);
                    break;
                case TAB.GOO_COST:
                    for(int i=0; i<FilterData.GooCostList.Count;++i)
                    {
                        FilterData.GooCostList[i] = false;
                        UpdateGooCostButtonDisplay(i);
                    }
                    break;
                default:
                    return;
            }
        }
        
        private void ButtonSelectAllHandler()
        {
            switch (_tab)
            {
                case TAB.ELEMENT:
                    foreach (Enumerators.SetType setType in AllAvailableSetTypeList)
                        SetSelectedSetType(setType, true);
                    break;
                case TAB.GOO_COST:
                    for(int i=0; i<FilterData.GooCostList.Count;++i)
                    {
                        FilterData.GooCostList[i] = true;
                        UpdateGooCostButtonDisplay(i);
                    }
                    break;
                default:
                    return;
            }
        }
        
        private void ButtonElementHandler()
        {
            ChangeTab(TAB.ELEMENT);
        }
        
        private void ButtonRankHandler()
        {
            ChangeTab(TAB.RANK);
        }
        
        private void ButtonTypeHandler()
        {
            ChangeTab(TAB.TYPE);
        }
        
        private void ButtonGooCostHandler()
        {
            ChangeTab(TAB.GOO_COST);
        }

        #endregion
        
        private void LoadTabs()
        {
            _tab = TAB.NONE;
            ChangeTab(TAB.ELEMENT);
        }
        
        public void ChangeTab(TAB newTab)
        {
            if (newTab == _tab)
                return;
                
            _tab = newTab;            
            
            for(int i=0; i<_tabObjects.Length;++i)
            {
                GameObject tabObject = _tabObjects[i];
                tabObject.SetActive(i == (int)newTab);
            }
            
            switch (newTab)
            {
                case TAB.NONE:
                    break;
                case TAB.ELEMENT:
                    break;
                case TAB.RANK:
                    break;
                case TAB.TYPE:                                      
                    break;
                case TAB.GOO_COST:                    
                    break;
                default:
                    break;
            }            
            
            EventChangeTab?.Invoke(_tab);
        }

        #region Element

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
            return FilterData.SetTypeDictionary.Any(kvp => kvp.Value);           
        }

        #endregion
        
        private void UpdateGooCostButtonDisplay(int gooIndex)
        {
            _buttonGooCostList[gooIndex].GetComponent<Image>().color =
                FilterData.GooCostList[gooIndex] ? Color.white : Color.gray;
            _buttonGooCostList[gooIndex].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = _buttonGooCostList[gooIndex].GetComponent<Image>().color;
        }
        
        private bool CheckIfAnyGooCostSelected()
        {
            return FilterData.GooCostList.Any(selected => selected);
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
            public List<bool> GooCostList;
            
            public CardFilterData(List<Enumerators.SetType> availableSetTypeList)
            {
                SetTypeDictionary = new Dictionary<Enumerators.SetType, bool>();
                foreach(Enumerators.SetType setType in availableSetTypeList)
                {
                    SetTypeDictionary.Add(setType, true);
                }
                GooCostList = new List<bool>();
                for(int i=0; i<11; ++i)
                {
                    GooCostList.Add(true);
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
                for (int i = 0; i < GooCostList.Count; ++i)
                {
                    GooCostList[i] = true;
                }
            }
        }

    }
}