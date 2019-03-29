using Loom.ZombieBattleground.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public interface ICardAbility
    {
        BoardUnitModel UnitModelOwner { get; }
        Player PlayerOwner { get; }

        void Init(
            BoardUnitModel boardUnitModel,
            IReadOnlyList<GenericParameter> genericParameters,
            IReadOnlyList<BoardObject> targets = null);
        void DoAction();
        void ChangePlyerOwner(Player player);
        void Dispose();
    }

    public abstract class CardAbility : ICardAbility
    {
        public BoardUnitModel UnitModelOwner { get; private set; }

        public Player PlayerOwner { get; private set; }

        protected IReadOnlyList<BoardObject> Targets { get; private set; }

        protected IReadOnlyList<GenericParameter> GenericParameters { get; private set; }

        public abstract void DoAction();

        public virtual void Init(
            BoardUnitModel boardUnitModel,
            IReadOnlyList<GenericParameter> genericParameters,
            IReadOnlyList<BoardObject> targets = null)
        {
            UnitModelOwner = boardUnitModel;
            PlayerOwner = boardUnitModel.OwnerPlayer;
            GenericParameters = genericParameters;
            Targets = targets;
        }

        public void ChangePlyerOwner(Player player)
        {
            PlayerOwner = player;
        }

        public void Dispose()
        {
        }
    }
}
