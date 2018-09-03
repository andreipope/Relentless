using System;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public interface IInputManager
    {
        bool CanHandleInput { get; set; }

        int RegisterInputHandler(
            Enumerators.InputType type, int inputCode, Action onInputUp = null, Action onInputDown = null,
            Action onInput = null);

        void UnregisterInputHandler(int index);
    }
}
