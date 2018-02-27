using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using UnityEngine;


namespace GrandDevs.CZB
{
    public class InputManager : IService, IInputManager
    {
        private object _sync = new object();
        
        private List<InputEvent> _inputHandlers;

        private int _customFreeIndex = 0;

        public bool CanHandleInput { get; set; }


        public void Init()
        {
            CanHandleInput = true;

            _inputHandlers = new List<InputEvent>();
        }

        public void Update()
        {
            if (CanHandleInput)
            {
                if (_inputHandlers.Count > 0)
                {
                    lock (_sync)
                    {
                        HandleInput();
                    }
                }
            }
        }

        public void Dispose()
        {
            _inputHandlers.Clear();
        }

        /// <summary>
        /// Registers the input handler by type and code.
        /// </summary>
        /// <returns>The input handler.</returns>
        /// <param name="type">Type.</param>
        /// <param name="inputCode">Input code.</param>
        /// <param name="onInputUp">On input up.</param>
        /// <param name="onInputDown">On input down.</param>
        /// <param name="onInput">On input.</param>
        public int RegisterInputHandler(Enumerators.InputType type, int inputCode, Action onInputUp = null, Action onInputDown = null, Action onInput = null)
        {
            lock (_sync)
            {
                var item = new InputEvent()
                {
                    code = (int)inputCode,
                    OnInputEvent = onInput,
                    OnInputDownEvent = onInputDown,
                    OnInputUpEvent = onInputUp,
                    type = type
                };

                item.index = _customFreeIndex++;

                _inputHandlers.Add(item);

                return item.index;
            }
        }

        public void UnregisterInputHandler(int index)
        {
            lock (_sync)
            {
                var inputHandler = _inputHandlers.Find(x => x.index == index);

                if (inputHandler != null)
                    _inputHandlers.Remove(inputHandler);
            }
        }

        private void HandleInput()
        {
            InputEvent item;
            for (int i = 0; i < _inputHandlers.Count; i++)
            {
                item = _inputHandlers[i];

                switch (item.type)
                {
                    case Enumerators.InputType.MOUSE:
                        {
                            if (Input.GetMouseButton(item.code))
                                item.ThrowOnInputEvent();

                            if (Input.GetMouseButtonUp(item.code))
                                item.ThrowOnInputUpEvent();

                            if (Input.GetMouseButtonDown(item.code))
                                item.ThrowOnInputDownEvent();
                        }
                        break;
                    case Enumerators.InputType.KEYBOARD:
                        {
                            if (Input.GetKey((KeyCode)item.code))
                                item.ThrowOnInputEvent();

                            if (Input.GetKeyUp((KeyCode)item.code))
                                item.ThrowOnInputUpEvent();

                            if (Input.GetKeyDown((KeyCode)item.code))
                                item.ThrowOnInputDownEvent();
                        }
                        break;
                    default: break;
                }
            }
        }
    }

    public class InputEvent
    {
        public int index;
        public int code;

        public Enumerators.InputType type;

        public Action OnInputUpEvent;
        public Action OnInputDownEvent;
        public Action OnInputEvent;

        public void ThrowOnInputUpEvent()
        {
            if (OnInputUpEvent != null)
                OnInputUpEvent();
        }

        public void ThrowOnInputDownEvent()
        {
            if (OnInputDownEvent != null)
                OnInputDownEvent();
        }

        public void ThrowOnInputEvent()
        {
            if (OnInputEvent != null)
                OnInputEvent();
        }
    }
}