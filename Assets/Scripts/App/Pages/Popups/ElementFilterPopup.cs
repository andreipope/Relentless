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

        private readonly List<Enumerators.Faction> _availableSetTypeList = new List<Enumerators.Faction>()
        {
            Enumerators.Faction.AIR,
            Enumerators.Faction.EARTH,
            Enumerators.Faction.LIFE,
            Enumerators.Faction.FIRE,
            Enumerators.Faction.TOXIC,
            Enumerators.Faction.WATER
        };

        private Enumerators.Faction _selectedSetType;

        #region IUIPopup

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _buttonElementsDictionary = new Dictionary<Enumerators.Faction, Button>();
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
            
            _imageGlow = Self.transform.Find("Scaler/Image_glow").GetComponent<Image>();  
            
            _buttonClose = Self.transform.Find("Scaler/Button_Close").GetComponent<Button>();                        
            _buttonClose.onClick.AddListener(ButtonCloseHandler);
            _buttonClose.onClick.AddListener(PlayClickSound);
            
            _buttonSave = Self.transform.Find("Scaler/Button_Save").GetComponent<Button>();                        
            _buttonSave.onClick.AddListener(ButtonSaveHandler);
            _buttonSave.onClick.AddListener(PlayClickSound);

            _buttonElementsDictionary.Clear();
            foreach(Enumerators.Faction faction in _availableSetTypeList)
            {
                Button buttonElementIcon = Self.transform.Find("Scaler/Group_ElementIcons/Button_element_"+faction.ToString().ToLower()).GetComponent<Button>();
                buttonElementIcon.onClick.AddListener
                (
                    ()=> ButtonElementIconHandler(faction)
                );
                buttonElementIcon.onClick.AddListener(PlayClickSound);

                _buttonElementsDictionary.Add(faction, buttonElementIcon);
            }

            UpdateSelectedSetType(Enumerators.Faction.AIR);      
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
            _uiManager.HidePopup<ElementFilterPopup>();
        }
        
        private void ButtonSaveHandler()
        {
            _uiManager.HidePopup<ElementFilterPopup>();
            ActionPopupHiding?.Invoke(_selectedSetType);
        }
        
        private void ButtonElementIconHandler(Enumerators.Faction faction)
        {
            UpdateSelectedSetType(faction);            
        }

        #endregion
        
        private void UpdateSelectedSetType(Enumerators.Faction faction)
        {
            _selectedSetType = faction;
            _imageGlow.gameObject.SetActive(true);
            _imageGlow.transform.position = _buttonElementsDictionary[faction].transform.position;
        }

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }        
           
    }
}
