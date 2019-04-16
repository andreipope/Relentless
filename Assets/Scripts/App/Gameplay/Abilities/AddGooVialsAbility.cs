using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AddGooVialsAbility : AbilityBase
    {
        private  int Value { get; } = 1;

        private int Count { get; }

        private int Defense { get; }

        public AddGooVialsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Count = ability.Count;
            Defense = ability.Defense;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            InvokeActionTriggered();
        }

        protected override void UnitDiedHandler()
        {
            if (AbilityTrigger == Enumerators.AbilityTrigger.DEATH)
            {
                InvokeActionTriggered();
            }

            base.UnitDiedHandler();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<Player> players = new List<Player>();

            foreach (Enumerators.Target target in AbilityTargets)
            {
                switch (target)
                {
                    case Enumerators.Target.PLAYER:
                        players.Add(PlayerCallerOfAbility);
                        break;
                    case Enumerators.Target.OPPONENT:
                        players.Add(GetOpponentOverlord());
                        break;
                }
            }
            foreach (Player player in players)
            {
                if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.None)
                {
                    AddGooVials(player);
                }
                else if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.OnlyThisUnitInPlay)
                {
                    if (player.PlayerCardsController.CardsOnBoard.Count == 1 &&
                        player.PlayerCardsController.CardsOnBoard[0] == CardModel)
                    {
                        AddGooVials(player);
                    }
                }
                else if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.LessDefThanInOpponent)
                {
                    if (player.Defense < GetOpponentOverlord(player).Defense)
                    {
                        AddGooVials(player);
                    }
                }
                else if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.OverlordDefenseEqualOrLess)
                {
                    if (player.Defense <= Defense)
                    {
                        AddGooVials(player);
                    }
                }
                else if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.IfHaveFewerUnitsInPlay)
                {
                    if (player.PlayerCardsController.CardsOnBoard.Count < GetOpponentOverlord(player).PlayerCardsController.CardsOnBoard.Count)
                    {
                        AddGooVials(player);
                    }
                }
            }
        }

        private void AddGooVials(Player player)
        {
            if (player.GooVials == player.MaxGooVials)
            {
                for (int i = 0; i < Count; i++)
                {
                    player.PlayerCardsController.AddCardFromDeckToHand();
                }
            }
            else if (player.GooVials == player.MaxGooVials - 1)
            {
                for (int i = 0; i < Count - 1; i++)
                {
                    player.PlayerCardsController.AddCardFromDeckToHand();
                }
            }

            player.GooVials += Value;
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Action();
        }
    }
}
