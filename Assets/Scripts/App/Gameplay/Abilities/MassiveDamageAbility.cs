using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class MassiveDamageAbility : AbilityBase
    {
        public int Value;

        public MassiveDamageAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();
            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();
            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            BoardObject caller = (BoardObject) AbilityUnitOwner ?? BoardSpell;

            Player opponent = PlayerCallerOfAbility == GameplayManager.CurrentPlayer ?
                GameplayManager.OpponentPlayer :
                GameplayManager.CurrentPlayer;
            foreach (Enumerators.AbilityTargetType target in AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                        foreach (BoardUnitView cardOpponent in opponent.BoardCards)
                        {
                            BattleController.AttackUnitByAbility(caller, AbilityData, cardOpponent.Model);
                        }

                        AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, opponent.BoardCards.Select(x => (BoardObject)x.Model).ToList(),
                            AbilityData.AbilityType, Protobuf.AffectObjectType.Character);

                        break;
                    case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:
                        foreach (BoardUnitView cardPlayer in PlayerCallerOfAbility.BoardCards)
                        {
                            BattleController.AttackUnitByAbility(caller, AbilityData, cardPlayer.Model);
                        }

                        AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, PlayerCallerOfAbility.BoardCards.Select(x => (BoardObject)x.Model).ToList(),
                            AbilityData.AbilityType, Protobuf.AffectObjectType.Character);

                        break;
                    case Enumerators.AbilityTargetType.OPPONENT:
                        BattleController.AttackPlayerByAbility(caller, AbilityData, opponent);

                        AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                        {
                            opponent
                        },AbilityData.AbilityType, Protobuf.AffectObjectType.Player);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        BattleController.AttackPlayerByAbility(caller, AbilityData, PlayerCallerOfAbility);

                        AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                        {
                            PlayerCallerOfAbility
                        }, AbilityData.AbilityType, Protobuf.AffectObjectType.Player);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }

            InvokeActionTriggered();
        }
    }
}
