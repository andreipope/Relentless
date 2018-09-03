using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public interface IContentManager
    {
        List<SpreadsheetModelInfo> TutorialInfo { get; }
        List<SpreadsheetModelInfo> FlavorTextInfo { get; }
    }
}
