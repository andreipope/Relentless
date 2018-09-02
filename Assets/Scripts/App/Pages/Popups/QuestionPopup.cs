using System;
using LoomNetwork.CZB.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class QuestionPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private Button _backButton;

        private TextMeshProUGUI _text;

        // private MenuButton _button1,
        // _button2;
        private ButtonShiftingContent // _closeButton,
            _buttonYes, _buttonNo;

        private TextMeshProUGUI _buttonText;

        public event Action<bool> ConfirmationEvent;

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
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/QuestionPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _buttonYes = Self.transform.Find("Button_Yes").GetComponent<ButtonShiftingContent>();
            _buttonNo = Self.transform.Find("Button_No").GetComponent<ButtonShiftingContent>();
            _backButton = Self.transform.Find("Button_Back").GetComponent<Button>();

            // _closeButton = _selfPage.transform.Find("CloseButton").GetComponent<MenuButtonNoGlow>();

            // _closeButton.onClickEvent.AddListener(Hide);
            _buttonYes.onClick.AddListener(ConfirmationButtonHandler);
            _buttonNo.onClick.AddListener(NoButtonOnClickHandler);
            _backButton.onClick.AddListener(BackButtonHandler);

            _text = Self.transform.Find("Text_Message").GetComponent<TextMeshProUGUI>();
        }

        public void Show(object data)
        {
            Show();

            if (data is object[])
            {
                object[] param = (object[])data;
                _text.text = (string)param[0];
                _backButton.gameObject.SetActive((bool)param[1]);
            }
            else
            {
                _backButton.gameObject.SetActive(false);
                _text.text = (string)data;
            }
        }

        public void Update()
        {
        }

        private void ConfirmationButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            ConfirmationEvent?.Invoke(true);

            Hide();
        }

        private void NoButtonOnClickHandler()
        {
            ConfirmationEvent?.Invoke(false);
            _uiManager.HidePopup<QuestionPopup>();
        }

        private void BackButtonHandler()
        {
            ConfirmationEvent = null;
            Hide();
        }
    }
}
