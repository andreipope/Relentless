using System;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public interface IInputManager
    {
        bool CanHandleInput { get; set; }

        int RegisterInputHandler(Enumerators.InputType type, int inputCode, Action onInputUp = null, Action onInputDown = null, Action onInput = null);
        void UnregisterInputHandler(int index);
    }
}
