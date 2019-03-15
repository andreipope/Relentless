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

        private Dictionary<Enumerators.Faction, Button> _buttonElementsDictionary;

        private Dictionary<Enumerators.CardRank, Button> _buttonRankDictionary;
        
        private Dictionary<Enumerators.CardType, Button> _buttonTypeDictionary;
        
        private List<Button> _buttonGooCostList;
        
        public readonly List<Enumerators.Faction> AllAvailableFactionList = new List<Enumerators.Faction>()
        {
            Enumerators.Faction.FIRE,
            Enumerators.Faction.WATER,
            Enumerators.Faction.EARTH,
            Enumerators.Faction.AIR,
            Enumerators.Faction.LIFE,
            Enumerators.Faction.TOXIC,
            Enumerators.Faction.ITEM
        };

        public readonly List<Enumerators.CardRank> AllAvailableRankList = new List<Enumerators.CardRank>()
        {
            Enumerators.CardRank.MINION,
            Enumerators.CardRank.OFFICER,
            Enumerators.CardRank.GENERAL,
            Enumerators.CardRank.COMMANDER
        };

        public readonly List<Enumerators.CardType> AllAvailableTypeList = new List<Enumerators.CardType>()
        {
            Enumerators.CardType.FERAL,
            Enumerators.CardType.WALKER,
            Enumerators.CardType.HEAVY,
            Enumerators.CardType.UNDEFINED
        };

        public CardFilterData FilterData;
        
        private CardFilterData _cacheFilterData;
        
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
            _buttonElementsDictionary = new Dictionary<Enumerators.Faction, Button>();
            _buttonRankDictionary = new Dictionary<Enumerators.CardRank, Button>();
            _buttonTypeDictionary = new Dictionary<Enumerators.CardType, Button>();
            FilterData = new CardFilterData
            (
                AllAvailableFactionList,
                AllAvailableRankList,
                AllAvailableTypeList
            );
            SaveCacheFilterData();
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
            _buttonRankDictionary.Clear();
            _buttonTypeDictionary.Clear();
            _buttonGooCostList.Clear();

            ResetEventSubscriptions();
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
            
            _buttonClose = Self.transform.Find("Button_Close").GetComponent<Button>();                        
            _buttonClose.onClick.AddListener(ButtonCloseHandler);
            
            _buttonSave = Self.transform.Find("Button_Save").GetComponent<Button>();                        
            _buttonSave.onClick.AddListener(ButtonSaveHandler);
            
            _buttonSelectNone = Self.transform.Find("Button_SelectNone").GetComponent<Button>();                        
            _buttonSelectNone.onClick.AddListener(ButtonSelectNoneHandler);
            
            _buttonSelectAll = Self.transform.Find("Button_SelectAll").GetComponent<Button>();                        
            _buttonSelectAll.onClick.AddListener(ButtonSelectAllHandler);
            
            _buttonElement = Self.transform.Find("Button_Element").GetComponent<Button>();                        
            _buttonElement.onClick.AddListener(ButtonElementHandler);
            
            _buttonRank = Self.transform.Find("Button_Rank").GetComponent<Button>();                        
            _buttonRank.onClick.AddListener(ButtonRankHandler);
            
            _buttonType = Self.transform.Find("Button_Type").GetComponent<Button>();                        
            _buttonType.onClick.AddListener(ButtonTypeHandler);
            
            _buttonGooCost = Self.transform.Find("Button_GooCost").GetComponent<Button>();                        
            _buttonGooCost.onClick.AddListener(ButtonGooCostHandler);

            _buttonElementsDictionary.Clear();
            foreach(Enumerators.Faction faction in AllAvailableFactionList)
            {
                Button buttonElementIcon = Self.transform.Find("Tab_Element/Group_ElementIcons/Button_element_"+faction.ToString().ToLower()).GetComponent<Button>();
                buttonElementIcon.onClick.AddListener
                (()=>
                    {
                        PlayClickSound();
                        ButtonElementIconHandler(faction);
                    }
                );

                _buttonElementsDictionary.Add(faction, buttonElementIcon);
            }

            _buttonRankDictionary.Clear();
            foreach (Enumerators.CardRank rank in AllAvailableRankList)
            {
                Button button = Self.transform.Find("Tab_Rank/Group_RankIcons/Button_rank_"+rank.ToString().ToLower()).GetComponent<Button>();
                button.onClick.AddListener
                (()=>
                    {
                        PlayClickSound();
                        ButtonRankIconHandler(rank);
                    }
                );

                _buttonRankDictionary.Add(rank, button);
            }
            
            _buttonTypeDictionary.Clear();
            foreach (Enumerators.CardType type in AllAvailableTypeList)
            {
                Button button = Self.transform.Find("Tab_Type/Group_TypeIcons/Button_type_"+type.ToString().ToLower()).GetComponent<Button>();
                button.onClick.AddListener
                (()=>
                    {
                        PlayClickSound();
                        ButtonTypeIconHandler(type);
                    }
                );

                _buttonTypeDictionary.Add(type, button);
            }

            _buttonGooCostList.Clear();
            for (int i=0;i<11;++i)
            {
                int gooIndex = i;
                Button button = Self.transform.Find("Tab_GooCost/Group_GooIcons/Button_element_goo_" + i).GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    PlayClickSound();
                    FilterData.GooCostList[gooIndex] = !FilterData.GooCostList[gooIndex];
                    UpdateGooCostButtonDisplay(gooIndex);
                });
                _buttonGooCostList.Add(button);
            }

            _tabObjects = new GameObject[]
            {
                Self.transform.Find("Tab_Element").gameObject,
                Self.transform.Find("Tab_Rank").gameObject,
                Self.transform.Find("Tab_Type").gameObject,
                Self.transform.Find("Tab_GooCost").gameObject
            };

            LoadCacaheFilterData();
            LoadTabs();
            UpdateAllButtonsStatus();
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
            PlayClickSound();
            Hide();
        }
        
        private void ButtonSaveHandler()
        {            
            PlayClickSound();
            if (!CheckIfAnyElementSelected())
            {
                OpenAlertDialog("No element selected!\nPlease select atleast one.");
                return;
            }
            
            if (!CheckIfAnyRankSelected())
            {
                OpenAlertDialog("No rank selected!\nPlease select atleast one.");
                return;
            }
            
            if (!CheckIfAnyTypeSelected())
            {
                OpenAlertDialog("No type selected!\nPlease select atleast one.");
                return;
            }
            
            if (!CheckIfAnyGooCostSelected())
            {
                OpenAlertDialog("No goo cost selected!\nPlease select atleast one.");
                return;
            }
            
            SaveCacheFilterData();
            ActionPopupHiding?.Invoke(FilterData);
            Hide();
        }
        
        private void ButtonElementIconHandler(Enumerators.Faction faction)
        {
            PlayClickSound();
            ToggleSelectedFaction(faction);            
        }

        private void ButtonRankIconHandler(Enumerators.CardRank rank)
        {
            PlayClickSound();
            FilterData.RankDictionary[rank] = !FilterData.RankDictionary[rank];
            UpdateRankButtonDisplay(rank);
        }
        
        private void ButtonTypeIconHandler(Enumerators.CardType type)
        {
            PlayClickSound();
            FilterData.TypeDictionary[type] = !FilterData.TypeDictionary[type];
            UpdateTypeButtonDisplay(type);
        }

        private void ButtonSelectNoneHandler()
        {
            PlayClickSound();
            switch (_tab)
            {
                case TAB.ELEMENT:
                    foreach (Enumerators.Faction faction in AllAvailableFactionList)
                    {
                        SetSelectedFaction(faction, false);
                    }
                    break;
                case TAB.RANK:
                    foreach (Enumerators.CardRank rank in AllAvailableRankList)
                    {
                        FilterData.RankDictionary[rank] = false;
                        UpdateRankButtonDisplay(rank);
                    }
                    break;
                case TAB.TYPE:
                    foreach (Enumerators.CardType type in AllAvailableTypeList)
                    {
                        FilterData.TypeDictionary[type] = false;
                        UpdateTypeButtonDisplay(type);
                    }
                    break;
                case TAB.GOO_COST:
                    for (int i=0; i<FilterData.GooCostList.Count;++i)
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
            PlayClickSound();
            switch (_tab)
            {
                case TAB.ELEMENT:
                    foreach (Enumerators.Faction faction in AllAvailableFactionList)
                    {
                        SetSelectedFaction(faction, true);
                    }
                    break;
                case TAB.RANK:
                    foreach (Enumerators.CardRank rank in AllAvailableRankList)
                    {
                        FilterData.RankDictionary[rank] = true;
                        UpdateRankButtonDisplay(rank);
                    }
                    break;
                case TAB.TYPE:
                    foreach (Enumerators.CardType type in AllAvailableTypeList)
                    {
                        FilterData.TypeDictionary[type] = true;
                        UpdateTypeButtonDisplay(type);
                    }
                    break;
                case TAB.GOO_COST:
                    for (int i=0; i<FilterData.GooCostList.Count;++i)
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
            PlayClickSound();
            ChangeTab(TAB.ELEMENT);
        }
        
        private void ButtonRankHandler()
        {
            PlayClickSound();
            ChangeTab(TAB.RANK);
        }
        
        private void ButtonTypeHandler()
        {
            PlayClickSound();
            ChangeTab(TAB.TYPE);
        }
        
        private void ButtonGooCostHandler()
        {
            PlayClickSound();
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
            
            for (int i=0; i<_tabObjects.Length;++i)
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
        
        private void SaveCacheFilterData()
        {
            _cacheFilterData = new CardFilterData(FilterData);
        }
        
        private void LoadCacaheFilterData()
        {
            if (_cacheFilterData != null)
            {
                FilterData = new CardFilterData(_cacheFilterData);
            }
        }

        #region Filter

        private void SetSelectedFaction(Enumerators.Faction faction, bool status)
        {
            FilterData.FactionDictionary[faction] = status;
            UpdateFactionButtonDisplay(faction);
        }
        
        private void ToggleSelectedFaction(Enumerators.Faction faction)
        {
            FilterData.FactionDictionary[faction] = !FilterData.FactionDictionary[faction];
            UpdateFactionButtonDisplay(faction);
        }
        
        private void UpdateFactionButtonDisplay(Enumerators.Faction faction)
        {
            _buttonElementsDictionary[faction].GetComponent<Image>().color =
                FilterData.FactionDictionary[faction] ? Color.white : Color.gray;
        }
        
        private void UpdateRankButtonDisplay(Enumerators.CardRank rank)
        {
            _buttonRankDictionary[rank].GetComponent<Image>().color =
                FilterData.RankDictionary[rank] ? Color.white : Color.gray;
        }
        
        private void UpdateTypeButtonDisplay(Enumerators.CardType type)
        {
            _buttonTypeDictionary[type].GetComponent<Image>().color =
                FilterData.TypeDictionary[type] ? Color.white : Color.gray;
        }
        
        private void UpdateGooCostButtonDisplay(int gooIndex)
        {
            _buttonGooCostList[gooIndex].GetComponent<Image>().color =
                FilterData.GooCostList[gooIndex] ? Color.white : Color.gray;
            _buttonGooCostList[gooIndex].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = _buttonGooCostList[gooIndex].GetComponent<Image>().color;
        }
        
        private bool CheckIfAnyElementSelected()
        {
            return FilterData.FactionDictionary.Any(kvp => kvp.Value);           
        }
        
        private bool CheckIfAnyGooCostSelected()
        {
            return FilterData.GooCostList.Any(selected => selected);
        }
        
        private bool CheckIfAnyRankSelected()
        {
            return FilterData.RankDictionary.Any(kvp => kvp.Value);           
        }
        
        private bool CheckIfAnyTypeSelected()
        {
            return FilterData.TypeDictionary.Any(kvp => kvp.Value);           
        }
        
        private void UpdateAllButtonsStatus()
        {
            foreach (Enumerators.Faction faction in AllAvailableFactionList)
            {
                UpdateFactionButtonDisplay(faction);
            }
            foreach (Enumerators.CardRank rank in AllAvailableRankList)
            {
                UpdateRankButtonDisplay(rank);
            }
            foreach (Enumerators.CardType type in AllAvailableTypeList)
            {
                UpdateTypeButtonDisplay(type);
            }
            for (int i = 0; i < FilterData.GooCostList.Count; ++i)
            {
                UpdateGooCostButtonDisplay(i);
            }
        }
        
        private void ResetEventSubscriptions()
        {
            ActionPopupHiding = null;
        }

        #endregion

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
            public Dictionary<Enumerators.Faction, bool> FactionDictionary;
            public Dictionary<Enumerators.CardRank, bool> RankDictionary;
            public Dictionary<Enumerators.CardType, bool> TypeDictionary;
            public List<bool> GooCostList;
            
            public CardFilterData(CardFilterData originalData)
            {
                FactionDictionary = originalData.FactionDictionary.ToDictionary
                (
                    entry => entry.Key,
                    entry => entry.Value
                );
                RankDictionary = originalData.RankDictionary.ToDictionary
                (
                    entry => entry.Key,
                    entry => entry.Value
                );
                TypeDictionary = originalData.TypeDictionary.ToDictionary
                (
                    entry => entry.Key,
                    entry => entry.Value
                );
                GooCostList = originalData.GooCostList.ToList();
            }

            public CardFilterData
            (
                List<Enumerators.Faction> availableFactionList,
                List<Enumerators.CardRank> availableRankList,
                List<Enumerators.CardType> availableTypeList
            )
            {
                FactionDictionary = new Dictionary<Enumerators.Faction, bool>();
                foreach(Enumerators.Faction faction in availableFactionList)
                {
                    FactionDictionary.Add(faction, true);
                }

                RankDictionary = new Dictionary<Enumerators.CardRank, bool>();
                foreach(Enumerators.CardRank rank in availableRankList)
                {
                    RankDictionary.Add(rank, true);
                }
                
                TypeDictionary = new Dictionary<Enumerators.CardType, bool>();
                foreach(Enumerators.CardType type in availableTypeList)
                {
                    TypeDictionary.Add(type, true);
                }
                
                GooCostList = new List<bool>();
                for (int i=0; i<11; ++i)
                {
                    GooCostList.Add(true);
                }
            }
            
            public List<Enumerators.Faction> GetFilterFactionList()
            {
                List<Enumerators.Faction> factionList = new List<Enumerators.Faction>();
                foreach (KeyValuePair<Enumerators.Faction, bool> kvp in FactionDictionary)
                {
                    if(kvp.Value)
                        factionList.Add(kvp.Key);
                }
                return factionList;
            }

            public void Reset()
            {
                for (int i=0; i<FactionDictionary.Count;++i)
                {
                    KeyValuePair<Enumerators.Faction, bool> kvp = FactionDictionary.ElementAt(i);
                    FactionDictionary[kvp.Key] = true;
                }
                for (int i=0; i<RankDictionary.Count;++i)
                {
                    KeyValuePair<Enumerators.CardRank, bool> kvp = RankDictionary.ElementAt(i);
                    RankDictionary[kvp.Key] = true;
                }
                for (int i=0; i<TypeDictionary.Count;++i)
                {
                    KeyValuePair<Enumerators.CardType, bool> kvp = TypeDictionary.ElementAt(i);
                    TypeDictionary[kvp.Key] = true;
                }
                for (int i = 0; i < GooCostList.Count; ++i)
                {
                    GooCostList[i] = true;
                }
            }
        }

    }
}