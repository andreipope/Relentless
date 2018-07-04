// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;

namespace LoomNetwork.CZB
{
    public interface ITimerManager
    {
        void StopTimer(Action<object[]> handler);
        void AddTimer(Action<object[]> handler, object[] parameters = null, float time = 1, bool loop = false, bool storeTimer = false);
    }
}