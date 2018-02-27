using System;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.CZB
{
    public class ButtonItem
    {
        public event Action<ButtonItem> SelectButtonItemEvent;

        private IUIManager _uiManager;

        private GameObject _selfObject;

        private Button _selectButton;

        private GameObject _activeStateObject,
                           _inactiveStateObject;

        public bool isSelected;

        public ButtonItem(GameObject self)
        {
            _uiManager = GameClient.Get<IUIManager>();
            _selfObject = self;

            _selectButton = _selfObject.GetComponent<Button>();
            _selectButton.onClick.AddListener(SelectButtonOnClickHandler);

            _activeStateObject = _selfObject.transform.Find("Image_BackgroundActive").gameObject;
            _inactiveStateObject = _selfObject.transform.Find("Image_BackgroundInactive").gameObject;

            Deselect();
        }

        public void Select()
        {
            isSelected = true;

            if (_activeStateObject != null && _inactiveStateObject != null)
            {
                _activeStateObject.SetActive(true);
                _inactiveStateObject.SetActive(false);
            }
        }

        public void Deselect()
        {
            isSelected = false;

            if (_activeStateObject != null && _inactiveStateObject != null)
            {
                _activeStateObject.SetActive(false);
                _inactiveStateObject.SetActive(true);
            }
        }

        private void SelectButtonOnClickHandler()
        {
            if (SelectButtonItemEvent != null)
                SelectButtonItemEvent(this);
        }

        public void Dispose()
        {
            _selfObject = null;
            _activeStateObject = null;
            _inactiveStateObject = null;
            _selectButton.onClick.RemoveAllListeners();
            _selectButton = null;
        }
    }
}