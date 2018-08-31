// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public interface IContentManager
    {
        List<SpreadsheetModelInfo> TutorialInfo { get; }
        List<SpreadsheetModelInfo> FlavorTextInfo { get; }
    }
}