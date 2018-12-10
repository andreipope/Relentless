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

        public List<PostItem> Posts;

        public CreditsInfo()
        {
            Posts = new List<PostItem>();
        }
    }

    public class PostItem
    {
        public string Post;

        public List<CreditItem> Credits;

        public PostItem()
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
