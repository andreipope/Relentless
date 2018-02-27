using System;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public interface IScenesManager
    {
        event Action<Enumerators.AppState> SceneForAppStateWasLoadedEvent;

        Enumerators.AppState CurrentAppStateScene { get; set; }
        int SceneLoadingProgress { get; set; }
        bool IsLoadedScene { get; set; }
        bool IsAutoSceneSwitcher { get; set; }

        void ChangeScene(Common.Enumerators.AppState appState); 
    }
}