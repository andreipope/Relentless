using System;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class QuestionPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private ILocalizationManager _localizationManager;

        private IUIManager _uiManager;

        private Button _backButton;

        private TextMeshProUGUI _text;

        private TextMeshProUGUI _buttonYesTextMesh;

        private TextMeshProUGUI _buttonNoTextMesh;

        private ButtonShiftingContent _buttonYes, _buttonNo;

        public event Action<bool> ConfirmationReceived;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
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
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _buttonYes = Self.transform.Find("Button_Yes").GetComponent<ButtonShiftingContent>();
            _buttonNo = Self.transform.Find("Button_No").GetComponent<ButtonShiftingContent>();
            _backButton = Self.transform.Find("Button_Back").GetComponent<Button>();
            _buttonYesTextMesh = _buttonYes.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _buttonNoTextMesh = _buttonNo.transform.Find("Text").GetComponent<TextMeshProUGUI>();

            _buttonYes.onClick.AddListener(ConfirmationButtonHandler);
            _buttonNo.onClick.AddListener(NoButtonOnClickHandler);
            _backButton.onClick.AddListener(BackButtonHandler);

            _text = Self.transform.Find("Text_Message").GetComponent<TextMeshProUGUI>();

            UpdateLocalization();
        }

        public void Show(object data)
        {
            Show();

            if (data is object[])
            {
                object[] param = (object[]) data;
                _text.text = (string) param[0];
                _backButton.gameObject.SetActive((bool) param[1]);
            }
            else
            {
                _backButton.gameObject.SetActive(false);
                _text.text = (string) data;
            }
        }

        public void Update()
        {
        }

        private void ConfirmationButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            ConfirmationReceived?.Invoke(true);

            Hide();
        }

        private void UpdateLocalization()
        {
            if (Self == null)
                return;

            _buttonYesTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.YesText.ToString());
            _buttonNoTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.NoText.ToString());
        }

        private void NoButtonOnClickHandler()
        {
            ConfirmationReceived?.Invoke(false);
            _uiManager.HidePopup<QuestionPopup>();
        }

        private void BackButtonHandler()
        {
            ConfirmationReceived = null;
            Hide();
        }
    }
}
