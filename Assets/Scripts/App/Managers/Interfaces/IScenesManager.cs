using System;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public interface IScenesManager
    {
        event Action<Enumerators.AppState> SceneForAppStateWasLoadedEvent;

        Enumerators.AppState CurrentAppStateScene { get; set; }

        int SceneLoadingProgress { get; set; }

        bool IsLoadedScene { get; set; }

        void ChangeScene(Enumerators.AppState appState, bool force = false);
    }
}
