using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public interface IPlayerManager
    {
        User LocalUser { get; set; }

        List<BoardUnit> PlayerGraveyardCards { get; set; }

        List<BoardUnit> OpponentGraveyardCards { get; set; }

        void ChangeGoo(int value);

        int GetGoo();
    }
}
