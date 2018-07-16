// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



namespace LoomNetwork.CZB
{
    public interface IAppStateManager
    {
        Common.Enumerators.AppState AppState { get; set; }
        void ChangeAppState(Common.Enumerators.AppState stateTo);
        void BackAppState();
    }
}