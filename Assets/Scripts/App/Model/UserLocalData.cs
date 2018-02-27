using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public class UserLocalData
    {
        public Enumerators.Language appLanguage;


        public UserLocalData()
        {
            Reset();
        }

        public void Reset()
        {
            appLanguage = Enumerators.Language.NONE;
        }
    }
}