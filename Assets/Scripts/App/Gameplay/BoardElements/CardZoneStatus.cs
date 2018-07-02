using UnityEngine;
using GrandDevs.CZB.Common;


namespace GrandDevs.CZB
{
    public class CardZoneStatus
    {
        public Enumerators.CardZoneType cardZone;
        public int percent;
        public Sprite statusSprite;

        public CardZoneStatus(Enumerators.CardZoneType cardZone, Sprite statusSprite, int percent)
        {
            this.cardZone = cardZone;
            this.statusSprite = statusSprite;
            this.percent = percent;
        }
    }
}