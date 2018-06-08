using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GrandDevs.CZB
{
    public class ConfirmationPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage;

        private TextMeshProUGUI _text;

        private MenuButtonNoGlow _cancelButton,
                       _confirmButton;

        private Action _callback;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/ConfirmationPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas3.transform, false);

            _cancelButton = _selfPage.transform.Find("Button_No").GetComponent<MenuButtonNoGlow>();
            _confirmButton = _selfPage.transform.Find("Button_Yes").GetComponent<MenuButtonNoGlow>();

            _confirmButton.onClickEvent.AddListener(ConfirmButtonOnClickHandler);
            _cancelButton.onClickEvent.AddListener(CancelButtonOnClickHandler);

            _text = _selfPage.transform.Find("Text_Message").GetComponent<TextMeshProUGUI>();

            Hide();
        }


        public void Dispose()
        {
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
            //Time.timeScale = 1;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            _selfPage.SetActive(true);
            //Time.timeScale = 0;
        }

        public void Show(object data)
        {
            //_text.text = (string)data;
            _callback = (Action)data;

            Show();
        }

        public void Update()
        {

        }

        private void ConfirmButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _callback?.Invoke();
            _callback = null;
            Hide();
        }

        private void CancelButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            Hide();
        }
    }
}