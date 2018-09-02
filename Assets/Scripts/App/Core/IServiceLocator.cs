// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

namespace LoomNetwork.CZB
{
    public interface IServiceLocator
    {
        T GetService<T>();

        void Update();
    }
}
