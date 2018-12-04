using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class TutorialPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private ILocalizationManager _localizationManager;

        private IUIManager _uiManager;

        private ITutorialManager _tutorialManager;

        private IDataManager _dataManager;

        private TextMeshProUGUI _text;

        private TextMeshProUGUI _nextButtonTextMesh;

        private TextMeshProUGUI _skipTutorialTextMesh;

        private ButtonShiftingContent _nextButton, _playButton, _skipButton;

        private GameObject _bubbleObject;

        private Image _janeImage;

        private List<GameObject> _focusObjects;

        private Sprite[] _janePoses;

        private Button _buttonBack;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();

            _janePoses = _loadObjectsManager.GetObjectsByPath<Sprite>(new string[] {
                "Images/Tutorial/1BasicJane",
                "Images/Tutorial/2ThinkingJane",
                "Images/Tutorial/3PointingJane",
                "Images/Tutorial/4ThumbsUpJane",
                "Images/Tutorial/5KissJane"
            });

            _focusObjects = new List<GameObject>();
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
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TutorialPopup"));

            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _bubbleObject = Self.transform.Find("Description").gameObject;

            _text = Self.transform.Find("Description/Text").GetComponent<TextMeshProUGUI>();

            _nextButton = Self.transform.Find("Button_Next").GetComponent<ButtonShiftingContent>();
            _playButton = Self.transform.Find("Button_Play").GetComponent<ButtonShiftingContent>();
            _skipButton = Self.transform.Find("Button_Skip").GetComponent<ButtonShiftingContent>();
            _buttonBack = Self.transform.Find("Button_Back").GetComponent<Button>();
            _nextButtonTextMesh = _nextButton.transform.Find("Button/Text").GetComponent<TextMeshProUGUI>();
            _skipTutorialTextMesh = _skipButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();

            _janeImage = Self.transform.Find("NPC").GetComponent<Image>();

            _nextButton.onClick.AddListener(_tutorialManager.NextButtonClickHandler);
            _playButton.onClick.AddListener(_tutorialManager.NextButtonClickHandler);
            _skipButton.onClick.AddListener(SkipButtonOnClickHandler);
            _buttonBack.onClick.AddListener(BackButtonOnClickHandler);

            _focusObjects.Clear();

            foreach (Transform obj in Self.transform.Find("FocusObjects_" + _tutorialManager.CurrentTutorial.TutorialId).transform)
            {
                _focusObjects.Add(obj.gameObject);
            }

            _nextButton.gameObject.SetActive(false);
            _playButton.gameObject.SetActive(false);
            _skipButton.gameObject.SetActive(true);

            UpdateLocalization();
        }

        public void Show(object data)
        {
            Show();

            if (_tutorialManager.CurrentTutorialDataStep.IsManuallyHideBubble)
            {
                _bubbleObject.SetActive(false);
                _tutorialManager.IsBubbleShow = false;
                GameClient.Get<ITimerManager>().AddTimer(ShowBubble, null, 6f);
            }

            _text.text = (string)data;
        }

        public void Update()
        {
        }

        public void UpdatePose(Enumerators.TutorialJanePoses pose)
        {
            _janeImage.sprite = _janePoses[(int)pose];
        }

        public void ShowBubble(object[] param)
        {
            _bubbleObject.SetActive(true);
            _tutorialManager.IsBubbleShow = true;
        }

        public void ShowTutorialFocus(int step)
        {
            HideTutorialFocus();
            Self.transform.Find("FocusObjects_" + _tutorialManager.CurrentTutorial.TutorialId + "/Step_" + step).gameObject.SetActive(true);
        }

        public void HideTutorialFocus()
        {
            foreach (GameObject obj in _focusObjects)
            {
                if (obj.activeSelf)
                {
                    obj.SetActive(false);
                }
            }

            _nextButton.gameObject.SetActive(false);
            _playButton.gameObject.SetActive(false);
        }

        public void ShowNextButton()
        {
            _nextButton.gameObject.SetActive(true);
        }

        public void ShowQuestion()
        {
            _playButton.gameObject.SetActive(true);
        }

        private void SkipButtonOnClickHandler()
        {
            _tutorialManager.SkipTutorial(Enumerators.AppState.HordeSelection);
        }

        private void BackButtonOnClickHandler()
        {
            _tutorialManager.SkipTutorial(Enumerators.AppState.MAIN_MENU);
        }

        private void UpdateLocalization()
        {
            if (Self == null)
                return;

            _nextButtonTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.NextButtonText.ToString());
            _skipTutorialTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.SkipTutorialButtonText.ToString());
        }
    }
}
