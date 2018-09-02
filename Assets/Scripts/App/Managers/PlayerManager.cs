// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;

namespace LoomNetwork.CZB
{
    public class PlayerManager : IService, IPlayerManager
    {
        public User LocalUser { get; set; }

        public List<BoardUnit> PlayerGraveyardCards { get; set; }

        public List<BoardUnit> OpponentGraveyardCards { get; set; }

        public void ChangeGoo(int value)
        {
            LocalUser.gooValue += value;
        }

        public int GetGoo()
        {
            return LocalUser.gooValue;
        }

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
    }
}
