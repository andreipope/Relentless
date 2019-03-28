using Loom.ZombieBattleground.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public interface ICardAbility
    {
        BoardUnitModel UnitModelOwner { get; }
        Player PlayerOwner { get; }

        void Init(
            BoardUnitModel boardUnitModel,
            Player playerOwner,
            List<GenericParameter> genericParameters,
            List<BoardObject> targets = null);
        void DoAction();
        void ChangePlyerOwner(Player player);
    }

    public abstract class CardAbility : ICardAbility
    {
        public BoardUnitModel UnitModelOwner { get; private set; }

        public Player PlayerOwner { get; private set; }

        protected List<BoardObject> Targets { get; private set; }

        protected List<GenericParameter> GenericParameters { get; private set; }

        public abstract void DoAction();

        public virtual void Init(
            BoardUnitModel boardUnitModel,
            Player playerOwner,
            List<GenericParameter> genericParameters,
            List<BoardObject> targets = null)
        {
            GenericParameters = genericParameters;
            Targets = targets;
        }

        public void ChangePlyerOwner(Player player)
        {
            PlayerOwner = player;
        }

        protected object GetParameterValue(Enumerators.AbilityParameter abilityParameter)
        {
            return GenericParameters.Find(param => param.AbilityParameter == abilityParameter);
        }

        protected bool HasParameter(Enumerators.AbilityParameter abilityParameter)
        {
            return GenericParameters.Exists(param => param.AbilityParameter == abilityParameter);
        }
    }
}
