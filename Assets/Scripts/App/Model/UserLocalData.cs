// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB.Data
{
    public class UserLocalData
    {
        public Enumerators.Language appLanguage;

        public bool tutorial = true;

        public int lastSelectedDeckId;

        public bool agreedTerms = false;

        public bool openedFirstPack;

        public UserLocalData()
        {
            Reset();
        }

        public void Reset()
        {
            appLanguage = Enumerators.Language.NONE;
            lastSelectedDeckId = -1;
            openedFirstPack = false;
        }
    }
}
