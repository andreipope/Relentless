using System.Collections.Generic;
using TMPro;

namespace Loom.ZombieBattleground
{
    public interface ILocalizableUI
    {
        List<TextMeshProUGUI> LocalizedTextList { get; }
    }
}