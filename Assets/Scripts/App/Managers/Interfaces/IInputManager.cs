// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public interface IInputManager
    {
        bool CanHandleInput { get; set; }

        int RegisterInputHandler(Enumerators.InputType type, int inputCode, Action onInputUp = null, Action onInputDown = null, Action onInput = null);

        void UnregisterInputHandler(int index);
    }
}
