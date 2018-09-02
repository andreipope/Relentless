using System;
using LoomNetwork.CZB.Common;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class ConfirmationPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private TextMeshProUGUI _text;

        private ButtonShiftingContent _cancelButton, _confirmButton;

        private Action _callback;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            // Time.timeScale = 1;
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
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/ConfirmationPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _cancelButton = Self.transform.Find("Button_No").GetComponent<ButtonShiftingContent>();
            _confirmButton = Self.transform.Find("Button_Yes").GetComponent<ButtonShiftingContent>();

            _confirmButton.onClick.AddListener(ConfirmButtonOnClickHandler);
            _cancelButton.onClick.AddListener(CancelButtonOnClickHandler);

            _text = Self.transform.Find("Text_Message").GetComponent<TextMeshProUGUI>();
        }

        public void Show(object data)
        {
            Show();

            // _text.text = (string)data;
            _callback = (Action)data;
        }

        public void Update()
        {
        }

        private void ConfirmButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _callback?.Invoke();
            _callback = null;
            Hide();
        }

        private void CancelButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            Hide();
        }
    }
}
