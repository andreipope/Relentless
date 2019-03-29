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
        protected IReadOnlyList<BoardObject> Targets { get; private set; }

        protected IReadOnlyList<GenericParameter> GenericParameters { get; private set; }

        protected readonly IGameplayManager GameplayManager;

        protected readonly AbilitiesController AbilitiesController;

        protected readonly BattlegroundController BattlegroundController;
        
        public BoardUnitModel UnitModelOwner { get; private set; }

        public Player PlayerOwner { get; private set; }

        public CardAbility()
        {
            GameplayManager = GameClient.Get<IGameplayManager>();
            AbilitiesController = GameplayManager.GetController<AbilitiesController>();
            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
        }

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
            AbilitiesController.EndAbility(this);
        }
    }
}
