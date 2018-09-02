// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public interface IAppStateManager
    {
        Enumerators.AppState AppState { get; set; }

        void ChangeAppState(Enumerators.AppState stateTo);

        void BackAppState();
    }
}
