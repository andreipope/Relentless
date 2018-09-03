using LoomNetwork.CZB.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class TermsPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IDataManager _dataManager;

        private TextMeshProUGUI _text, _titleText;

        private ButtonShiftingContent _gotItButton;

        private Toggle _toggle;

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
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TermsPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _gotItButton = Self.transform.Find("Button_GotIt").GetComponent<ButtonShiftingContent>();
            _gotItButton.onClick.AddListener(CloseButtonHandler);

            _toggle = Self.transform.Find("Toggle").GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(ToggleValueChanged);

            _text = Self.transform.Find("Message").GetComponent<TextMeshProUGUI>();
            _titleText = Self.transform.Find("Title").GetComponent<TextMeshProUGUI>();

            _gotItButton.gameObject.SetActive(false);

            _titleText.text = "UPDATE ver. " + BuildMetaInfo.Instance.DisplayVersionName;
        }

        public void Show(object data)
        {
            Show();

            _text.text = (string) data;
        }

        public void Update()
        {
        }

        public void CloseButtonHandler()
        {
            _dataManager.CachedUserLocalData.AgreedTerms = true;
            _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            Hide();
        }

        private void ToggleValueChanged(bool change)
        {
            if (_toggle.isOn)
            {
                _gotItButton.gameObject.SetActive(true);
            }
            else
            {
                _gotItButton.gameObject.SetActive(false);
            }
        }
    }
}
