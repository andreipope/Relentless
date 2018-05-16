using GrandDevs.CZB.Common;

namespace GrandDevs.CZB.Data
{
    public class UserLocalData
    {
        public Enumerators.Language appLanguage;
        public bool tutorial = true;
        public int lastSelectedDeckId;

        public UserLocalData()
        {
            Reset();
        }

        public void Reset()
        {
            appLanguage = Enumerators.Language.NONE;
            lastSelectedDeckId = -1;
        }
    }
}