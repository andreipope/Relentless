using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public interface IPlayerManager
    {
        User LocalUser { get; set; }
        List<BoardCreature> PlayerGraveyardCards { get; set; }
        List<BoardCreature> OpponentGraveyardCards { get; set; }
        Player PlayerInfo { get; set; }
        Player OpponentInfo { get; set; }
    }
}