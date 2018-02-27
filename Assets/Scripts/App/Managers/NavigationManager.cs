using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public class NavigationManager : IService, INavigationManager
    {
        private EventSystem _currentEventSystem;
        private IInputManager _inputManager;

        private int _registeredTabHandlerIndex = -1;

        public void Dispose()
        {
            _inputManager.UnregisterInputHandler(_registeredTabHandlerIndex);
        }

        public void Init()
        {
            _currentEventSystem = EventSystem.current;
            _inputManager = GameClient.Get<IInputManager>();

            _registeredTabHandlerIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.KEYBOARD, (int)KeyCode.Tab, null, OnInputDownTabButton, null);
        }

        public void Update()
        {
        }

        private void OnInputDownTabButton()
        {
            if (_currentEventSystem.currentSelectedGameObject != null)
            {
                var currentSelectable = _currentEventSystem.currentSelectedGameObject.GetComponent<Selectable>();

                if (currentSelectable != null)
                {
                    var nextSelectable = currentSelectable.FindSelectableOnDown();

                    if (nextSelectable != null)
                    {
                        var inputfield = nextSelectable.GetComponent<InputField>();

                        if (inputfield != null)
                            inputfield.OnPointerClick(new PointerEventData(_currentEventSystem));  //if it's an input field, also set the text caret

                        _currentEventSystem.SetSelectedGameObject(nextSelectable.gameObject, new BaseEventData(_currentEventSystem));
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.Log("next nagivation element not found");
#endif
                    }
                }
            }
        }
    }
}