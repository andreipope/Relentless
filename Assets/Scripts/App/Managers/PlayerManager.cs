// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using Newtonsoft.Json;
using System;
using LoomNetwork.CZB.Common;
using UnityEngine;
using System.Collections.Generic;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class PlayerManager : IService, IPlayerManager
    {
        private IDataManager _dataManager;

        public User LocalUser { get; set; }

        public List<BoardUnit> PlayerGraveyardCards { get; set; }
        public List<BoardUnit> OpponentGraveyardCards { get; set; }
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

        public void AddExperience(int value)
        {
            LocalUser.gooValue += value;
            CheckLevel();
        }

        public void AddGoo(int value)
        {

        }

        private void CheckLevel()
        {
            if()
        }

        private void LevelUP()
        {

        }

    }
}