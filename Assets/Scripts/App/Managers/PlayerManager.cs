// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using Loom.Newtonsoft.Json;
using System;
using LoomNetwork.CZB.Common;
using UnityEngine;
using System.Collections.Generic;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class PlayerManager : IService, IPlayerManager
    {
        public User LocalUser { get; set; }

        public List<BoardUnit> PlayerGraveyardCards { get; set; }
        public List<BoardUnit> OpponentGraveyardCards { get; set; }

        public void Dispose()
        {
        }

        public void Init()
        {
            LocalUser = new User();
        }

        public void Update()
        {
        }

        public void ChangeGoo(int value)
        {
            LocalUser.gooValue += value;
        }

        public int GetGoo()
        {
            return LocalUser.gooValue;
        }
    }
}