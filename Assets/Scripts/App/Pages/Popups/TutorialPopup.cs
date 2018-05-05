using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GrandDevs.CZB
{
    public class TutorialPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage;

		private TextMeshProUGUI _text;
		private GameObject _yesnoObject;
		private GameObject _nextObject;
        private GameObject _focusedObject;

        private List<GameObject> _focusObjects;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TutorialPopup"));
            _selfPage.transform.SetParent(GameObject.Find("CanvasTutorial").transform, false);

			_text = _selfPage.transform.Find("Description/Text").GetComponent<TextMeshProUGUI>();
            _focusedObject = _selfPage.transform.Find("TutorialFocusObject").gameObject;

			_nextObject = _selfPage.transform.Find("Description/NextButton").gameObject;
			_yesnoObject = _selfPage.transform.Find("Description/Question").gameObject;


			_focusObjects = new List<GameObject>();

            foreach (Transform obj in _selfPage.transform.Find("FocusObjects").transform)
            {
                _focusObjects.Add(obj.gameObject);
            }

			_nextObject.SetActive(false);
			_yesnoObject.SetActive(false);

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
            _text.text = (string)data;

            Show();
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
			_nextObject.SetActive(false);
			_yesnoObject.SetActive(false);
        }

        public void ShowNextButton()
        {
		    _nextObject.SetActive(true);
        }

        public void ShowQuestion()
        {
			_yesnoObject.SetActive(true);

		}

        public void Update()
        {

        }

    }
}