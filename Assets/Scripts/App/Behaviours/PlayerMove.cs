using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class PlayerMove
    {
        private List<IMove> _movesList;

        public PlayerMove()
        {
            _movesList = new List<IMove>();
        }

        public void AddAction(IMove move)
        {
            _movesList.Add(move);
        }

        public void RemoveAction(IMove move)
        {
            if (_movesList.Contains(move))
            {
                _movesList.Remove(move);
            }
        }

    }
}

public interface IMove
{
    Enumerators.PlayerActionType PlayerActionType { get; set; }
}

public class PlayCardOnBoard : IMove
{
    public Enumerators.PlayerActionType PlayerActionType { get; set; }

}
