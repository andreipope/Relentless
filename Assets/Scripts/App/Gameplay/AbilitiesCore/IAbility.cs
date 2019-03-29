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

        CardAbilityData CardAbilityData { get; }

        void Init(
            BoardUnitModel boardUnitModel,
            CardAbilityData cardAbilityData,
            IReadOnlyList<BoardObject> targets = null);
        void DoAction();
        void ChangePlayerOwner(Player player);
        void InsertTargets(IReadOnlyList<BoardObject> targets);
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

        public CardAbilityData CardAbilityData { get; private set; }

        public CardAbility()
        {
            GameplayManager = GameClient.Get<IGameplayManager>();
            AbilitiesController = GameplayManager.GetController<AbilitiesController>();
            BattlegroundController = GameplayManager.GetController<BattlegroundController>();
        }

        public abstract void DoAction();

        public virtual void Init(
            BoardUnitModel boardUnitModel,
            CardAbilityData cardAbilityData,
            IReadOnlyList<BoardObject> targets = null)
        {
            UnitModelOwner = boardUnitModel;
            PlayerOwner = boardUnitModel.OwnerPlayer;
            CardAbilityData = cardAbilityData;
            GenericParameters = cardAbilityData.GenericParameters;
            Targets = targets;
        }

        public void ChangePlayerOwner(Player player)
        {
            PlayerOwner = player;
        }

        public void Dispose()
        {
            AbilitiesController.EndAbility(this);
        }

        public void InsertTargets(IReadOnlyList<BoardObject> targets)
        {
            Targets = targets;
        }
    }
}
