using System.Collections.Generic;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class PlayerMoveAction
    {
        private Stack<PlayerMove> _movesList;

        public PlayerMoveAction()
        {
            _movesList = new Stack<PlayerMove>();
        }

        public void AddPlayerMove(PlayerMove move)
        {
            _movesList.Push(move);
        }

        public PlayerMove GetPlayerMove()
        {
            if (_movesList.Count <= 0)
            {
                Debug.LogError("No Moves Left");
                return null;
            }

            return _movesList.Pop();
        }
    }
}

public class PlayerMove
{
    public Enumerators.PlayerActionType PlayerActionType;
    public IMove Move;

    public PlayerMove(Enumerators.PlayerActionType playerActionType, IMove move)
    {
        PlayerActionType = playerActionType;
        Move = move;
    }
}

public interface IMove
{

}

public class PlayCardOnBoard : IMove
{
    public BoardUnitView Unit;
    public int GooCost;

    public PlayCardOnBoard(BoardUnitView unit, int gooCost)
    {
        Unit = unit;
        GooCost = gooCost;
    }
}

public class AttackOverlord : IMove
{
    public BoardUnitModel UnitModel;
    public Player AttackedPlayer;
    public int Damage;

    public AttackOverlord(BoardUnitModel unitModel, Player player, int damage)
    {
        UnitModel = unitModel;
        AttackedPlayer = player;
        Damage = damage;
    }
}


