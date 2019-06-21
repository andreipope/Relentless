using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class TutorialAvatarPopup : IUIPopup
    {
        private const int SortingOrderForAboveUI = 33;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private ITutorialManager _tutorialManager;

        private IDataManager _dataManager;

        private TextMeshProUGUI _text;

        private ButtonShiftingContent _hideButton;

        private TextMeshProUGUI _hideButtonText;

        private GameObject _bubbleObject;

        private Image _janeImage;

        private Sprite[] _janePoses;

        private Button _buttonBack;

        private Canvas _selfCanvas;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _janePoses = _loadObjectsManager.GetObjectsByPath<Sprite>(new string[] {
                "Images/Tutorial/1BasicJane",
                "Images/Tutorial/2ThinkingJane",
                "Images/Tutorial/3PointingJane",
                "Images/Tutorial/4ThumbsUpJane",
                "Images/Tutorial/5KissJane"
            });
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
            if (Self != null)
            {
                Hide();
            }

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TutorialAvatarPopup"));

            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _bubbleObject = Self.transform.Find("Description").gameObject;

            _text = Self.transform.Find("Description/Text").GetComponent<TextMeshProUGUI>();

            _hideButton = Self.transform.Find("Button_Ok").GetComponent<ButtonShiftingContent>();

            _hideButtonText = Self.transform.Find("Description/Button_Ok/Text").GetComponent<TextMeshProUGUI>();

            _janeImage = Self.transform.Find("NPC").GetComponent<Image>();

            _selfCanvas = Self.GetComponent<Canvas>();

            _hideButton.onClick.AddListener(HideButtonOnClickHandler);
        }

        public void Show(object data)
        {
            Show();

            if (data is object[] array)
            {
                _text.text = (string)array[0];
                _hideButtonText.text = (string)array[1];
                _janeImage.sprite = _janePoses[(int)((Enumerators.TutorialAvatarPose)array[2])];
                bool aboveUI = (bool)array[3];
                if(aboveUI)
                {
                    _selfCanvas.sortingOrder = SortingOrderForAboveUI;
                }
            }          
        }

        public void Update()
        {
        }

        private void HideButtonOnClickHandler()
        {
            _uiManager.HidePopup<TutorialAvatarPopup>();

            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.AvatarTooltipClosed); 
        }
    }
}
