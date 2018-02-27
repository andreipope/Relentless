using UnityEngine;

namespace GrandDevs.CZB
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