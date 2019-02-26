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
    public class CardFilterPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        
        public event Action ActionPopupHiding;
        
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private Button _buttonClose,
                       _buttonSave;

        private Dictionary<Enumerators.SetType, Button> _buttonElementsDictionary;
        
        private readonly List<Enumerators.SetType> _availableSetTypeList = new List<Enumerators.SetType>()
        {
            Enumerators.SetType.AIR,
            Enumerators.SetType.EARTH,
            Enumerators.SetType.LIFE,
            Enumerators.SetType.FIRE,
            Enumerators.SetType.TOXIC,
            Enumerators.SetType.WATER,
            Enumerators.SetType.OTHERS
        };

        private Dictionary<Enumerators.SetType, bool> _selectSetTypeList;

        #region IUIPopup

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _buttonElementsDictionary = new Dictionary<Enumerators.SetType, Button>();
            _selectSetTypeList = new Dictionary<Enumerators.SetType, bool>();            
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

            _selectSetTypeList.Clear();
            foreach(Enumerators.SetType setType in _availableSetTypeList)
            {
                _selectSetTypeList.Add(setType, true);
            }

            UpdateSelectedSetType(Enumerators.SetType.NONE);      
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
            _uiManager.HidePopup<CardFilterPopup>();
            ActionPopupHiding?.Invoke();
        }
        
        private void ButtonElementIconHandler(Enumerators.SetType setType)
        {
            UpdateSelectedSetType(setType);            
        }

        #endregion
        
        private void UpdateSelectedSetType(Enumerators.SetType setType)
        {
            _selectSetTypeList[setType] = !_selectSetTypeList[setType];
            _buttonElementsDictionary[setType].GetComponent<Image>().color =
                _selectSetTypeList[setType] ? Color.white : Color.gray;
        }

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }        
           
    }
}