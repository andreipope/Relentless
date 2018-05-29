using GrandDevs.CZB.Common;

namespace GrandDevs.CZB.Data
{
    public class UserLocalData
    {
        public Enumerators.Language appLanguage;
        public bool tutorial = true;
        public int lastSelectedDeckId;
        public bool openedFirstPack = false;

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