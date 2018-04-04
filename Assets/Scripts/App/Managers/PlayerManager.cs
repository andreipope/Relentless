using Newtonsoft.Json;
using System;
using GrandDevs.CZB.Common;
using UnityEngine;
using System.Collections.Generic;
using GrandDevs.CZB.Data;
using CCGKit;

namespace GrandDevs.CZB
{
    public class PlayerManager : IService, IPlayerManager
    {
        public Action<CCGKit.RuntimeCard> OnBoardCardKilled { get; set; }
        public Action OnLocalPlayerSetUp { get; set; }

        private IDataManager _dataManager;

        public User LocalUser { get; set; }

        public List<BoardCreature> PlayerGraveyardCards { get; set; }
        public List<BoardCreature> OpponentGraveyardCards { get; set; }
        public PlayerInfo playerInfo { get; set; }
        public PlayerInfo opponentInfo { get; set; }

        public void Dispose()
        {
        }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();

            LocalUser = new User();
        }

        public void Update()
        {
        }
    }
}