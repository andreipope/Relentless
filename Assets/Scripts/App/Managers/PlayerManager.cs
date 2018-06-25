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
        public event Action<int> OnPlayerGraveyardUpdatedEvent;
        public event Action<int> OnOpponentGraveyardUpdatedEvent;

        public Action<BoardCreature> OnBoardCardKilled { get; set; }
        public Action OnLocalPlayerSetUp { get; set; }

        private IDataManager _dataManager;

        public User LocalUser { get; set; }

        public List<BoardCreature> PlayerGraveyardCards { get; set; }
        public List<BoardCreature> OpponentGraveyardCards { get; set; }
        public Player PlayerInfo { get; set; }
        public Player OpponentInfo { get; set; }

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

        public void UpdatePlayerGraveyard(int index)
        {
            OnPlayerGraveyardUpdatedEvent?.Invoke(index);
        }
        public void UpdateOpponentGraveyard(int index)
        {
            OnOpponentGraveyardUpdatedEvent?.Invoke(index);
        }
    }
}