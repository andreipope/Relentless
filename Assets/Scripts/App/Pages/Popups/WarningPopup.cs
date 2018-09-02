using System;
using LoomNetwork.CZB.Common;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class WarningPopup : IUIPopup
    {
        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private TextMeshProUGUI _text;

        // private MenuButton _button;
        private ButtonShiftingContent _gotItButton;

        public GameObject Self { get; private set; }

        // private TextMeshProUGUI _buttonText;
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
            OnHidePopupEvent?.Invoke();

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
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/WarningPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            // _button = _selfPage.transform.Find("Button").GetComponent<MenuButton>();
            _gotItButton = Self.transform.Find("Button_GotIt").GetComponent<ButtonShiftingContent>();

            // _button.onClickEvent.AddListener(Hide);
            _gotItButton.onClick.AddListener(CloseButtonHandler);

            _text = Self.transform.Find("Text_Message").GetComponent<TextMeshProUGUI>();
        }

        public void Show(object data)
        {
            Show();

            _text.text = (string)data;
        }

        public void Update()
        {
        }

        public void CloseButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            Hide();
        }
    }
}
