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
    public class ElementFilterPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        
        public event Action ActionPopupHiding;
        
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private Button _buttonClose,
                       _buttonSave;

        private Image _imageGlow;

        private Dictionary<Enumerators.Faction, Button> _buttonElementsDictionary;

        public readonly List<Enumerators.Faction> AvailableFactionList = new List<Enumerators.Faction>()
        {
            Enumerators.Faction.AIR,
            Enumerators.Faction.EARTH,
            Enumerators.Faction.LIFE,
            Enumerators.Faction.FIRE,
            Enumerators.Faction.TOXIC,
            Enumerators.Faction.WATER
        };

        public List<Enumerators.Faction> SelectedFactionList;
                                    
        private List<Enumerators.Faction> _cacheSelectedFactionList;

        #region IUIPopup

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _buttonElementsDictionary = new Dictionary<Enumerators.Faction, Button>();

            SelectedFactionList = new List<Enumerators.Faction>();
            foreach(Enumerators.Faction faction in AvailableFactionList)
            {
                SelectedFactionList.Add(faction);
            }
            _cacheSelectedFactionList = SelectedFactionList.ToList();
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
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/ElementFilterPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);
            
            _imageGlow = Self.transform.Find("Image_glow").GetComponent<Image>();
            _imageGlow.gameObject.SetActive(false);
            
            _buttonClose = Self.transform.Find("Button_Close").GetComponent<Button>();                        
            _buttonClose.onClick.AddListener(ButtonCloseHandler);
            
            _buttonSave = Self.transform.Find("Button_Save").GetComponent<Button>();                        
            _buttonSave.onClick.AddListener(ButtonSaveHandler);

            _buttonElementsDictionary.Clear();
            foreach(Enumerators.Faction faction in AvailableFactionList)
            {
                Button buttonElementIcon = Self.transform.Find("Group_ElementIcons/Button_element_"+faction.ToString().ToLower()).GetComponent<Button>();
                buttonElementIcon.onClick.AddListener
                (()=> 
                    {
                        PlayClickSound();
                        ButtonElementIconHandler(faction); 
                    }
                );

                _buttonElementsDictionary.Add(faction, buttonElementIcon);
                UpdateFactionButtonDisplay(faction);
            }

            LoadCache();   
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
            Hide();
            SaveCache();
            ActionPopupHiding?.Invoke();
        }
        
        private void ButtonElementIconHandler(Enumerators.Faction faction)
        {
            SetSelectedFaction
            (
                faction,
                !SelectedFactionList.Contains(faction)
            );          
        }

        #endregion
        
        private void SetSelectedFaction(Enumerators.Faction faction, bool status)
        {
            if(status)
            {
                if(!SelectedFactionList.Contains(faction))
                {
                    SelectedFactionList.Add(faction);
                }
            }       
            else
            {
                if(SelectedFactionList.Contains(faction))
                {
                    SelectedFactionList.Remove(faction);
                }
            }
            UpdateFactionButtonDisplay(faction);
        }
        
        private void UpdateFactionButtonDisplay(Enumerators.Faction faction)
        {
            _buttonElementsDictionary[faction].GetComponent<Image>().color =
                SelectedFactionList.Contains(faction) ? Color.white : Color.gray;
        }
        
        private void SaveCache()
        {
            _cacheSelectedFactionList = SelectedFactionList.ToList();
        }
        
        private void LoadCache()
        {
            SelectedFactionList = _cacheSelectedFactionList.ToList();
        }

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }        
           
    }
}