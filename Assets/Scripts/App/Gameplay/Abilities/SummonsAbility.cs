using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SummonsAbility : AbilityBase
    {
        public int Count;

        public string Name;

        public List<Enumerators.AbilityTargetType> TargetTypes;

        public SummonsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Name = ability.Name;
            Count = ability.Count;
            TargetTypes = ability.AbilityTargetTypes;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            InvokeUseAbilityEvent();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            foreach (Enumerators.AbilityTargetType target in TargetTypes)
            {
                BoardUnitView unit = null;

                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT:
                        for (int i = 0; i < Count; i++)
                        {
                            unit = CardsController.SpawnUnitOnBoard(GetOpponentOverlord(), Name, ItemPosition.End, IsPVPAbility);
                            if (unit != null)
                            {
                                AddUnitToBoardCards(GetOpponentOverlord(), ItemPosition.End, unit);
                            }
                        }

                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        for (int i = 0; i < Count; i++)
                        {
                            unit = CardsController.SpawnUnitOnBoard(PlayerCallerOfAbility, Name, ItemPosition.End, IsPVPAbility);
                            if (unit != null)
                            {
                                AddUnitToBoardCards(PlayerCallerOfAbility, ItemPosition.End, unit);
                            }
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.TURN ||
                !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            Action();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.END ||
               !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            Action();
        }

        private void AddUnitToBoardCards(Player owner, ItemPosition position, BoardUnitView unit)
        {
            if (owner.IsLocalPlayer)
            {
                BattlegroundController.PlayerBoardCards.Insert(position, unit);
            }
            else
            {
                BattlegroundController.OpponentBoardCards.Insert(position, unit);
            }
        }
    }
}
