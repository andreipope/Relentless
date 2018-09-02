using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB.Data
{
    public class UserLocalData
    {
        public Enumerators.Language AppLanguage;

        public bool Tutorial = true;

        public int LastSelectedDeckId;

        public bool AgreedTerms = false;

        public bool OpenedFirstPack;

        public UserLocalData()
        {
            Reset();
        }

        public void Reset()
        {
            AppLanguage = Enumerators.Language.NONE;
            LastSelectedDeckId = -1;
            OpenedFirstPack = false;
        }
    }
}
