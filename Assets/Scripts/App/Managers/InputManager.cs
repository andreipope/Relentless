using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class InputManager : IService, IInputManager
    {
        private readonly object _sync = new object();

        private List<InputEvent> _inputHandlers;

        private int _customFreeIndex;

        public bool CanHandleInput { get; set; }

        /// <summary>
        ///     Registers the input handler by type and code.
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
                InputEvent item = new InputEvent
                {
                    Code = inputCode,
                    OnInputEvent = onInput,
                    OnInputDownEvent = onInputDown,
                    OnInputUpEvent = onInputUp,
                    Type = type
                };

                item.Index = _customFreeIndex++;

                _inputHandlers.Add(item);

                return item.Index;
            }
        }

        public void UnregisterInputHandler(int index)
        {
            lock (_sync)
            {
                InputEvent inputHandler = _inputHandlers.Find(x => x.Index == index);

                if (inputHandler != null)
                {
                    _inputHandlers.Remove(inputHandler);
                }
            }
        }

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

        private void HandleInput()
        {
            InputEvent item;
            for (int i = 0; i < _inputHandlers.Count; i++)
            {
                item = _inputHandlers[i];

                switch (item.Type)
                {
                    case Enumerators.InputType.Mouse:
                    {
                        if (Input.GetMouseButton(item.Code))
                        {
                            item.ThrowOnInputEvent();
                        }

                        if (Input.GetMouseButtonUp(item.Code))
                        {
                            item.ThrowOnInputUpEvent();
                        }

                        if (Input.GetMouseButtonDown(item.Code))
                        {
                            item.ThrowOnInputDownEvent();
                        }
                    }

                        break;
                    case Enumerators.InputType.Keyboard:
                    {
                        if (Input.GetKey((KeyCode)item.Code))
                        {
                            item.ThrowOnInputEvent();
                        }

                        if (Input.GetKeyUp((KeyCode)item.Code))
                        {
                            item.ThrowOnInputUpEvent();
                        }

                        if (Input.GetKeyDown((KeyCode)item.Code))
                        {
                            item.ThrowOnInputDownEvent();
                        }
                    }

                        break;
                }
            }
        }
    }

    public class InputEvent
    {
        public int Index;

        public int Code;

        public Enumerators.InputType Type;

        public Action OnInputUpEvent;

        public Action OnInputDownEvent;

        public Action OnInputEvent;

        public void ThrowOnInputUpEvent()
        {
            OnInputUpEvent?.Invoke();
        }

        public void ThrowOnInputDownEvent()
        {
            OnInputDownEvent?.Invoke();
        }

        public void ThrowOnInputEvent()
        {
            OnInputEvent?.Invoke();
        }
    }
}
