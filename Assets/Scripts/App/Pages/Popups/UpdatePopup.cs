using System;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class UpdatePopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IDataManager _dataManager;

        private ButtonShiftingContent _quitButton, _updateButton;

        private Action _callbackOnUpdate, _callbackOnQuit;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
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
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/UpdatePopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _quitButton = Self.transform.Find("Button_Quit").GetComponent<ButtonShiftingContent>();
            _updateButton = Self.transform.Find("Button_Update").GetComponent<ButtonShiftingContent>();

            _updateButton.onClick.AddListener(UpdateButtonOnClickHandler);
            _quitButton.onClick.AddListener(QuitButtonOnClickHandler);
        }

        public void Show(object data)
        {
            Show();
            Action[] actions = (Action[]) data;
            _callbackOnUpdate = actions[0];
            _callbackOnQuit = actions[1];
        }

        public void Update()
        {
            if (Self != null && Self.activeInHierarchy)
            {
                _updateButton.interactable = _dataManager.ZbVersion != null;
            }
        }

        private void UpdateButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _callbackOnUpdate?.Invoke();
            _callbackOnUpdate = null;
        }

        private void QuitButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _callbackOnQuit?.Invoke();
            _callbackOnQuit = null;
        }
    }
}
