using Newtonsoft.Json;
using System;
using GrandDevs.CZB.Common;
using UnityEngine;
using System.Collections.Generic;

namespace GrandDevs.CZB
{
    public class PlayerManager : IService, IPlayerManager
    {
        private IDataManager _dataManager;


        public User LocalUser { get; set; }


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