// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public interface IScenesManager
    {
        event Action<Enumerators.AppState> SceneForAppStateWasLoadedEvent;

        Enumerators.AppState CurrentAppStateScene { get; set; }

        int SceneLoadingProgress { get; set; }

        bool IsLoadedScene { get; set; }

        bool IsAutoSceneSwitcher { get; set; }

        void ChangeScene(Enumerators.AppState appState);
    }
}
