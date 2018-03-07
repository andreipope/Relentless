using CCGKit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public interface IPlayerManager
    {
        Action OnLocalPlayerSetUp { get; set; }
        Action<CCGKit.RuntimeCard> OnBoardCardKilled { get; set; }
        User LocalUser { get; set; }
        List<BoardCreature> PlayerGraveyardCards { get; set; }
        List<BoardCreature> OpponentGraveyardCards { get; set; }
        PlayerInfo playerInfo { get; set; }
        PlayerInfo opponentInfo { get; set; }
    }
}