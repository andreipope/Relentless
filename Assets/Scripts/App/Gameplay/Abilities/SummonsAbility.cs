using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public static Func<float> LessDefThanInOpponentSubTriggerDelay = null;

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

        public override async void Action(object info = null)
        {
            base.Action(info);

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.LessDefThanInOpponent)
            {
                if (PlayerCallerOfAbility.Defense >= GetOpponentOverlord().Defense)
                { 
                    return;                    
                }
                else
                {
                    if(LessDefThanInOpponentSubTriggerDelay != null)
                    {
                        float delay = LessDefThanInOpponentSubTriggerDelay.Invoke();
                        LessDefThanInOpponentSubTriggerDelay = null;
                        await Task.Delay(TimeSpan.FromSeconds(delay));                        
                    }
                }
            }

            InvokeActionTriggered();
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

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
                GameplayManager.CurrentTurnPlayer != PlayerCallerOfAbility)
                return;

            Action();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.END ||
                GameplayManager.CurrentTurnPlayer != PlayerCallerOfAbility)
                return;

            Action();
        }
    }
}
