// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using LoomNetwork.CZB.Common;


namespace LoomNetwork.CZB
{
    public class CardZoneOnBoardStatus
    {
        public Enumerators.CardZoneOnBoardType cardZone;
        public int percent;
        public Sprite statusSprite;

        public CardZoneOnBoardStatus(Enumerators.CardZoneOnBoardType cardZone, Sprite statusSprite, int percent)
        {
            this.cardZone = cardZone;
            this.statusSprite = statusSprite;
            this.percent = percent;
        }
    }
}