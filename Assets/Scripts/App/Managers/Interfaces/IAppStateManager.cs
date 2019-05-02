using System;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public interface IAppStateManager
    {
        Enumerators.AppState AppState { get; set; }

        void ChangeAppState(Enumerators.AppState stateTo, bool force = false);

        void SetPausingApp(bool mustPause);

        void BackAppState();

        void Dispose();

        void QuitApplication();

        void HandleNetworkExceptionFlow(Exception exception, bool leaveCurrentAppState = false, bool drawErrorMessage = true);

        Task SendLeaveMatchIfInPlay();
    }
}
