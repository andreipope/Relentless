// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;

namespace LoomNetwork.CZB
{
    public interface IUIElement
    {
        void Init();
        void Show();
        void Hide();
        void Update();
        void Dispose();
    }
}