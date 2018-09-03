using System.Collections.Generic;

namespace Loom.ZombieBattleground.Data
{
    public class CreditsData
    {
        public List<CreditsInfo> CreditsInfo;

        public CreditsData()
        {
            CreditsInfo = new List<CreditsInfo>();
        }
    }

    public class CreditsInfo
    {
        public string SubsectionType;

        public List<CreditItem> Credits;

        public CreditsInfo()
        {
            Credits = new List<CreditItem>();
        }
    }

    public class CreditItem
    {
        public string FullName;

        public string Post;
    }
}
