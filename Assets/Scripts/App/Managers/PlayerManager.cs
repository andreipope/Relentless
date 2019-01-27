using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class PlayerManager : IService, IPlayerManager
    {
        public User LocalUser { get; set; }

        public UniqueList<BoardUnitView> PlayerGraveyardCards { get; set; }

        public UniqueList<BoardUnitView> OpponentGraveyardCards { get; set; }

        public void ChangeGoo(int value)
        {
            LocalUser.GooValue += value;
        }

        public int GetGoo()
        {
            return LocalUser.GooValue;
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
