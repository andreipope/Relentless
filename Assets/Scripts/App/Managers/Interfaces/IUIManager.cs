using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public interface IUIManager
    {
        GameObject Canvas { get; set; }

        GameObject Canvas2 { get; set; }

        GameObject Canvas3 { get; set; }

        List<IUIPopup> UiPopups { get; set; }

        void SetPage<T>(bool hideAll = false)
            where T : IUIElement;

        void DrawPopup<T>(object message = null, bool setMainPriority = false)
            where T : IUIPopup;

        void HidePopup<T>()
            where T : IUIPopup;

        T GetPopup<T>()
            where T : IUIPopup;

        T GetPage<T>()
            where T : IUIElement;

        void HideAllPages();
        void HideAllPopups();

        void DrawPopupByName(string name, object data = null);
        void SetPageByName(string name, bool hideAll = false);

        IUIElement CurrentPage {get; set;}
    }
}
