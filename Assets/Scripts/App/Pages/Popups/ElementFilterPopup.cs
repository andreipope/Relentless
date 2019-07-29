using System;
using System.Linq;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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

        private List<Enumerators.Faction> _tempSelectedFactionList;

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
            _tempSelectedFactionList = new List<Enumerators.Faction>();
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

            _tempSelectedFactionList = SelectedFactionList.ToList();

            foreach(Enumerators.Faction faction in AvailableFactionList)
            {
                Button buttonElementIcon = Self.transform.Find("Group_ElementIcons/Button_element_"+faction.ToString().ToLowerInvariant()).GetComponent<Button>();
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
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        #endregion

        public void ResetSelectedFactionList()
        {
            SelectedFactionList.Clear();
            foreach(Enumerators.Faction faction in AvailableFactionList)
            {
                SelectedFactionList.Add(faction);
            }
        }

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
            SaveSelectedFactions();
            ActionPopupHiding?.Invoke();
        }

        private void ButtonElementIconHandler(Enumerators.Faction faction)
        {
            SetSelectedFaction
            (
                faction,
                !_tempSelectedFactionList.Contains(faction)
            );
        }

        #endregion

        private void SetSelectedFaction(Enumerators.Faction faction, bool status)
        {
            if(status)
            {
                if(!_tempSelectedFactionList.Contains(faction))
                {
                    _tempSelectedFactionList.Add(faction);
                }
            }
            else
            {
                if(_tempSelectedFactionList.Contains(faction))
                {
                    _tempSelectedFactionList.Remove(faction);
                }
            }
            UpdateFactionButtonDisplay(faction);
        }

        private void UpdateFactionButtonDisplay(Enumerators.Faction faction)
        {
            _buttonElementsDictionary[faction].GetComponent<Image>().color =
                _tempSelectedFactionList.Contains(faction) ? Color.white : Color.gray;
        }

        private void SaveSelectedFactions()
        {
            SelectedFactionList = _tempSelectedFactionList.ToList();
        }

        private void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

    }
}
