// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoomNetwork.CZB
{
    public interface IController
    {
        void Init();
        void Update();
        void Dispose();
    }
}
