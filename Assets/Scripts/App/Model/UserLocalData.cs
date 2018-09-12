using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Data
{
    public class UserLocalData
    {
        public Enumerators.Language AppLanguage;

        public bool Tutorial = true;

        public int LastSelectedDeckId;

        public bool AgreedTerms = false;

        public bool OpenedFirstPack;

        public int CurrentTutorialId = 0;

        public UserLocalData()
        {
            Reset();
        }

        public void Reset()
        {
            AppLanguage = Enumerators.Language.NONE;
            LastSelectedDeckId = -1;
            OpenedFirstPack = false;
            CurrentTutorialId = 0;
        }
    }
}
