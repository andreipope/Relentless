using log4net;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class NavigationManager : IService, INavigationManager
    {
        private static readonly ILog Log = Logging.GetLog(nameof(NavigationManager));

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

            _registeredTabHandlerIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.KEYBOARD,
                (int) KeyCode.Tab, null, OnInputDownTabButton);
        }

        public void Update()
        {
        }

        private void OnInputDownTabButton()
        {
            if (_currentEventSystem.currentSelectedGameObject != null)
            {
                Selectable currentSelectable = _currentEventSystem.currentSelectedGameObject.GetComponent<Selectable>();

                if (currentSelectable != null)
                {
                    Selectable nextSelectable = currentSelectable.FindSelectableOnDown();

                    if (nextSelectable != null)
                    {
                        InputField inputfield = nextSelectable.GetComponent<InputField>();

                        if (inputfield != null)
                        {
                            inputfield.OnPointerClick(
                                new PointerEventData(
                                    _currentEventSystem)); // if it's an input field, also set the text caret
                        }

                        _currentEventSystem.SetSelectedGameObject(nextSelectable.gameObject,
                            new BaseEventData(_currentEventSystem));
                    }
                    else
                    {
#if UNITY_EDITOR
                        Log.Info("next nagivation element not found");
#endif
                    }
                }
            }
        }
    }
}
