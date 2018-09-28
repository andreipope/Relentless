using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public interface IPlayerManager
    {
        User LocalUser { get; set; }

        List<BoardUnitView> PlayerGraveyardCards { get; set; }

        List<BoardUnitView> OpponentGraveyardCards { get; set; }

        void ChangeGoo(int value);

        int GetGoo();
    }
}
