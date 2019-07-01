using System.Linq;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class CardFilter
    {
        public GameObject Self { get; private set; }

        private IUIManager _uiManager;
        private ITutorialManager _tutorialManager;

        private Button _buttonElement;
        private Button _buttonGooCost;
        private Button _buttonRank;

        private Button _buttonGooCostLeftArrow;
        private Button _buttonGooCostRightArrow;
        private ScrollRect _scrollRectGooCost;

        private Dictionary<Enumerators.Faction, Button> _buttonElementsDictionary;
        private Dictionary<Enumerators.CardRank, Button> _buttonRankDictionary;
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

        public CardFilterData FilterData;

        private CardFilterData _cacheFilterData;

        public UnityAction<Enumerators.Faction> UpdateElementFilterEvent;
        public UnityAction<Enumerators.CardRank> UpdateRankFilterEvent;
        public UnityAction<int> UpdateGooCostFilterEvent;

        public enum Tab
        {
            None = -1,
            Element = 0,
            GooCost = 1,
            Rank = 2

        }

        private Tab _tab = Tab.None;

        private GameObject[] _tabObjects;

        #region IUIPopup

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _buttonElementsDictionary = new Dictionary<Enumerators.Faction, Button>();
            _buttonRankDictionary = new Dictionary<Enumerators.CardRank, Button>();

            FilterData = new CardFilterData
            (
                AllAvailableFactionList,
                AllAvailableRankList
            );
            SaveCacheFilterData();
            _buttonGooCostList = new List<Button>();
        }

        public void Hide()
        {
            SaveCacheFilterData();
            _buttonElementsDictionary.Clear();
            _buttonGooCostList.Clear();
            _buttonRankDictionary.Clear();
        }

        public void Show(GameObject obj)
        {
            Self = obj;
            _buttonElement = Self.transform.Find("FiltersButtons/Button_Element").GetComponent<Button>();
            _buttonElement.onClick.AddListener(ButtonElementHandler);

            _buttonGooCost = Self.transform.Find("FiltersButtons/Button_GooCost").GetComponent<Button>();
            _buttonGooCost.onClick.AddListener(ButtonGooCostHandler);

            _buttonRank = Self.transform.Find("FiltersButtons/Button_Rarity").GetComponent<Button>();
            _buttonRank.onClick.AddListener(ButtonRankHandler);

            _buttonGooCostLeftArrow = Self.transform.Find("Tab_GooCost/Goo/Left_Arrow_Button").GetComponent<Button>();
            _buttonGooCostLeftArrow.onClick.AddListener(ButtonGooCostLeftArrowHandler);

            _buttonGooCostRightArrow = Self.transform.Find("Tab_GooCost/Goo/Right_Arrow_Button").GetComponent<Button>();
            _buttonGooCostRightArrow.onClick.AddListener(ButtonGooCostRightArrowHandler);

            _scrollRectGooCost = Self.transform.Find("Tab_GooCost/Goo/Scroll View").GetComponent<ScrollRect>();

            _buttonElementsDictionary.Clear();
            foreach(Enumerators.Faction faction in AllAvailableFactionList)
            {
                Button buttonElementIcon = Self.transform.Find("Tab_Element/Elements/Scroll View/Viewport/Content/"+faction.ToString().ToLower()).GetComponent<Button>();
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
                Button button = Self.transform.Find("Tab_Rarity/Rarity/Scroll View/Viewport/Content/"+rank.ToString().ToLower()).GetComponent<Button>();
                button.onClick.AddListener
                (()=>
                    {
                        PlayClickSound();
                        ButtonRankIconHandler(rank);
                    }
                );

                _buttonRankDictionary.Add(rank, button);
            }

            _buttonGooCostList.Clear();
            for (int i = 0;i < 11;++i)
            {
                int gooIndex = i;
                int pageIndex = i < 6 ? 1 : 2;
                Button button = Self.transform.Find("Tab_GooCost/Goo/Scroll View/Viewport/Content/Goo_page_"+pageIndex+"/goo_" + i).GetChild(0).GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    PlayClickSound();

                    if (FilterData.GooCostList[gooIndex] && FilterData.GooCostList.FindAll(gooBottle => gooBottle).Count <= 1)
                    {
                        OpenAlertDialog("At least one goo cost should be selected.");
                        return;
                    }


                    FilterData.GooCostList[gooIndex] = !FilterData.GooCostList[gooIndex];
                    UpdateGooCostButtonDisplay(gooIndex);
                    UpdateGooCostFilterEvent?.Invoke(gooIndex);
                });
                _buttonGooCostList.Add(button);
            }

            _tabObjects = new GameObject[]
            {
                Self.transform.Find("Tab_Element").gameObject,
                Self.transform.Find("Tab_GooCost").gameObject,
                Self.transform.Find("Tab_Rarity").gameObject
            };


            LoadCacaheFilterData();
            LoadTabs();
            UpdateAllButtonsStatus();
        }

        #endregion

        #region Buttons Handlers
        private void ButtonGooCostLeftArrowHandler()
        {
            PlayClickSound();
            _scrollRectGooCost.horizontalNormalizedPosition = 0;

            _buttonGooCostLeftArrow.interactable = false;
            _buttonGooCostRightArrow.interactable = true;
        }

        private void ButtonGooCostRightArrowHandler()
        {
            PlayClickSound();
            _scrollRectGooCost.horizontalNormalizedPosition = 1;

            _buttonGooCostLeftArrow.interactable = true;
            _buttonGooCostRightArrow.interactable = false;
        }

        private void ButtonElementIconHandler(Enumerators.Faction faction)
        {
            if (_tutorialManager.IsTutorial)
                return;

            PlayClickSound();

            if (FilterData.FactionDictionary[faction] && FilterData.GetActiveElementFilterCount() <= 1)
            {
                OpenAlertDialog("At least one element should be selected.");
                return;
            }

            ToggleSelectedFaction(faction);
            UpdateElementFilterEvent?.Invoke(faction);
        }

        private void ButtonRankIconHandler(Enumerators.CardRank rank)
        {
            PlayClickSound();

            if (FilterData.RankDictionary[rank] && FilterData.GetActiveRankFilterCount() <= 1)
            {
                OpenAlertDialog("At least one rank should be selected.");
                return;
            }

            FilterData.RankDictionary[rank] = !FilterData.RankDictionary[rank];
            UpdateRankButtonDisplay(rank);
            UpdateRankFilterEvent?.Invoke(rank);
        }

        private void ButtonElementHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonElement.name))
                return;

            PlayClickSound();
            ChangeTab(Tab.Element);
        }

        private void ButtonRankHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonRank.name))
                return;

            PlayClickSound();
            ChangeTab(Tab.Rank);
        }

        private void ButtonGooCostHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonGooCost.name))
                return;

            PlayClickSound();
            ChangeTab(Tab.GooCost);
        }

        #endregion

        private void LoadTabs()
        {
            _tab = Tab.None;
            ChangeTab(Tab.Element);
        }

        public void ChangeTab(Tab newTab)
        {
            if (newTab == _tab)
                return;

            switch (_tab)
            {
                case Tab.Element:
                    _buttonElement.GetComponent<Image>().sprite = _buttonElement.spriteState.disabledSprite;
                    break;
                case Tab.Rank:
                    _buttonRank.GetComponent<Image>().sprite = _buttonRank.spriteState.disabledSprite;
                    break;
                case Tab.GooCost:
                    _buttonGooCost.GetComponent<Image>().sprite = _buttonGooCost.spriteState.disabledSprite;
                    break;
            }

            _tab = newTab;

            for (int i = 0; i < _tabObjects.Length;++i)
            {
                GameObject tabObject = _tabObjects[i];
                tabObject.SetActive(i == (int)newTab);
            }

            switch (newTab)
            {
                case Tab.None:
                    break;
                case Tab.Element:
                    _buttonElement.GetComponent<Image>().sprite = _buttonElement.spriteState.pressedSprite;
                    break;
                case Tab.Rank:
                    _buttonRank.GetComponent<Image>().sprite = _buttonRank.spriteState.pressedSprite;
                    break;
                case Tab.GooCost:
                    _buttonGooCost.GetComponent<Image>().sprite = _buttonGooCost.spriteState.pressedSprite;
                    break;
            }
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
            Button button = _buttonElementsDictionary[faction].GetComponent<Button>();
            button.GetComponent<Image>().color = FilterData.FactionDictionary[faction] ? Color.white : Color.gray;
        }

        private void UpdateRankButtonDisplay(Enumerators.CardRank rank)
        {
            Button button = _buttonRankDictionary[rank].GetComponent<Button>();
            button.GetComponent<Image>().color = FilterData.RankDictionary[rank] ? Color.white : Color.gray;
        }

        private void UpdateGooCostButtonDisplay(int gooIndex)
        {
            Button button = _buttonGooCostList[gooIndex].GetComponent<Button>();
            TextMeshProUGUI text = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            Color color = FilterData.GooCostList[gooIndex] ? Color.white : Color.gray;
            button.GetComponent<Image>().color = color;
            text.color = color;
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
            for (int i = 0; i < FilterData.GooCostList.Count; ++i)
            {
                UpdateGooCostButtonDisplay(i);
            }

            _buttonGooCostLeftArrow.interactable = false;
            _buttonGooCostRightArrow.interactable = true;
        }

        #endregion

        private void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        private void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        public class CardFilterData
        {
            public Dictionary<Enumerators.Faction, bool> FactionDictionary;
            public Dictionary<Enumerators.CardRank, bool> RankDictionary;
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
               GooCostList = originalData.GooCostList.ToList();
            }

            public CardFilterData
            (
                List<Enumerators.Faction> availableFactionList,
                List<Enumerators.CardRank> availableRankList
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

                GooCostList = new List<bool>();
                for (int i = 0; i < 11; ++i)
                {
                    GooCostList.Add(true);
                }
            }

            public List<Enumerators.Faction> GetFilteredFactionList()
            {
                List<Enumerators.Faction> factionList = new List<Enumerators.Faction>();
                foreach (KeyValuePair<Enumerators.Faction, bool> kvp in FactionDictionary)
                {
                    if(kvp.Value)
                        factionList.Add(kvp.Key);
                }
                return factionList;
            }

            public int GetActiveElementFilterCount()
            {
                int count = 0;
                foreach (KeyValuePair<Enumerators.Faction, bool> kvp in FactionDictionary)
                {
                    if (kvp.Value)
                        count++;
                }
                return count;
            }

            public int GetActiveRankFilterCount()
            {
                int count = 0;
                foreach (KeyValuePair<Enumerators.CardRank, bool> kvp in RankDictionary)
                {
                    if (kvp.Value)
                        count++;
                }
                return count;
            }

            public List<Enumerators.CardRank> GetFilteredRankList()
            {
                List<Enumerators.CardRank> rankList = new List<Enumerators.CardRank>();
                foreach (KeyValuePair<Enumerators.CardRank, bool> kvp in RankDictionary)
                {
                    if(kvp.Value)
                        rankList.Add(kvp.Key);
                }
                return rankList;
            }


            public List<int> GetGooCostList()
            {
                var gooCostList = new List<int>();
                for (int i = 0; i < GooCostList.Count; i++)
                {
                    if(GooCostList[i])
                        gooCostList.Add(i);
                }
                return gooCostList;
            }

            public void Reset()
            {
                for (int i = 0; i < FactionDictionary.Count;++i)
                {
                    KeyValuePair<Enumerators.Faction, bool> kvp = FactionDictionary.ElementAt(i);
                    FactionDictionary[kvp.Key] = true;
                }
                for (int i = 0; i < RankDictionary.Count;++i)
                {
                    KeyValuePair<Enumerators.CardRank, bool> kvp = RankDictionary.ElementAt(i);
                    RankDictionary[kvp.Key] = true;
                }
                for (int i = 0; i < GooCostList.Count; ++i)
                {
                    GooCostList[i] = true;
                }
            }
        }

    }
}
