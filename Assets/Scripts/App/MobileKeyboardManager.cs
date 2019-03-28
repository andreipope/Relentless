using System;
using UnityEngine;


namespace Loom.ZombieBattleground
{
    public class MobileKeyboardManager : IService, IMobileKeyboardManager
    {
        public event Action KeyboardDoneEvent;
        public event Action KeyboardCanceledEvent;
        public event Action<string> KeyboardUpdateEvent;

        private TouchScreenKeyboard _currentKeyboard;

        public float CurrentKeyboardHeight
        {
            get
            {
                if (_currentKeyboard != null)
                    return TouchScreenKeyboard.area.height;
                else
                    return -1f;
            }
        }

        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            if (_currentKeyboard != null)
            {
                if (KeyboardUpdateEvent != null)
                    KeyboardUpdateEvent(_currentKeyboard.text);

                if (!TouchScreenKeyboard.visible && _currentKeyboard.wasCanceled)
                    HideKeyboard(false, true);
                else if (!TouchScreenKeyboard.visible && _currentKeyboard.done && !_currentKeyboard.wasCanceled)
                    HideKeyboard(true, false);
            }
        }

        public void DrawKeyboard(string text, bool hideInput = false, TouchScreenKeyboardType keyboardType = TouchScreenKeyboardType.Default, bool autocorrection = true, bool multiline = false, bool secure = false)
        {
            _currentKeyboard = TouchScreenKeyboard.Open(text, keyboardType, autocorrection, multiline, secure, false, string.Empty);
            TouchScreenKeyboard.hideInput = hideInput;
        }

        public void HideKeyboard(bool throwDoneEvent, bool throwCanceledEvent)
        {
            if (_currentKeyboard == null)
                return;

            _currentKeyboard.active = false;
            _currentKeyboard = null;

            if (throwDoneEvent)
            {
                if (KeyboardDoneEvent != null)
                    KeyboardDoneEvent();
            }

            if (throwCanceledEvent)
            {
                if (KeyboardCanceledEvent != null)
                    KeyboardCanceledEvent();
            }
        }
    }
}
