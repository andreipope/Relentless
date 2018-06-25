using CCGKit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public interface IPlayerManager
    {
        event Action<int> OnPlayerGraveyardUpdatedEvent;
        event Action<int> OnOpponentGraveyardUpdatedEvent;

        Action OnLocalPlayerSetUp { get; set; }
        Action<BoardCreature> OnBoardCardKilled { get; set; }
        User LocalUser { get; set; }
        List<BoardCreature> PlayerGraveyardCards { get; set; }
        List<BoardCreature> OpponentGraveyardCards { get; set; }
        Player PlayerInfo { get; set; }
        Player OpponentInfo { get; set; }


        void UpdatePlayerGraveyard(int index);
        void UpdateOpponentGraveyard(int index);
    }
}