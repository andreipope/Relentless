using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB.Data
{
    public class CreditsData
    {
        public List<CreditsInfo> creditsInfo;

        public CreditsData()
        {
            creditsInfo = new List<CreditsInfo>();
        }
    }

    public class CreditsInfo
    {
        public string subsectionType;
        public List<CreditItem> credits;

        public CreditsInfo()
        {
            credits = new List<CreditItem>();
        }
    }

    public class CreditItem
    {
        public string FullName;
        public string Post;

        public CreditItem() { }
    }
}
