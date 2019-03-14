using System;
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
        
        public event Action<Enumerators.Faction> ActionPopupHiding;
        
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private Button _buttonClose,
                       _buttonSave;

        private Image _imageGlow;

        private Dictionary<Enumerators.Faction, Button> _buttonElementsDictionary;

        private readonly List<Enumerators.Faction> _availableFactionList = new List<Enumerators.Faction>()
        {
            Enumerators.Faction.AIR,
            Enumerators.Faction.EARTH,
            Enumerators.Faction.LIFE,
            Enumerators.Faction.FIRE,
            Enumerators.Faction.TOXIC,
            Enumerators.Faction.WATER
        };

        private Enumerators.Faction _selectedFaction,
                                    _cacheSelectedFaction;

        private const Enumerators.Faction DefaultSelectedFaction = Enumerators.Faction.AIR;

        #region IUIPopup

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _buttonElementsDictionary = new Dictionary<Enumerators.Faction, Button>();

            _selectedFaction = DefaultSelectedFaction;
            _cacheSelectedFaction = DefaultSelectedFaction;
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
            
            _buttonClose = Self.transform.Find("Button_Close").GetComponent<Button>();                        
            _buttonClose.onClick.AddListener(ButtonCloseHandler);
            _buttonClose.onClick.AddListener(PlayClickSound);
            
            _buttonSave = Self.transform.Find("Button_Save").GetComponent<Button>();                        
            _buttonSave.onClick.AddListener(ButtonSaveHandler);
            _buttonSave.onClick.AddListener(PlayClickSound);

            _buttonElementsDictionary.Clear();
            foreach(Enumerators.Faction faction in _availableFactionList)
            {
                Button buttonElementIcon = Self.transform.Find("Group_ElementIcons/Button_element_"+faction.ToString().ToLower()).GetComponent<Button>();
                buttonElementIcon.onClick.AddListener
                (
                    ()=> ButtonElementIconHandler(faction)
                );
                buttonElementIcon.onClick.AddListener(PlayClickSound);

                _buttonElementsDictionary.Add(faction, buttonElementIcon);
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
            Hide();
        }
        
        private void ButtonSaveHandler()
        {
            Hide();
            SaveCache();
            ActionPopupHiding?.Invoke(_selectedFaction);
        }
        
        private void ButtonElementIconHandler(Enumerators.Faction faction)
        {
            UpdateSelectedFaction(faction);            
        }

        #endregion
        
        private void UpdateSelectedFaction(Enumerators.Faction faction)
        {
            _selectedFaction = faction;
            _imageGlow.transform.position = _buttonElementsDictionary[faction].transform.position;
        }
        
        private void SaveCache()
        {
            _cacheSelectedFaction = _selectedFaction;
        }
        
        private void LoadCache()
        {
            UpdateSelectedFaction(_cacheSelectedFaction);   
        }

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }        
           
    }
}