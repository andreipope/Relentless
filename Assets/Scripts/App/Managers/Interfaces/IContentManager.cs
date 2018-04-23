using System;
using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using UnityEngine;

namespace GrandDevs.CZB
{
    public interface IContentManager
    {
        List<SpreadsheetModelTutorialInfo> TutorialInfo { get; }
    }
}