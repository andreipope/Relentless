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

        public List<Enumerators.Target> TargetTypes;

        public SummonsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Name = ability.Name;
            Count = ability.Count;
            TargetTypes = ability.Targets;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.LessDefThanInOpponent)
            {
                if (PlayerCallerOfAbility.Defense >= GetOpponentOverlord().Defense)
                    return;
            }

            foreach (Enumerators.Target target in TargetTypes)
            {
                switch (target)
                {
                    case Enumerators.Target.OPPONENT:
                        for (int i = 0; i < Count; i++)
                        {
                            GetOpponentOverlord().PlayerCardsController.SpawnUnitOnBoard(Name, ItemPosition.End, IsPVPAbility);
                        }

                        break;
                    case Enumerators.Target.PLAYER:
                        for (int i = 0; i < Count; i++)
                        {
                            PlayerCallerOfAbility.PlayerCardsController.SpawnUnitOnBoard(Name, ItemPosition.End, IsPVPAbility);
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

            if (AbilityTrigger != Enumerators.AbilityTrigger.TURN ||
                !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            Action();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.END ||
               !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            Action();
        }
    }
}
