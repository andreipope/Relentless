// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LoomNetwork.CZB
{
    public class TutorialPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private ITutorialManager _tutorialManager;
        private GameObject _selfPage;

		private TextMeshProUGUI _text;
		private GameObject _yesnoObject;
		private ButtonShiftingContent _nextButton, _playButton, _skipButton;
        private GameObject _focusedObject;
		private GameObject _bubbleObject;
		private Image _janeImage;

		private List<GameObject> _focusObjects;
		private Sprite[] _janePoses;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TutorialPopup"));
            //_selfPage.transform.SetParent(GameObject.Find("CanvasTutorial").transform, false);
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

            _bubbleObject = _selfPage.transform.Find("Description").gameObject;

			_text = _selfPage.transform.Find("Description/Text").GetComponent<TextMeshProUGUI>();
            _focusedObject = _selfPage.transform.Find("TutorialFocusObject").gameObject;

            _nextButton = _selfPage.transform.Find("Button_Next").GetComponent<ButtonShiftingContent>();
            _playButton = _selfPage.transform.Find("Button_Play").GetComponent<ButtonShiftingContent>();
            _skipButton = _selfPage.transform.Find("Button_Skip").GetComponent<ButtonShiftingContent>();

            _janeImage = _selfPage.transform.Find("NPC").GetComponent<Image>();

            _nextButton.onClick.AddListener(_tutorialManager.NextButtonClickHandler);
            _playButton.onClick.AddListener(_tutorialManager.NextButtonClickHandler);
            _skipButton.onClick.AddListener(_tutorialManager.SkipTutorial);

            _janePoses = Resources.LoadAll<Sprite>("Images/Tutorial");

			_focusObjects = new List<GameObject>();

            foreach (Transform obj in _selfPage.transform.Find("FocusObjects").transform)
            {
                _focusObjects.Add(obj.gameObject);
            }

            _nextButton.gameObject.SetActive(false);
            _playButton.gameObject.SetActive(false);
            _skipButton.gameObject.SetActive(true);

            Hide();
        }


		public void Dispose()
		{
		}

		public void Hide()
		{
			  _selfPage.SetActive(false);
		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            _selfPage.SetActive(true);
        }

        public void Show(object data)
        {
            if(_tutorialManager.CurrentStep == 22)
            {
                _bubbleObject.SetActive(false);
                _tutorialManager.IsBubbleShow = false;
                GameClient.Get<ITimerManager>().AddTimer(ShowBubble, null, 6f, false);
            }
            _text.text = (string)data;
            Show();
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

        public void SetPosition(Vector2 position)
        {
            _focusedObject.transform.position = new Vector3(position.x, position.y, 0);
        }

        public void ShowTutorialFocus(int step)
        {
            HideTutorialFocus();
            //_focusObjects[step].SetActive(true);
            _selfPage.transform.Find("FocusObjects/Step_" + step).gameObject.SetActive(true);
        }

        public void HideTutorialFocus()
        {
            foreach (var obj in _focusObjects)
                if (obj.activeSelf)
                    obj.SetActive(false);
            _nextButton.gameObject.SetActive(false);
            _playButton.gameObject.SetActive(false);
          //  _skipButton.gameObject.SetActive(false);
        }

        public void ShowNextButton()
        {
            _nextButton.gameObject.SetActive(true);
        }

        public void ShowQuestion()
        {
            _playButton.gameObject.SetActive(true);
          //  _skipButton.gameObject.SetActive(true);
        }

        public void Update()
        {

        }
    }
}