using System;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TutorialSkipPopup : IUIPopup
    {
        private const float SpeedFilling = 0.35f;

        public event Action PopupHiding;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private ITutorialManager _tutorialManager;

        private IDataManager _dataManager;

        private ButtonShiftingContent _skipButton;

        public GameObject Self { get; private set; }


        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
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
            UnityEngine.Object.Destroy(Self);
            Self = null;
            PopupHiding?.Invoke();
            PopupHiding = null;
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

            Self = UnityEngine.Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TutorialSkipPopup"));

            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _skipButton = Self.transform.Find("Button_Skip").GetComponent<ButtonShiftingContent>();
            _skipButton.onClick.AddListener(OnClickSkipButtonHandler);
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        private void OnClickSkipButtonHandler()
        {
            _tutorialManager.SkipTutorial();
        }
    }
}
